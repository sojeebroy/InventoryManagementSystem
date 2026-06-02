
class SimpleThemeManager {
    constructor() {
        this.storageKey = 'app-theme';
        this.init();
    }

    init() {
        const storedTheme = localStorage.getItem(this.storageKey);
        const theme = storedTheme || this.getSystemTheme();
        this.applyTheme(theme);

        this.setupListeners();

        console.log('Theme Manager initialized:', theme);
    }

    setupListeners() {
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggleTheme();
            });
            console.log('Theme toggle button listener attached');
        } else {
            console.warn('Theme toggle button not found');
        }
    }

    getSystemTheme() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
            ? 'dark'
            : 'light';
    }

    applyTheme(theme) {
        document.body.classList.remove('light-theme', 'dark-theme');

        document.body.classList.add(`${theme}-theme`);

        localStorage.setItem(this.storageKey, theme);

        this.updateThemeIcon(theme);

        console.log('Theme applied:', theme);
    }

    toggleTheme() {
        const currentTheme = document.body.classList.contains('dark-theme') ? 'dark' : 'light';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.applyTheme(newTheme);
    }

    updateThemeIcon(theme) {
        const themeToggle = document.getElementById('themeToggle');
        const themeSpan = document.querySelector('#themeToggle span');
        if (themeSpan) {
            themeSpan.textContent = theme === 'dark' ? '☀️' : '🌙';
        }
        if (themeToggle) {
            themeToggle.title = theme === 'dark' ? 'Switch to Light Theme' : 'Switch to Dark Theme';
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new SimpleThemeManager();
});

