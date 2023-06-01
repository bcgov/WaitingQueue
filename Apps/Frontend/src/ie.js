import "core-js/features/object/entries";
import "core-js/features/array/includes";
import "url-polyfill";
import "promise-polyfill/src/polyfill";
import "whatwg-fetch";
import {
  request,
  wait,
  STORAGE_KEY,
  COOKIE_KEY,
  handleTokenRefresh,
} from "./request.js";

/**
 * @typedef {import("./request.js").Ticket} Ticket
 */

/**
 * @param {Ticket} json
 */
function updatePosition(json) {
  const content = document.getElementById("content");
  const mark = content.getElementsByTagName("mark")[0];
  const timeout = json.checkInAfter * 1000 - Date.now();

  if (mark) {
    mark.innerText = json.queuePosition?.toString();
  }

  wait(timeout).then(function () {
    return refreshTicket(json);
  });
}

/**
 * @param {Ticket} json
 */
function refreshTicket(json) {
  const queuePoller = document.getElementById("poller");
  const url = queuePoller.getAttribute("refresh-url");
  const body = JSON.stringify({
    id: json.id,
    nonce: json.nonce,
    room: json.room,
  });

  return request({
    url,
    fetchOptions: {
      method: "PUT",
      body,
      headers: { "Content-Type": "application/json" },
    },
  }).then(function (json) {
    document.cookie = COOKIE_KEY + "=" + json.token;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(json));
    if (json.queuePosition === 0) {
      var redirectUrl = queuePoller.getAttribute("redirect-url");
      location.assign(redirectUrl);
    } else {
      updatePosition(json);
    }
  });
}

window.handleTokenRefresh = handleTokenRefresh;

let locales = null;
let lang = "en-CA";
function handleLangChange(event) {
  lang = event.target.value;
  document.querySelector("html").setAttribute("lang", lang);
  const els = document.getElementsByTagName("bdi");

  for (var el in els) {
    const key = els[el].getAttribute("data-lang");
    const content = locales[lang][key];
    els[el].textContent = content;
  }
}

function init() {
  document.body.className = "ie";

  if (document.readyState === "complete") {
    const queuePoller = document.getElementById("poller");
    const url = queuePoller.getAttribute("poll-url");
    const room = queuePoller.getAttribute("room");
    const template = document.getElementById("poller-template");
    const errorTemplate = document.getElementById("error-template");
    const content = document.getElementById("content");
    content.innerHTML = template.innerHTML;

    const localesJson = document.getElementById("locales").textContent;
    locales = JSON.parse(localesJson);
    document
      .getElementById("lang-dropdown")
      .addEventListener("change", handleLangChange);

    request({
      url,
      params: {
        room,
      },
    })
      .then(updatePosition)
      .catch(function (err) {
        console.error(err);
        document.querySelector("button[data-retry]").addEventListener(
          "click",
          function () {
            request({
              url,
              params: {
                room,
              },
            }).then(updatePosition);
          },
          {
            once: true,
          }
        );
        content.innerHTML = errorTemplate.innerHTML;
      });
  }
}

document.onreadystatechange = init;
