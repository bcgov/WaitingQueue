export default {
  /** @param {string | URL} url */
  open(url) {
    location.assign(url);
  },
  /** @returns {string} */
  getLocation() {
    return location.href;
  },
  /** @returns {string} */
  getLanguage() {
    return navigator.language;
  },
};
