/**
 * Checks to see if any format of the user's preferred language is supported by the application
 *
 * @param {string} preferredLanguage - The user's preferred language, should be taken from `navigator.language`
 * @param {string[]} supportedLanguages - Languages supported from configuration
 * @returns string | undefined - The supported, preferred language
 */
export function findUserLanguage(preferredLanguage, supportedLanguages) {
  const preferred = preferredLanguage.toLowerCase();
  const languageSupported = supportedLanguages?.find((l) => {
    // strict match, language or <language>-<region>
    if (preferred === l.toLowerCase()) {
      return true;
    }

    // language only
    const preferredLanguageOnly = preferredLanguage.split("-")[0];
    const supportedLanguagesOnly = l.split("-")[0];
    if (supportedLanguagesOnly === preferredLanguageOnly) {
      return true;
    }

    return false;
  });

  return languageSupported;
}

export function checkLanguage() {
  const userLanguage = navigator.language;
  /** @type string[] */
  const supportedLanguages = globalThis.supportedLanguages ?? [];
  const language = findUserLanguage(userLanguage, supportedLanguages);
  const currentLanguage = document.querySelector("html")?.getAttribute("lang");

  if (language && currentLanguage !== language) {
    const languageRedirectUrl = new URL(`/${language}`, location.href);
    location.assign(languageRedirectUrl);
  }
}
