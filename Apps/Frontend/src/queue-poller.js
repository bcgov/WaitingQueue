import {
  request,
  IncidentError,
  COOKIE_KEY,
  STORAGE_KEY,
  wait,
} from "./request.js";
import utils from "./utils.js";

/**
 * @typedef {import("./request.js").Ticket} Ticket
 */

/** @type HTMLTemplateElement */
const pollTemplate = document.querySelector("#poller-template");
/** @type HTMLTemplateElement */
const errorTemplate = document.querySelector("#error-template");
/** @type HTMLTemplateElement */
const redirectTemplate = document.querySelector("#redirect-template");
/** @type HTMLTemplateElement */
const noticeTemplate = document.querySelector("#notice-template");

/**
 * Simple element for polling a specified endpoint and updating the UI
 * with the requester's place in line.
 *
 * @property {poll-url} Target API endpoint to poll for queue position.
 * @property {refresh-url} Target API endpoint for refreshing queue position.
 * @property {redirect-url} The location the requester should be redirected to upon success.
 */
class QueuePoller extends HTMLElement {
  static get observedAttributes() {
    return ["lang"];
  }
  /**
   * Sets the polling interval
   * @type number
   * */
  #timer = null;
  /** @type Ticket */
  #_ticket = null;
  /** @type string */
  #_error = null;

  constructor() {
    super();
    this.innerText = "";
  }

  connectedCallback() {
    const cached = localStorage.getItem(STORAGE_KEY);

    if (cached) {
      /** @type Ticket */
      const ticket = JSON.parse(cached);
      // If the cached ticket is processed, restart
      if (ticket.status !== "Processed") {
        this.render();
        this.#ticket = ticket;
        return;
      }
      localStorage.removeItem(STORAGE_KEY);
    }

    this.#fetchTicket().then((ticket) => {
      this.render();
      this.#ticket = ticket;
    });
  }

  disconnectedCallback() {
    this.cleanUp();
  }

  /**
   * @param {string} name
   * @param {string} _
   * @param {string} value
   */
  attributeChangedCallback(name, _, value) {
    if (name === "lang" && value) {
      this.renderLocaleStrings();
    }
  }

  renderLocaleStrings = () => {
    const lang = this.getAttribute("lang");
    const localesJson = document.querySelector("#locales").textContent;
    const locales = JSON.parse(localesJson);
    const locale = locales[lang];

    if (locale) {
      Object.entries(locale).forEach(([key, value]) => {
        const node = document.querySelector(`[data-lang="${key}"]`);
        if (node) {
          node.textContent = value;
        }
      });
    }
  };

  /** @param {boolean} value */
  set #incident(value) {
    this.value = value;
    this.render();
  }

  /** @returns {boolean} value */
  get #incident() {
    return this.value;
  }

  /** @param {Ticket} ticket */
  set #ticket(ticket) {
    this.#_ticket = ticket;
    if (ticket) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(ticket));
      this.#setTimer();
      this.render();
    }
  }

  get #ticket() {
    return this.#_ticket;
  }

  set #error(err) {
    this.#_error = err;
    this.render();
  }

  get #error() {
    return this.#_error;
  }

  /**
   * Request the status of the ticket
   * @returns {Promise<Ticket>} The initial ticket with starting queue position
   */
  #fetchTicket = async () => {
    const pollUrl = this.getAttribute("poll-url");
    const room = this.getAttribute("room");

    try {
      const json = await request({
        url: pollUrl,
        fetchOptions: {
          method: "POST",
          headers: { "Content-Type": "application/json" },
        },
        params: { room },
      });
      return json;
    } catch (err) {
      this.handlerError(err);
    }
  };

  /**
   * This fetch function only runs in intervals
   */
  #refreshTicket = async () => {
    if (!this.#ticket) {
      return;
    }

    const refreshUrl = this.getAttribute("refresh-url");
    const { id, room, nonce } = this.#ticket;
    const body = JSON.stringify({
      id,
      nonce,
      room,
    });

    try {
      const json = await request({
        url: refreshUrl,
        fetchOptions: {
          method: "PUT",
          body,
          headers: { "Content-Type": "application/json" },
        },
      });
      this.#ticket = json;
    } catch (err) {
      this.handlerError(err);
      localStorage.removeItem(STORAGE_KEY);
    }
  };

  /**
   * @param {Error} err
   */
  handlerError = (err) => {
    if (err instanceof IncidentError) {
      this.#incident = true;
    } else {
      this.#error = err.message;
    }
  };

  #handleProcessed = () => {
    const currentUrl = new URL(location.href);
    const redirectUrl = new URL(this.getAttribute("redirect-url"));
    redirectUrl.pathname = (
      redirectUrl.pathname + currentUrl.pathname
    ).replaceAll(/\/{2,}/g, "/");
    currentUrl.searchParams.forEach((value, key) => {
      redirectUrl.searchParams.append(key, value);
    });
    document.cookie = `${COOKIE_KEY}=${
      this.#ticket.token
    }; domain=apps.gov.bc.ca; path=/; Secure; SameSite=Strict`;
    // await this.#deleteTicket();
    this.replaceChildren(redirectTemplate.content.cloneNode(true));
    this.renderLocaleStrings();
    this.cleanUp();
    utils.open(redirectUrl);
  };

  #setTimer = () => {
    const timeout = this.#ticket.checkInAfter * 1000 - Date.now();

    if (timeout > 0) {
      this.#timer = window.setTimeout(this.#refreshTicket, timeout);
    } else {
      this.#refreshTicket();
    }
  };

  /**
   * Handle clean up of the ticket on the API side
   * @returns {Promise<void>} Successful deletion of ticket
   */
  // #deleteTicket = async () => {
  //   const pollUrl = this.getAttribute("poll-url");
  //
  //   try {
  //     const { id, room, nonce } = this.#ticket;
  //     const body = JSON.stringify({
  //       id,
  //       nonce,
  //       room,
  //     });
  //
  //     await request({
  //       url: pollUrl,
  //       fetchOptions: {
  //         method: "DELETE",
  //         body,
  //       },
  //     });
  //     localStorage.removeItem(STORAGE_KEY);
  //   } catch (err) {
  //     this.renderError(err.message ?? "Unable to remove ticket from queue");
  //   }
  // };

  /**
   * Render and error message when an operation failed
   */
  renderError = () => {
    this.replaceChildren(errorTemplate.content.cloneNode(true));
    this.renderLocaleStrings();
    this.querySelector("button").addEventListener(
      "click",
      () => {
        this.#error = null;
        if (this.#ticket) {
          this.#refreshTicket();
        } else {
          this.#fetchTicket().then((ticket) => {
            this.#ticket = ticket;
          });
        }
      },
      {
        once: true,
      }
    );
  };

  renderIncident = () => {
    this.replaceChildren(noticeTemplate.content.cloneNode(true));
    this.renderLocaleStrings();
  };

  /**
   * Checks the position based on the API's response
   */
  #updatePosition = () => {
    const mark = this.querySelector("mark");
    if (mark) {
      mark.innerText = this.#ticket.queuePosition?.toString();
    }
    this.renderLocaleStrings();
  };

  render() {
    this.replaceChildren(pollTemplate.content.cloneNode(true));

    if (this.#error) {
      this.renderError();
      return;
    }

    if (this.#incident) {
      this.renderIncident();
      return;
    }

    if (!this.#ticket) {
      this.textContent = "Loading...";
      return;
    }

    if (this.#ticket.status === "Processed") {
      this.#handleProcessed();
    } else {
      this.#updatePosition();
    }
  }

  cleanUp() {
    clearInterval(this.#timer);
    this.#timer = null;
  }
}

export default QueuePoller;

customElements.define("queue-poller", QueuePoller);
