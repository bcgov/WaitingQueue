// @ts-nocheck
import { assert } from "chai";
import sinon from "sinon";
import utils from "./utils.js";
import * as lang from "./lang.js";

const html = String.raw;

describe("lang", () => {
  /** @type import("sinon").SinonStub **/
  const fixture = document.querySelector("#fixture");
  sinon.stub(utils, "getLocation").returns("http://test.com");

  beforeEach(() => {
    document.querySelector("html").setAttribute("lang", "en-CA");
    fixture.innerHTML = html`<select id="lang-dropdown">
      <option lang="en-CA" value="en-ca">English</option>
      <option lang="fr-CA" value="fr-ca">Français</option>
      <option lang="pa-IN" value="pa-IN">ਪੰਜਾਬੀ</option>
    </select>`;
  });

  afterEach(() => {
    const select = fixture.querySelector("select");
    select.replaceWith(select.cloneNode(true));
    fixture.replaceChildren("");
    document.querySelector("html").setAttribute("lang", "en-CA");
  });

  describe("#findUserLanguage", () => {
    it("should the exact match", () => {
      assert.equal(lang.findUserLanguage("en-CA", ["en-CA"]), "en-CA");
    });

    it("should return match it's case-insensitive match", () => {
      assert.equal(lang.findUserLanguage("en-CA", ["en-ca"]), "en-ca");
      assert.equal(lang.findUserLanguage("en-ca", ["en-CA"]), "en-CA");
    });

    it("should return closest region", () => {
      assert.equal(lang.findUserLanguage("en-US", ["en-CA"]), "en-CA");
      assert.equal(lang.findUserLanguage("en", ["en-CA"]), "en-CA");
    });

    it("should return undefined if not supported", () => {
      assert.isUndefined(lang.findUserLanguage("xx", ["en-CA"]));
      assert.isUndefined(lang.findUserLanguage("fr-FR", ["en-CA"]));
    });
  });

  describe("#checkLanguage", () => {
    it("should do nothing if the language is the same", () => {
      globalThis.supportedLanguages = ["en-CA"];
      lang.checkLanguage();
      assert.equal(
        document.querySelector("html").getAttribute("lang"),
        "en-CA"
      );
    });

    it("should redirect if current language is not supported", () => {
      sinon.stub(utils, "getLanguage").returns("fr-CA");
      globalThis.supportedLanguages = ["en-CA", "fr-CA"];
      lang.checkLanguage();
      assert.equal(
        document.querySelector("html").getAttribute("lang"),
        "fr-CA"
      );
    });
  });

  describe("#updateLanguageSelect", () => {
    beforeEach(() => {
      lang.checkLanguage();
    });

    it("should select the correct language", () => {
      document.querySelector("html").setAttribute("lang", "fr-CA");
      lang.updateLanguageSelect();
      assert.equal(
        fixture
          .querySelector('select option[lang="fr-CA"]')
          .getAttribute("selected"),
        "selected"
      );
    });

    it("should change the language on change", () => {
      /** HTMLSelectElement */
      const select = fixture.querySelector("select");
      select
        .querySelector('option[value="pa-IN"]')
        .setAttribute("selected", "selected");
      select.dispatchEvent(new Event("change"));
      assert.equal(
        document.querySelector("html").getAttribute("lang"),
        "pa-IN"
      );
    });
  });
});
