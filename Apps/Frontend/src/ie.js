import "fetch-polyfill";
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
  var content = document.getElementById("content");
  var mark = content.getElementsByTagName("mark")[0];
  var timeout = json.checkInAfter * 1000 - Date.now();

  if (mark) {
    mark.innerText = json.queuePosition.toString();
  }

  wait(timeout).then(function () {
    return refreshTicket(json);
  });
}

/**
 * @param {Ticket} json
 */
function refreshTicket(json) {
  var queuePoller = document.getElementById("poller");
  var url = queuePoller.getAttribute("refresh-url");
  var body = JSON.stringify({
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

    request({
      url,
      params: {
        room,
      },
    })
      .then(updatePosition)
      .catch(function () {
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
