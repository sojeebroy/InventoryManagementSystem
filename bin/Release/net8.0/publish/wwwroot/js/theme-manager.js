

class ThemeManager {
    constructor() {
        this.storageKey = 'app-theme';
        this.htmlElement = document.documentElement;
        this.currentTheme = null;
    }

quickInit() {
        const local = this.getStoredTheme();
        const fallback = local || this.getSystemTheme();

        this.applyTheme(fallback);

        this.loadServerThemeInBackground();

        this.setupListeners();
    }

loadServerThemeInBackground() {
        setTimeout(async () => {
            try {
                const resp = await fetch('/api/user/theme', { method: 'GET' });
                if (resp.ok) {
                    const data = await resp.json();
                    const serverTheme = data?.theme;
                    if (serverTheme && ['light', 'dark', 'auto'].includes(serverTheme)) {
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

setupListeners() {
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

        const themeSelector = document.getElementById('themeSelector');
        if (themeSelector) {
            themeSelector.addEventListener('change', (e) => this.setTheme(e.target.value));
        }

        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (this.currentTheme === 'auto') {
                    this.applyTheme('auto');
                }
            });
        }
    }

getStoredTheme() {
        return localStorage.getItem(this.storageKey);
    }

getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

applyTheme(theme) {
        if (!['light', 'dark', 'auto'].includes(theme)) {
            theme = this.getSystemTheme();
        }

        this.htmlElement.setAttribute('data-theme', theme);
        this.currentTheme = theme;
        localStorage.setItem(this.storageKey, theme);

        console.debug('Theme applied:', theme);

        this.updateThemeUI(theme);

        this.persistThemeToServer(theme);
    }

toggleTheme() {
        console.debug('Toggle theme called, current:', this.currentTheme);
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
    }

setTheme(theme) {
        this.applyTheme(theme);
    }

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

getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }
}

let themeManager = null;

document.addEventListener('DOMContentLoaded', () => {
    console.debug('DOMContentLoaded - initializing ThemeManager');
    themeManager = new ThemeManager();
    themeManager.quickInit();
});

if (document.readyState !== 'loading') {
    console.debug('DOM already loaded - initializing ThemeManager immediately');
    themeManager = new ThemeManager();
    themeManager.quickInit();
}

