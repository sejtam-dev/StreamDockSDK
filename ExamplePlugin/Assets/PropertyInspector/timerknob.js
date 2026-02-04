document.addEventListener('DOMContentLoaded', () => {
    // Register settings
    sdpi.registerSetting('duration', 'duration', parseInt);

    // Update duration display when slider changes
    const durationSlider = document.getElementById('duration');
    const durationValue = document.getElementById('durationValue');
    
    durationSlider.addEventListener('input', () => {
        durationValue.textContent = durationSlider.value + ' min';
    });

    // Update running state checkbox (read-only)
    sdpi.on('settingsUpdated', (settings) => {
        const isRunningCheckbox = document.getElementById('isRunning');
        if (isRunningCheckbox) {
            isRunningCheckbox.checked = settings.isRunning || false;
        }
        
        durationValue.textContent = durationSlider.value + ' min';
    });

    // Reset button handler
    document.getElementById('resetButton').addEventListener('click', () => {
        sdpi.sendToPlugin({
            action: 'resetTimer'
        });
    });

    // Initialize display
    durationValue.textContent = durationSlider.value + ' min';
});
