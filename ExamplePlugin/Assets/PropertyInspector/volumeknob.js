document.addEventListener('DOMContentLoaded', () => {
    // Register settings
    sdpi.registerSetting('volume', 'volume', parseInt);
    sdpi.registerSetting('defaultVolume', 'defaultVolume', parseInt);
    sdpi.registerSetting('longPressThreshold', 'longPressThreshold', parseInt);
    sdpi.registerSetting('isMuted', 'isMuted');

    // Update value displays when sliders change
    const volumeSlider = document.getElementById('volume');
    const volumeValue = document.getElementById('volumeValue');
    const defaultVolumeSlider = document.getElementById('defaultVolume');
    const defaultVolumeValue = document.getElementById('defaultVolumeValue');
    const thresholdSlider = document.getElementById('longPressThreshold');
    const thresholdValue = document.getElementById('thresholdValue');

    volumeSlider.addEventListener('input', () => {
        volumeValue.textContent = volumeSlider.value + '%';
    });

    defaultVolumeSlider.addEventListener('input', () => {
        defaultVolumeValue.textContent = defaultVolumeSlider.value + '%';
    });

    thresholdSlider.addEventListener('input', () => {
        thresholdValue.textContent = thresholdSlider.value + 'ms';
    });

    sdpi.on('settingsUpdated', (settings) => {
        volumeValue.textContent = volumeSlider.value + '%';
        defaultVolumeValue.textContent = defaultVolumeSlider.value + '%';
        thresholdValue.textContent = thresholdSlider.value + 'ms';
    });

    // Initialize displays
    volumeValue.textContent = volumeSlider.value + '%';
    defaultVolumeValue.textContent = defaultVolumeSlider.value + '%';
    thresholdValue.textContent = thresholdSlider.value + 'ms';
});
