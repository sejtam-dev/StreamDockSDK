using System.Reflection;
using log4net;

namespace StreamDockSDK.Actions;

/// <summary>
///     Factory function for creating action handlers with custom dependencies
/// </summary>
/// <param name="connection">StreamDock connection</param>
/// <param name="context">Action instance context</param>
/// <param name="settings">Action settings</param>
/// <returns>Created action handler or null if cannot create</returns>
public delegate ActionHandler? ActionHandlerFactory(StreamDockConnection connection, string context,
    Dictionary<string, object>? settings);

/// <summary>
///     Manages registration, creation, and lifecycle of ActionHandlers
///     Supports automatic discovery via ActionAttribute and custom factories with dependency injection
/// </summary>
public class ActionHandlerManager : IDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ActionHandlerManager));
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, ActionHandlerFactory> _factories = new();

    private readonly Dictionary<string, ActionHandler> _handlers = new();
    private readonly Dictionary<string, Type> _handlerTypes = new();
    private bool _disposed;

    /// <summary>
    ///     Register a custom factory for an action
    ///     Useful for handlers that need custom dependencies (e.g., VoiceMeeter)
    /// </summary>
    /// <param name="actionId">Action identifier</param>
    /// <param name="factory">Factory function to create the handler</param>
    public void RegisterFactory(string actionId, ActionHandlerFactory factory)
    {
        _semaphore.Wait();
        try
        {
            _factories[actionId] = factory;
            Log.Debug($"Registered custom factory for action: {actionId}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Register a handler type with its action ID
    ///     Handler must have a constructor: (StreamDockConnection, string, Dictionary<string, object>?)
    /// </summary>
    /// <typeparam name="T">Handler type</typeparam>
    /// <param name="actionId">Action identifier</param>
    public void RegisterHandler<T>(string actionId) where T : ActionHandler
    {
        _semaphore.Wait();
        try
        {
            _handlerTypes[actionId] = typeof(T);
            Log.Debug($"Registered handler type {typeof(T).Name} for action: {actionId}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Automatically discover and register handlers in an assembly
    ///     Looks for classes with [Action("actionId")] attribute
    /// </summary>
    /// <param name="assembly">Assembly to scan (null = calling assembly)</param>
    public void DiscoverHandlers(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ActionHandler)))
            .ToList();

        _semaphore.Wait();
        try
        {
            foreach (var type in handlerTypes)
            {
                var attributes = type.GetCustomAttributes<ActionAttribute>();
                foreach (var attr in attributes)
                {
                    _handlerTypes[attr.ActionId] = type;
                    Log.Info($"Discovered handler {type.Name} for action: {attr.ActionId}");
                }
            }

            Log.Info($"Discovery complete: found {_handlerTypes.Count} action handlers");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Create or get existing handler for an action instance
    /// </summary>
    /// <param name="actionId">Action identifier</param>
    /// <param name="connection">StreamDock connection</param>
    /// <param name="context">Action instance context</param>
    /// <param name="settings">Action settings</param>
    /// <returns>Created or existing handler, or null if not registered</returns>
    public ActionHandler? GetOrCreateHandler(string actionId, StreamDockConnection connection, string context,
        Dictionary<string, object>? settings)
    {
        Log.Debug($"[HandlerManager] GetOrCreateHandler called for context: {context}, action: {actionId}");

        _semaphore.Wait();
        try
        {
            // Return existing handler if already created
            if (_handlers.TryGetValue(context, out var existingHandler)) return existingHandler;

            // Try custom factory first
            if (_factories.TryGetValue(actionId, out var factory))
                try
                {
                    var handler = factory(connection, context, settings);
                    if (handler != null)
                    {
                        _handlers[context] = handler;
                        Log.Debug($"Created handler via factory for {actionId} (context: {context})");
                        return handler;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error creating handler via factory for {actionId}", ex);
                    return null;
                }

            // Try registered type
            if (_handlerTypes.TryGetValue(actionId, out var handlerType))
                try
                {
                    var handler = (ActionHandler?)Activator.CreateInstance(
                        handlerType,
                        connection,
                        context,
                        settings
                    );

                    if (handler != null)
                    {
                        _handlers[context] = handler;
                        Log.Debug($"Created handler {handlerType.Name} for {actionId} (context: {context})");
                        return handler;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error creating handler {handlerType.Name} for {actionId}", ex);
                    return null;
                }

            Log.Warn($"No handler registered for action: {actionId}");
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Get existing handler by context
    /// </summary>
    public ActionHandler? GetHandler(string context)
    {
        _semaphore.Wait();
        try
        {
            _handlers.TryGetValue(context, out var handler);
            return handler;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Remove handler for a context (called when action disappears)
    /// </summary>
    public void RemoveHandler(string context)
    {
        _semaphore.Wait();
        try
        {
            if (_handlers.TryGetValue(context, out var handler))
            {
                _handlers.Remove(context);
                
                // Dispose handler if it implements IDisposable
                if (handler is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error disposing handler for context: {context}", ex);
                    }
                }
                
                Log.Debug($"Removed handler for context: {context}");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Get all registered handlers
    /// </summary>
    public IEnumerable<ActionHandler> GetAllHandlers()
    {
        _semaphore.Wait();
        try
        {
            return _handlers.Values.ToList(); // Return copy to avoid collection modification issues
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Clear all handlers
    /// </summary>
    public void Clear()
    {
        _semaphore.Wait();
        try
        {
            // Dispose all handlers
            foreach (var handler in _handlers.Values)
            {
                if (handler is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error disposing handler during Clear", ex);
                    }
                }
            }
            
            _handlers.Clear();
            Log.Debug("Cleared all handlers");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Dispose of resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        _semaphore.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}