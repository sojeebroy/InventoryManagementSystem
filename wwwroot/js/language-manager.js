/**
 * Language Manager - Handles language switching and localization
 * Supports English (en) and Spanish (es)
 */

class LanguageManager {
    constructor() {
        this.storageKey = 'app-language';
        this.cookieName = 'X-Culture';
        this.supportedLanguages = ['en', 'es'];
        this.currentLanguage = this.getStoredLanguage() || this.getBrowserLanguage() || 'en';

        this.init();
        this.setupListeners();
    }

    /**
     * Initialize the language on page load
     */
    init() {
        this.applyLanguage(this.currentLanguage);
        console.log(`Language initialized: ${this.currentLanguage}`);
    }

    /**
     * Setup event listeners for language switcher
     */
    setupListeners() {
        // Language dropdown
        const languageSelector = document.getElementById('languageSelector');
        if (languageSelector) {
            languageSelector.addEventListener('change', (e) => this.setLanguage(e.target.value));
        }

        // Language buttons
        document.querySelectorAll('[data-language]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const lang = e.currentTarget.getAttribute('data-language');
                this.setLanguage(lang);
            });
        });
    }

    /**
     * Get language from localStorage
     */
    getStoredLanguage() {
        return localStorage.getItem(this.storageKey);
    }

    /**
     * Get browser's preferred language
     */
    getBrowserLanguage() {
        const browserLang = navigator.language || navigator.userLanguage;
        const shortLang = browserLang.split('-')[0].toLowerCase();
        return this.supportedLanguages.includes(shortLang) ? shortLang : 'en';
    }

    /**
     * Apply language to the document
     */
    applyLanguage(language) {
        if (!this.supportedLanguages.includes(language)) {
            language = 'en';
        }

        this.currentLanguage = language;

        // Update HTML lang attribute
        document.documentElement.lang = language;

        // Store in localStorage
        localStorage.setItem(this.storageKey, language);

        // Update UI to reflect current language
        this.updateLanguageUI(language);

        // Set cookie for server-side localization
        this.setLanguageCookie(language);

        // Persist to server
        this.persistLanguageToServer(language);
    }

    /**
     * Set language
     */
    setLanguage(language) {
        this.applyLanguage(language);
    }

    /**
     * Set cookie for server-side detection
     */
    setLanguageCookie(language) {
        const date = new Date();
        date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000)); // 1 year
        const expires = `expires=${date.toUTCString()}`;
        document.cookie = `${this.cookieName}=${language};${expires};path=/`;
    }

    /**
     * Update UI elements to reflect current language
     */
    updateLanguageUI(language) {
        const languageSelector = document.getElementById('languageSelector');
        if (languageSelector) {
            languageSelector.value = language;
        }

        // Update active state on language buttons
        document.querySelectorAll('[data-language]').forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-language') === language);
        });

        // Update language indicator
        const languageLabel = document.getElementById('languageLabel');
        if (languageLabel) {
            const displayNames = { 'en': 'English', 'es': 'Español' };
            languageLabel.textContent = displayNames[language] || language;
        }
    }

    /**
     * Send language preference to server
     */
    async persistLanguageToServer(language) {
        try {
            const response = await fetch('/api/user/language', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ language })
            });

            if (!response.ok) {
                console.warn(`Failed to persist language to server: ${response.status}`);
            }
        } catch (error) {
            console.warn('Error persisting language to server:', error);
            // Language is still applied locally even if server persist fails
        }
    }

    /**
     * Get localized string
     * @param {string} key - Resource key (e.g., "Nav.Home")
     * @param {object} params - Optional parameters for interpolation
     */
    getString(key, params = {}) {
        // This would typically fetch from server or localization cache
        // For now, return the key as fallback
        const element = document.querySelector(`[data-i18n="${key}"]`);
        if (element) {
            return element.textContent;
        }
        return key;
    }

    /**
     * Translate all elements with data-i18n attribute
     */
    translatePage() {
        document.querySelectorAll('[data-i18n]').forEach(element => {
            const key = element.getAttribute('data-i18n');
            // Translation would be handled by server or included in page
            // This is a placeholder for manual translation if needed
        });
    }

    /**
     * Get CSRF token for server requests
     */
    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }
}

// Initialize language manager on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    new LanguageManager();
});

// Also initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    // DOM is still loading
} else {
    // DOM is already ready
    new LanguageManager();
}
