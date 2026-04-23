// MatriuWeb — app.js

window.matriuWeb = {

    // Obre una URL en una nova pestanya
    openTab: (url) => window.open(url, '_blank', 'noopener,noreferrer'),

    // Descàrrega de text com a fitxer
    downloadText: (filename, content) => {
        const blob = new Blob([content], { type: 'application/json' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(a.href);
    },

    // Registra els handlers onload/onerror d'un iframe i notifica .NET
    initFrame: (iframe, dotNetRef) => {
        if (!iframe || !dotNetRef) return;

        // Elimina handlers previs per evitar duplicats en reload
        iframe.onload = null;
        iframe.onerror = null;

        iframe.onload = () => {
            try {
                dotNetRef.invokeMethodAsync('OnIframeLoaded');
            } catch (e) {
                // component ja eliminat
            }
        };

        iframe.onerror = () => {
            try {
                dotNetRef.invokeMethodAsync('OnIframeError');
            } catch (e) {
                // component ja eliminat
            }
        };
    },

    // Comprova si el document del service worker és visible
    isTabVisible: () => !document.hidden
};

// Gestió de visibilitat de pestanya
document.addEventListener('visibilitychange', () => {
    window._mwTabHidden = document.hidden;
});
