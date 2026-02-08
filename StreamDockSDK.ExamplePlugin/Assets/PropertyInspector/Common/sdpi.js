/**
 * StreamDock Property Inspector Common JavaScript Library
 *
 * Provides utilities for creating Property Inspector UIs for StreamDock plugins.
 * Compatible with Elgato Stream Deck API for cross-platform development.
 *
 * @file sdpi.js
 * @version 1.0.0
 * @license MIT
 *
 * @example
 * // In your HTML file:
 * <script src="Common/sdpi.js"></script>
 *
 * // In your JS file:
 * document.addEventListener('DOMContentLoaded', () => {
 *     sdpi.registerSetting('inputId', 'settingKey');
 * });
 */

/** @type {WebSocket|null} WebSocket connection to StreamDock */
let websocket = null;

/** @type {string|null} Property Inspector UUID (used as context) */
let pluginUUID = null;

/** @type {object|null} Action information from StreamDock */
let actionInfo = null;

/** @type {object} Current settings dictionary */
let currentSettings = {};

/** @type {string} Current language code (e.g., 'en', 'cs', 'de', 'zh_CN') */
let currentLanguage = 'en';

/** @type {object} Translations dictionary */
let translations = {};

/**
 * StreamDock entry point - called automatically by StreamDock software
 * Uses Elgato Stream Deck API signature for compatibility
 *
 * This function is called when the Property Inspector is opened.
 * Do NOT call this manually - StreamDock calls it automatically.
 *
 * @param {number} inPort - WebSocket port number
 * @param {string} inPropertyInspectorUUID - UUID for this Property Inspector instance
 * @param {string} inRegisterEvent - Event name to register with
 * @param {string} inInfo - JSON string with StreamDock info
 * @param {string} inActionInfo - JSON string with action information and current settings
 *
 * @example
 * // StreamDock automatically calls:
 * connectElgatoStreamDeckSocket(28196, "uuid-here", "registerPropertyInspector", "{}", "{}");
 */
function connectElgatoStreamDeckSocket(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    pluginUUID = inPropertyInspectorUUID;

    // Parse action info to get context and settings
    actionInfo = JSON.parse(inActionInfo);
    currentSettings = actionInfo.payload?.settings || {};

    // Parse language from inInfo
    try {
        const info = JSON.parse(inInfo);
        currentLanguage = info.application?.language || 'en';
        console.log('[PI] Language detected:', currentLanguage);
    } catch (e) {
        console.warn('[PI] Could not parse language from inInfo, using default: en');
        currentLanguage = 'en';
    }

    console.log('[PI] Connecting to StreamDock:', inPort);
    console.log('[PI] Property Inspector UUID:', pluginUUID);
    console.log('[PI] Action Context:', actionInfo.context);
    console.log('[PI] Action info:', actionInfo);
    console.log('[PI] Current settings:', currentSettings);

    // Load translations
    loadTranslations();

    // Connect WebSocket
    websocket = new WebSocket('ws://127.0.0.1:' + inPort);

    websocket.onopen = function () {
        console.log('[PI] WebSocket connected');

        // Register property inspector
        const json = {
            event: inRegisterEvent,
            uuid: pluginUUID
        };
        websocket.send(JSON.stringify(json));

        // Load current settings into UI
        setTimeout(() => loadSettingsIntoUI(), 100);
    };

    websocket.onerror = function (error) {
        console.error('[PI] WebSocket error:', error);
    };

    websocket.onclose = function () {
        console.log('[PI] WebSocket closed');
    };

    websocket.onmessage = function (evt) {
        try {
            const jsonObj = JSON.parse(evt.data);
            console.log('[PI] Received message:', jsonObj);

            if (jsonObj.event === 'didReceiveSettings') {
                currentSettings = jsonObj.payload?.settings || {};
                console.log('[PI] Settings updated:', currentSettings);
                loadSettingsIntoUI();

                // Trigger custom event
                triggerEvent('settingsUpdated', currentSettings);
            }
        } catch (e) {
            console.error('[PI] Error parsing message:', e);
        }
    };
}

/** @type {object} Map of registered settings (settingKey -> {element, parser}) */
const settingElements = {};

/** @type {object} Event listeners for custom events */
const eventListeners = {};

/**
 * Register a setting element for automatic synchronization
 *
 * Automatically:
 * - Loads initial value from settings
 * - Saves changes to StreamDock
 * - Handles different input types (text, checkbox, select, etc.)
 *
 * @param {string} elementId - ID of the HTML element
 * @param {string} settingKey - Key in the settings object
 * @param {function} [parser=null] - Optional function to parse the value before saving (e.g., parseInt, parseFloat)
 *
 * @example
 * // Text input
 * sdpi.registerSetting('nameInput', 'userName');
 *
 * @example
 * // Number input with parsing
 * sdpi.registerSetting('counterInput', 'startValue', parseInt);
 *
 * @example
 * // Checkbox
 * sdpi.registerSetting('enabledCheckbox', 'isEnabled');
 */
function registerSetting(elementId, settingKey, parser = null) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error(`[PI] Element with id '${elementId}' not found`);
        return;
    }

    console.log(`[PI] Registering setting: ${settingKey} -> ${elementId}`);

    settingElements[settingKey] = {
        element: element,
        parser: parser
    };

    // Add event listener based on element type
    const eventType = element.type === 'checkbox' ? 'change' :
        (element.tagName === 'SELECT' ? 'change' : 'input');

    element.addEventListener(eventType, () => {
        let value = element.type === 'checkbox' ? element.checked : element.value;
        if (parser && value !== null && value !== undefined) {
            value = parser(value);
        }
        console.log(`[PI] Setting changed: ${settingKey} = ${value}`);
        updateSetting(settingKey, value);
    });

    // Load initial value if it exists
    if (currentSettings[settingKey] !== undefined) {
        if (element.type === 'checkbox') {
            element.checked = currentSettings[settingKey];
        } else {
            element.value = currentSettings[settingKey];
        }
    }
}

/**
 * Update a single setting value
 *
 * Updates the setting in memory and immediately saves to StreamDock.
 *
 * @param {string} key - Setting key
 * @param {any} value - Setting value (will be JSON serialized)
 *
 * @example
 * sdpi.updateSetting('counter', 42);
 * sdpi.updateSetting('enabled', true);
 */
function updateSetting(key, value) {
    currentSettings[key] = value;
    saveSettings();
}

/**
 * Load current settings into registered UI elements
 *
 * Called automatically when settings are received from StreamDock.
 * Can be called manually to refresh UI.
 *
 * @private
 */
function loadSettingsIntoUI() {
    console.log('[PI] Loading settings into UI:', currentSettings);

    for (const [key, info] of Object.entries(settingElements)) {
        const value = currentSettings[key];

        if (value === undefined) {
            console.log(`[PI] No value for ${key}, skipping`);
            continue;
        }

        console.log(`[PI] Loading ${key} = ${value}`);

        if (info.element) {
            if (info.element.type === 'checkbox') {
                info.element.checked = value;
            } else {
                info.element.value = value;
            }
        }
    }
}

/**
 * Save current settings to StreamDock
 *
 * Sends all current settings to StreamDock via WebSocket.
 * Settings are persisted and sent to the plugin.
 *
 * @private
 */
function saveSettings() {
    if (websocket && websocket.readyState === WebSocket.OPEN && pluginUUID) {
        const json = {
            event: 'setSettings',
            context: pluginUUID,  // Use Property Inspector UUID as context
            payload: currentSettings
        };
        console.log('[PI] Saving settings:', json);
        websocket.send(JSON.stringify(json));
    } else {
        console.error('[PI] Cannot save settings - websocket not ready or no UUID');
    }
}

/**
 * Send custom data to the plugin
 *
 * Use this to send custom commands or data that isn't part of settings.
 * The plugin receives this via OnSendToPluginAsync() method.
 *
 * @param {object} payload - Data to send (will be JSON serialized)
 *
 * @example
 * // Send a reset command
 * sdpi.sendToPlugin({ action: 'reset' });
 *
 * @example
 * // Send custom data
 * sdpi.sendToPlugin({
 *     action: 'updateValue',
 *     value: 42,
 *     timestamp: Date.now()
 * });
 */
function sendToPlugin(payload) {
    if (websocket && websocket.readyState === WebSocket.OPEN && pluginUUID) {
        const json = {
            event: 'sendToPlugin',
            context: pluginUUID,  // Use Property Inspector UUID as context
            payload: payload
        };
        console.log('[PI] Sending to plugin:', json);
        websocket.send(JSON.stringify(json));
    } else {
        console.error('[PI] Cannot send to plugin - websocket not ready or no UUID');
    }
}

/**
 * Register event listener
 *
 * @param {string} eventName - Name of the event to listen for
 * @param {function} callback - Function to call when event is triggered
 *
 * @example
 * sdpi.on('settingsUpdated', (settings) => {
 *     console.log('Settings changed:', settings);
 * });
 */
function on(eventName, callback) {
    if (!eventListeners[eventName]) {
        eventListeners[eventName] = [];
    }
    eventListeners[eventName].push(callback);
    console.log(`[PI] Registered event listener for: ${eventName}`);
}

/**
 * Trigger custom event
 *
 * @param {string} eventName - Name of the event to trigger
 * @param {any} data - Data to pass to event listeners
 * @private
 */
function triggerEvent(eventName, data) {
    if (eventListeners[eventName]) {
        console.log(`[PI] Triggering event: ${eventName}`, data);
        eventListeners[eventName].forEach(callback => {
            try {
                callback(data);
            } catch (e) {
                console.error(`[PI] Error in event listener for ${eventName}:`, e);
            }
        });
    }
}

// ============================================================================
// i18n (Internationalization) Support
// ============================================================================

/**
 * Load translations for current language
 *
 * Attempts to load translations from:
 * 1. ../Lang/{lang}.json (e.g., ../Lang/cs.json)
 * 2. Falls back to en.json if language file not found
 *
 * @private
 */
function loadTranslations() {
    const langFile = `../Lang/${currentLanguage}.json`;

    console.log(`[PI] Loading translations: ${langFile}`);

    fetch(langFile)
        .then(response => {
            if (!response.ok) {
                // Language file not found, try fallback
                if (currentLanguage !== 'en') {
                    console.warn(`[PI] Translation file not found: ${langFile}, falling back to en.json`);
                    throw new Error('Language file not found');
                } else {
                    console.error('[PI] Even en.json not found!');
                    throw new Error('No translation files available');
                }
            }
            return response.json();
        })
        .catch(error => {
            // Try to load en.json as fallback
            if (currentLanguage !== 'en') {
                console.log('[PI] Loading fallback: ../Lang/en.json');
                return fetch('../Lang/en.json')
                    .then(response => {
                        if (!response.ok) {
                            console.error('[PI] Fallback en.json not found either!');
                            return null;
                        }
                        return response.json();
                    })
                    .catch(e => {
                        console.error('[PI] Error loading fallback en.json:', e);
                        return null;
                    });
            } else {
                console.error('[PI] Error loading en.json:', error);
                return null;
            }
        })
        .then(data => {
            if (data) {
                translations = data;
                console.log('[PI] Translations loaded:', Object.keys(translations).length, 'keys');
                // Apply translations to UI
                applyTranslations();
            } else {
                translations = {};
                console.log('[PI] Using empty translations');
            }
        })
        .catch(error => {
            console.error('[PI] Fatal error loading translations:', error);
            translations = {};
        });
}

/**
 * Get translated string
 *
 * @param {string} key - Translation key (dot notation supported, e.g., 'settings.volume')
 * @param {string} [defaultValue=key] - Default value if translation not found
 * @returns {string} Translated string or default value
 *
 * @example
 * const volumeLabel = sdpi.translate('settings.volume', 'Volume');
 *
 * @example
 * // With nested keys
 * const resetBtn = sdpi.translate('buttons.reset', 'Reset');
 */
function translate(key, defaultValue) {
    if (!key) return defaultValue || '';

    // Support dot notation (e.g., 'settings.volume')
    const keys = key.split('.');
    let value = translations;

    for (const k of keys) {
        if (value && typeof value === 'object' && k in value) {
            value = value[k];
        } else {
            return defaultValue || key;
        }
    }

    return typeof value === 'string' ? value : (defaultValue || key);
}

/**
 * Apply translations to elements with data-i18n attribute
 *
 * Automatically translates all elements with data-i18n attribute.
 *
 * @example
 * <label data-i18n="settings.volume">Volume</label>
 * <button data-i18n="buttons.reset">Reset</button>
 *
 * @private
 */
function applyTranslations() {
    console.log('[PI] Applying translations to UI');

    document.querySelectorAll('[data-i18n]').forEach(element => {
        const key = element.getAttribute('data-i18n');
        const translated = translate(key, element.textContent);

        if (translated !== key) {
            element.textContent = translated;
            console.log(`[PI] Translated ${key} -> ${translated}`);
        }
    });

    // Also translate placeholders
    document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
        const key = element.getAttribute('data-i18n-placeholder');
        const translated = translate(key, element.placeholder);

        if (translated !== key) {
            element.placeholder = translated;
            console.log(`[PI] Translated placeholder ${key} -> ${translated}`);
        }
    });
}

// Export API for use in HTML/JavaScript
/**
 * StreamDock Property Inspector API
 *
 * @namespace sdpi
 * @property {function} registerSetting - Register a setting for automatic sync
 * @property {function} updateSetting - Manually update a setting value
 * @property {function} saveSettings - Manually trigger settings save
 * @property {function} sendToPlugin - Send custom data to the plugin
 * @property {function} on - Register event listener
 * @property {function} translate - Get translated string
 * @property {function} getLanguage - Get current language code
 *
 * @example
 * // Register settings
 * sdpi.registerSetting('input1', 'settingKey1');
 *
 * @example
 * // Update setting manually
 * sdpi.updateSetting('myKey', 'myValue');
 *
 * @example
 * // Send custom command
 * sdpi.sendToPlugin({ action: 'doSomething' });
 *
 * @example
 * // Translate text
 * const text = sdpi.translate('settings.volume', 'Volume');
 */
const sdpi = {
    registerSetting: registerSetting,
    updateSetting: updateSetting,
    saveSettings: saveSettings,
    sendToPlugin: sendToPlugin,
    on: on,
    translate: translate,
    getLanguage: () => currentLanguage
};

