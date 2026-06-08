

class LanguageManager {
    constructor() {
        this.storageKey = 'app-language';
        this.cookieName = 'X-Culture';
        this.supportedLanguages = ['en', 'es'];
        this.currentLanguage = this.getStoredLanguage() || this.getBrowserLanguage() || 'en';

        this.init();
        this.setupListeners();
    }

init() {
        this.applyLanguage(this.currentLanguage);
        console.log(`Language initialized: ${this.currentLanguage}`);
    }

setupListeners() {
        const languageSelector = document.getElementById('languageSelector');
        if (languageSelector) {
            languageSelector.addEventListener('change', (e) => this.setLanguage(e.target.value));
        }

        document.querySelectorAll('[data-language]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const lang = e.currentTarget.getAttribute('data-language');
                this.setLanguage(lang);
            });
        });
    }

getStoredLanguage() {
        return localStorage.getItem(this.storageKey);
    }

getBrowserLanguage() {
        const browserLang = navigator.language || navigator.userLanguage;
        const shortLang = browserLang.split('-')[0].toLowerCase();
        return this.supportedLanguages.includes(shortLang) ? shortLang : 'en';
    }

applyLanguage(language) {
        if (!this.supportedLanguages.includes(language)) {
            language = 'en';
        }

        this.currentLanguage = language;

        document.documentElement.lang = language;

        localStorage.setItem(this.storageKey, language);

        this.updateLanguageUI(language);

        this.setLanguageCookie(language);

        this.persistLanguageToServer(language);
    }

setLanguage(language) {
        this.applyLanguage(language);
    }

setLanguageCookie(language) {
        const date = new Date();
        date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000));
        const expires = `expires=${date.toUTCString()}`;
        document.cookie = `${this.cookieName}=${language};${expires};path=/`;
    }

updateLanguageUI(language) {
        const languageSelector = document.getElementById('languageSelector');
        if (languageSelector) {
            languageSelector.value = language;
        }

        document.querySelectorAll('[data-language]').forEach(btn => {
            btn.classList.toggle('active', btn.getAttribute('data-language') === language);
        });

        const languageLabel = document.getElementById('languageLabel');
        if (languageLabel) {
            const displayNames = { 'en': 'English', 'es': 'EspaÃ±ol' };
            languageLabel.textContent = displayNames[language] || language;
        }
    }

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
        }
    }

getString(key, params = {}) {
        const element = document.querySelector(`[data-i18n="${key}"]`);
        if (element) {
            return element.textContent;
        }
        return key;
    }

translatePage() {
        document.querySelectorAll('[data-i18n]').forEach(element => {
            const key = element.getAttribute('data-i18n');
        });
    }

getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new LanguageManager();
});

if (document.readyState === 'loading') {
} else {
    new LanguageManager();
}

