/**
 * Theme Manager - Handles light/dark theme switching
 * Persists theme preference to localStorage and sends updates to server
 */

class ThemeManager {
    constructor() {
        this.storageKey = 'app-theme';
        this.htmlElement = document.documentElement;
        this.currentTheme = null;
    }

    /**
     * Quick init - apply theme immediately, then fetch server preference
     */
    quickInit() {
        // Get stored theme from localStorage
        const local = this.getStoredTheme();
        const fallback = local || this.getSystemTheme();

        // Apply immediately (don't wait for server)
        this.applyTheme(fallback);

        // Then try to load server preference in background
        this.loadServerThemeInBackground();

        // Setup listeners immediately
        this.setupListeners();
    }

    /**
     * Load server theme in background without blocking UI
     */
    loadServerThemeInBackground() {
        // Use setTimeout to not block initialization
        setTimeout(async () => {
            try {
                const resp = await fetch('/api/user/theme', { method: 'GET' });
                if (resp.ok) {
                    const data = await resp.json();
                    const serverTheme = data?.theme;
                    if (serverTheme && ['light', 'dark', 'auto'].includes(serverTheme)) {
                        // Only update if different from current
                        if (serverTheme !== this.currentTheme) {
                            this.applyTheme(serverTheme);
                        }
                    }
                }
            } catch (err) {
                console.debug('Could not load theme from server:', err);
            }
        }, 0);
    }

    /**
     * Setup event listeners for theme toggle buttons
     */
    setupListeners() {
        // Theme toggle button
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            console.debug('Theme button found, attaching click handler');
            themeToggle.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.toggleTheme();
            });
        } else {
            console.warn('Theme toggle button not found (ID: themeToggle)');
        }

        // Theme dropdown
        const themeSelector = document.getElementById('themeSelector');
        if (themeSelector) {
            themeSelector.addEventListener('change', (e) => this.setTheme(e.target.value));
        }

        // Listen for system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (this.currentTheme === 'auto') {
                    this.applyTheme('auto');
                }
            });
        }
    }

    /**
     * Get theme from localStorage
     */
    getStoredTheme() {
        return localStorage.getItem(this.storageKey);
    }

    /**
     * Get system theme preference
     */
    getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    /**
     * Apply theme to the document
     */
    applyTheme(theme) {
        if (!['light', 'dark', 'auto'].includes(theme)) {
            theme = this.getSystemTheme();
        }

        this.htmlElement.setAttribute('data-theme', theme);
        this.currentTheme = theme;
        localStorage.setItem(this.storageKey, theme);

        console.debug('Theme applied:', theme);

        // Update UI to reflect current theme
        this.updateThemeUI(theme);

        // Persist to server
        this.persistThemeToServer(theme);
    }

    /**
     * Toggle between light and dark themes
     */
    toggleTheme() {
        console.debug('Toggle theme called, current:', this.currentTheme);
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
    }

    /**
     * Set specific theme
     */
    setTheme(theme) {
        this.applyTheme(theme);
    }

    /**
     * Update UI elements to reflect current theme
     */
    updateThemeUI(theme) {
        const themeToggle = document.getElementById('themeToggle');
        const themeIcon = document.getElementById('themeIcon');
        const themeSelector = document.getElementById('themeSelector');

        if (themeToggle) {
            themeToggle.title = theme === 'light' ? 'Switch to Dark Theme' : 'Switch to Light Theme';
            themeToggle.setAttribute('aria-label', themeToggle.title);
        }

        if (themeIcon) {
            themeIcon.className = theme === 'light' ? 'bi bi-moon-fill' : 'bi bi-sun-fill';
        }

        if (themeSelector) {
            themeSelector.value = theme;
        }
    }

    /**
     * Send theme preference to server
     */
    async persistThemeToServer(theme) {
        try {
            const response = await fetch('/api/user/theme', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ theme })
            });

            if (!response.ok) {
                console.debug(`Failed to persist theme to server: ${response.status}`);
            }
        } catch (error) {
            console.debug('Error persisting theme to server:', error);
        }
    }

    /**
     * Get CSRF token for server requests
     */
    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }
}

// Create global instance
let themeManager = null;

// Initialize theme manager on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    console.debug('DOMContentLoaded - initializing ThemeManager');
    themeManager = new ThemeManager();
    themeManager.quickInit();
});

// Also initialize immediately if DOM is already loaded
if (document.readyState !== 'loading') {
    console.debug('DOM already loaded - initializing ThemeManager immediately');
    themeManager = new ThemeManager();
    themeManager.quickInit();
}
