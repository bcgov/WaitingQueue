import utils from "./utils.js";

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
  const userLanguage = utils.getLanguage();
  /** @type string[] */
  const supportedLanguages = globalThis.supportedLanguages ?? [];
  const language = findUserLanguage(userLanguage, supportedLanguages);
  const currentLanguage = document.querySelector("html")?.getAttribute("lang");

  if (language && currentLanguage !== language) {
    const languageRedirectUrl = new URL(`/${language}`, utils.getLocation());
    utils.open(languageRedirectUrl);
  }
  updateLanguageSelect();
}

/**
 * @param {Event & { target: HTMLSelectElement}} event
 */
function handleLangChange(event) {
  const url = event.target.value;
  utils.open(url);
}

export function updateLanguageSelect() {
  const pageLanguage = document.querySelector("html").lang;
  /** @type HTMLSelectElement */
  const langDropdown = document.querySelector("select#lang-dropdown");
  langDropdown
    .querySelector(`option[lang="${pageLanguage}"]`)
    .setAttribute("selected", "selected");

  langDropdown.addEventListener("change", handleLangChange);
}
