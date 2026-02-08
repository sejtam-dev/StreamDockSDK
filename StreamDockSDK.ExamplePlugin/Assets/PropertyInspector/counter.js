document.addEventListener('DOMContentLoaded', () => {
    // Register settings
    sdpi.registerSetting('startValue', 'startValue', parseInt);
    sdpi.registerSetting('increment', 'increment', parseInt);
    sdpi.registerSetting('resetOnAppear', 'resetOnAppear');

    // Reset button handler
    document.getElementById('resetBtn').addEventListener('click', () => {
        // Send custom message to plugin to reset counter
        sdpi.sendToPlugin({
            action: 'resetCounter'
        });
    });
});

