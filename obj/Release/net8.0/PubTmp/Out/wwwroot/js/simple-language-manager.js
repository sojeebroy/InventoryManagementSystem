

class SimpleLanguageManager {
    constructor() {
        this.storageKey = 'app-language';
        this.supportedLanguages = ['en', 'es'];
        this.init();
    }

    init() {
        const storedLanguage = localStorage.getItem(this.storageKey);
        const browserLang = this.getBrowserLanguage();
        const currentLanguage = storedLanguage || browserLang || 'en';

        this.setCurrentLanguage(currentLanguage);
        this.setupListeners();

        console.log('Language Manager initialized:', currentLanguage);
    }

    setupListeners() {
        const enBtn = document.getElementById('languageToggleEn');
        if (enBtn) {
            enBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.changeLanguage('en');
            });
        }

        const esBtn = document.getElementById('languageToggleEs');
        if (esBtn) {
            esBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.changeLanguage('es');
            });
        }

        console.log('Language buttons listeners attached');
    }

    getBrowserLanguage() {
        const lang = navigator.language || navigator.userLanguage;
        const langCode = lang.split('-')[0].toLowerCase();
        return this.supportedLanguages.includes(langCode) ? langCode : 'en';
    }

    setCurrentLanguage(language) {
        if (!this.supportedLanguages.includes(language)) {
            language = 'en';
        }

        localStorage.setItem(this.storageKey, language);
        this.updateLanguageUI(language);
    }

    changeLanguage(language) {
        if (!this.supportedLanguages.includes(language)) {
            return;
        }

        localStorage.setItem(this.storageKey, language);

        this.setCultureCookie(language);

        this.updateLanguageUI(language);

        setTimeout(() => {
            window.location.reload();
        }, 200);

        console.log('Language changed to:', language);
    }

    updateLanguageUI(language) {
        const enBtn = document.getElementById('languageToggleEn');
        const esBtn = document.getElementById('languageToggleEs');

        if (enBtn) {
            enBtn.classList.toggle('active', language === 'en');
        }
        if (esBtn) {
            esBtn.classList.toggle('active', language === 'es');
        }
    }

    setCultureCookie(language) {
        const cultureValue = `c=${language}|uic=${language}`;
        document.cookie = `.AspNetCore.Culture=${cultureValue}; path=/; max-age=31536000`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new SimpleLanguageManager();
});

