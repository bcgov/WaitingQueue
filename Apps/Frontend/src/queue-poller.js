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

      throw new Error(error);
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

  #queryParams = null;

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
    document.cookie = `${COOKIE_KEY}=${this.#ticket.token}`;
    // await this.#deleteTicket();
    this.cleanUp();
    this.replaceChildren(redirectTemplate.content.cloneNode(true));
    location.assign(redirectUrl + this.#queryParams);
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
    this.#queryParams = window.location.search;

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

export default QueuePoller;

customElements.define("queue-poller", QueuePoller);

/**
 * A simple promise-based timeout
 *
 * @param {number} timeout
 * @returns Promise<void>
 */
async function wait(timeout = 0) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve();
    }, timeout);
  });
}

/**
 * Reload the ticket. Available for the redirect URLs
 *
 * @param {Ticket} ticket
 * @param {string} refreshUrl
 * @returns void
 */
async function refreshToken(ticket, refreshUrl) {
  const { id, room, nonce, checkInAfter } = ticket;
  const timeout = checkInAfter * 1000 - Date.now();

  // Prevent an infinite loop
  if (timeout <= 0) {
    return;
  }

  await wait(timeout);
  const body = JSON.stringify({
    id,
    nonce,
    room,
  });

  /** @type {Ticket} */
  const json = await request({
    url: refreshUrl,
    fetchOptions: {
      method: "PUT",
      body,
      headers: { "Content-Type": "application/json" },
    },
  });

  if (json) {
    document.cookie = `${COOKIE_KEY}=${json.token}`;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(json));

    refreshToken(json, refreshUrl);
  }
}

/**
 * @param {string} refreshUrl
 */
export async function handleTokenRefresh(refreshUrl) {
  try {
    const cached = localStorage.getItem(STORAGE_KEY);
    /** @type Ticket */
    const ticket = JSON.parse(cached);
    await refreshToken(ticket, refreshUrl);
  } catch {
    // TODO: When there is a standard design for this page, handle error messaging in a more helpful way
    const div = document.createElement("div");
    div.innerText = "Unauthorized WR0001";
    document.body.insertBefore(div, document.body.firstChild);
  }
}
