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
  const languages = supportedLanguages ? supportedLanguages : [];
  const languageSupported = languages.find((l) => {
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
  // @ts-ignore
  const supportedLanguages = window.supportedLanguages || [];
  const language = findUserLanguage(userLanguage, supportedLanguages);
  const currentLanguage = document.querySelector("html").getAttribute("lang");

  if (language && currentLanguage !== language) {
    changeLanguage(language);
  }
  updateLanguageSelect();
}

/**
 * @param {string} lang
 */
export function changeLanguage(lang) {
  document.querySelector("html").setAttribute("lang", lang);
  document.querySelectorAll("queue-poller").forEach((el) => {
    el.setAttribute("lang", lang);
  });
}

/**
 * @param {Event & { target: HTMLSelectElement}} event
 */
export function handleLangChange(event) {
  changeLanguage(event.target.value);
}

export function updateLanguageSelect() {
  const pageLanguage = document.querySelector("html").lang;
  /** @type HTMLSelectElement */
  const langDropdown = document.querySelector("select#lang-dropdown");
  langDropdown.value = pageLanguage;
  langDropdown
    .querySelector(`option[lang="${pageLanguage}"]`)
    .setAttribute("selected", "selected");

  langDropdown.addEventListener("change", handleLangChange);
}
