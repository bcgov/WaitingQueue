/** @type HTMLTemplateElement */
const pollTemplate = document.querySelector("#poller-template");
/** @type HTMLTemplateElement */
const errorTemplate = document.querySelector("#error-template");
/** @type HTMLTemplateElement */
const redirectTemplate = document.querySelector("#redirect-template");

const STORAGE_KEY = "queue-poller.cached";
const COOKIE_KEY = "WAITINGROOM";

/**
 * @typedef {Object} Ticket
 * @property {string} id
 * @property {string=} room The room the ticket belongs to
 * @property {string=} nonce Ticket nonce
 * @property {string} createdTime Created at
 * @property {number} checkInAfter Number of milliseconds until next checkin should be triggered
 * @property {number} tokenExpires Token expiry
 * @property {number} queuePosition The ticket's position in the queue
 * @property {"Processed" | "Queued"} status
 * @property {string=} token
 */

/**
 * @typedef {Object} CheckInRequest
 * @property {string} id The room ID
 * @property {string} room Room name
 * @property {string} nonce
 */

/**
 * @typedef {Object} ProblemDetails
 * @property {string} type
 * @property {string} title
 * @property {number} status HTTP status code
 * @property {string} detail
 * @property {string} instance
 */

/**
 * @param {Object} input The request options input
 * @param {RequestInit} [input.fetchOptions] Add any native `fetch` options here
 * @param {Record<string, string>} [input.params] Any key/value to append to search params.
 * @param {string} input.url The request URL. Can include params, which will be merged with `options.params`.
 * @returns {Promise<Ticket, Error>} A ticket is returned
 */
async function request(input) {
  const { url, fetchOptions, params } = input;
  try {
    const composedUrl = new URL(url, location.href);

    if (params) {
      Object.entries(params).forEach((p) => {
        const [key, value] = p;
        composedUrl.searchParams.append(key, value);
      });
    }

    const req = await fetch(composedUrl, fetchOptions);

    if (!req.ok) {
      let error = req.statusText;
      // Only these types return JSON from the API
      if ([404, 412].includes(req.status)) {
        /** @type {ProblemDetails} */
        const json = await req.json();
        error = json.detail;
      }

      throw error;
    }

    /** @type Ticket */
    const json = await req.json();
    return json;
  } catch (err) {
    throw err;
  }
}
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

  constructor() {
    super();
    this.innerText = "Loading...";
  }

  connectedCallback() {
    const cached = localStorage.getItem(STORAGE_KEY);

    if (cached) {
      this.replaceChildren(pollTemplate.content.cloneNode(true));
      this.#ticket = JSON.parse(cached);
    } else {
      this.#fetchTicket().then((ticket) => {
        this.replaceChildren(pollTemplate.content.cloneNode(true));
        this.#ticket = ticket;
      });
    }
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
      this.#error = err;
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
    // const cookie = JSON.stringify({
    //   id: this.#ticket.id,
    //   room: this.#ticket.room,
    //   nonce: this.#ticket.nonce,
    //   token: this.#ticket.token,
    //   checkInAfter: this.#ticket.checkInAfter
    // });
    document.cookie = `${COOKIE_KEY}=${this.#ticket.token}`;
    // await this.#deleteTicket();
    this.cleanUp();
    this.replaceChildren(redirectTemplate.content.cloneNode(true));
    //localStorage.removeItem(STORAGE_KEY);
    location.assign(redirectUrl);
  };

  #setTimer = () => {
    const timeout = this.#ticket.checkInAfter * 1000 - Date.now();

    if (timeout > 0) {
      this.#timer = setTimeout(this.#refreshTicket, timeout);
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
    this.querySelector("[data-error-text]").textContent =
      this.#error?.name ?? "An error occurred";
    this.querySelector("button").addEventListener(
      "click",
      this.#refreshTicket,
      {
        once: true,
      }
    );
  };

  /**
   * Checks the position based on the API's response
   */
  #updatePosition = () => {
    this.querySelector("mark").innerText =
      this.#ticket.queuePosition?.toString();
  };

  render() {
    if (!this.#ticket) {
      this.textContent = "Loading...";
      return;
    }

    if (this.#error) {
      this.renderError();
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

/**
 * @param {string} refreshUrl
 */
export async function handleTokenRefresh(refreshUrl) {
  try {
    const cached = localStorage.getItem(STORAGE_KEY);

    // const body = document.cookie
    //   .split("; ")
    //   .find((c) => c.startsWith(COOKIE_KEY))
    //   .split("=")[1];
    // /** @type Ticket */

    const ticket = JSON.parse(cached);

    const timeout = ticket.checkInAfter * 1000 - Date.now();

    const refreshToken = async function() {

      const { id, room, nonce } = ticket;
      const body = JSON.stringify({
        id,
        nonce,
        room,
      });
  
      const newTicket = await request({
        url: refreshUrl,
        fetchOptions: {
          method: "PUT",
          body,
          headers: { "Content-Type": "application/json" },
        },
      });
  
      // const cookie = JSON.stringify({
      //   id: newTicket.id,
      //   room: ticket.room,
      //   nonce: ticket.nonce,
      //   token: ticket.token,
      //   checkInAfter: ticket.checkInAfter
      // });
      document.cookie = `${COOKIE_KEY}=${newTicket.token}`;
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newTicket));

      handleTokenRefresh(refreshUrl);
    }

    if (timeout > 0) {
      console.log("Refresh token sleep for " + timeout/1000 + " seconds...");
      setTimeout(refreshToken, timeout);
    } else {
      refreshToken();
    }

  } catch {
    const div = document.createElement("div");
    div.innerText = "Unauthorized WR0001";
    document.body.insertBefore(div, document.body.firstChild);
  }
}
