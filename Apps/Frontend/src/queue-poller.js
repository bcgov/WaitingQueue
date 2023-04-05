import { request, COOKIE_KEY, STORAGE_KEY } from "./request.js";

/**
 * @typedef {import("./request.js").Ticket} Ticket
 */

/** @type HTMLTemplateElement */
const pollTemplate = document.querySelector("#poller-template");
/** @type HTMLTemplateElement */
const errorTemplate = document.querySelector("#error-template");
/** @type HTMLTemplateElement */
const redirectTemplate = document.querySelector("#redirect-template");

/**
 * Simple element for polling a specified endpoint and updating the UI
 * with the requester's place in line.
 *
 * @property {poll-url} Target API endpoint to poll for queue position.
 * @property {refresh-url} Target API endpoint for refreshing queue position.
 * @property {redirect-url} The location the requester should be redirected to upon success.
 */
class QueuePoller extends HTMLElement {
  /**
   * Sets the polling interval
   * @type number
   * */
  #timer = null;
  /** @type Ticket */
  #_ticket = null;
  /** @type Error */
  #_error = null;

  #redirectPath = null;

  constructor() {
    super();
    this.innerText = "Loading...";
  }

  connectedCallback() {
    const cached = localStorage.getItem(STORAGE_KEY);
    this.replaceChildren(pollTemplate.content.cloneNode(true));

    if (cached) {
      /** @type Ticket */
      const ticket = JSON.parse(cached);
      // If the cached ticket is processed, restart
      if (ticket.status !== "Processed") {
        this.#ticket = ticket;
        return;
      }
      localStorage.removeItem(STORAGE_KEY);
    }

    this.#fetchTicket().then((ticket) => {
      this.#ticket = ticket;
    });
  }

  disconnectedCallback() {
    this.cleanUp();
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
        },
        params: { room },
      });
      return json;
    } catch (err) {
      this.#error = err.message;
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
      localStorage.removeItem(STORAGE_KEY);
      this.#error = err.message;
    }
  };

  #handleProcessed = async () => {
    const redirectUrl = this.getAttribute("redirect-url");
    document.cookie = `${COOKIE_KEY}=${
      this.#ticket.token
    }; path=/; Secure; SameSite=Strict`;
    // await this.#deleteTicket();
    this.cleanUp();
    this.replaceChildren(redirectTemplate.content.cloneNode(true));
    location.assign(redirectUrl + this.#redirectPath);
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

  /**
   * Checks the position based on the API's response
   */
  #updatePosition = () => {
    const mark = this.querySelector("mark");
    if (mark) {
      mark.innerText = this.#ticket.queuePosition?.toString();
    }
  };

  render() {
    this.#redirectPath = window.location.pathname + window.location.search;

    if (this.#error) {
      this.renderError();
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

// export default QueuePoller;

customElements.define("queue-poller", QueuePoller);
