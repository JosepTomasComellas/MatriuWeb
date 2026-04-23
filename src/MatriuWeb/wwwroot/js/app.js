// MatriuWeb — app.js

window.matriuWeb = {
    openTab: (url) => window.open(url, '_blank'),

    downloadText: (filename, content) => {
        const blob = new Blob([content], { type: 'application/json' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
        URL.revokeObjectURL(a.href);
    }
};

// Pausar refresh quan la pestanya no és visible
document.addEventListener('visibilitychange', () => {
    window._tabHidden = document.hidden;
});
