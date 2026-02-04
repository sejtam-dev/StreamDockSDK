namespace StreamDockSDK;

/// <summary>
///     Represents an async event handler delegate
/// </summary>
public delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e) where TEventArgs : EventArgs;