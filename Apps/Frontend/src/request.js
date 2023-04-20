/**
 * @typedef {object} Ticket
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
 * @typedef {object} CheckInRequest
 * @property {string} id The room ID
 * @property {string} room Room name
 * @property {string} nonce
 */

/**
 * @typedef {object} ProblemDetails
 * @property {string} type
 * @property {string} title
 * @property {number} status HTTP status code
 * @property {string} detail
 * @property {string} instance
 */

export const STORAGE_KEY = "queue-poller.cached";
export const COOKIE_KEY = "WAITINGROOM";

/**
 * @param {number} min
 * @param {number} max
 * @returns {number}
 */
function getRandomArbitrary(min, max) {
  return Math.random() * (max - min) + min;
}

/**
 * @param {Request} request
 * @param {number} delay
 * @returns {Promise<Response>}
 */
async function fetchAndRetryIfNecessary(request, delay = 1) {
  while (true) {
    const response = await fetch(request);
    if (response.status === 429) {
      const waitMS = getRandomArbitrary(
        delay * 1000 * 0.75,
        delay * 1000 * 1.25
      );
      console.warn(`[429] Retry in ${waitMS} milliseconds..`);
      await wait(waitMS);
      delay = delay * 2;
      if (delay >= 64) {
        return response;
      }
    } else {
      return response;
    }
  }
}

/**
 * A simple promise-based timeout
 *
 * @param {number} timeout
 * @returns Promise<void>
 */
export async function wait(timeout = 0) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve();
    }, timeout);
  });
}

/**
 * @param {Object} input The request options input
 * @param {RequestInit} [input.fetchOptions] Add any native `fetch` options here
 * @param {Record<string, string>} [input.params] Any key/value to append to search params.
 * @param {string} input.url The request URL. Can include params, which will be merged with `options.params`.
 * @returns {Promise<Ticket, Error>} A ticket is returned
 */
export async function request(input) {
  const { url, fetchOptions, params } = input;
  try {
    const composedUrl = new URL(url, location.href);

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        composedUrl.searchParams.append(key, value);
      });
    }

    const settings = new Request(composedUrl, fetchOptions);
    const req = await fetchAndRetryIfNecessary(settings);

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
 * Reload the ticket. Available for the redirect URLs
 *
 * @param {Ticket} ticket
 * @param {string} refreshUrl
 * @returns void
 */
export async function refreshToken(ticket, refreshUrl) {
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
    document.cookie = `${COOKIE_KEY}=${json.token}; domain=apps.gov.bc.ca; path=/; Secure; SameSite=Strict`;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(json));

    refreshToken(json, refreshUrl);
  }
}

/**
 * Start a refresh token request for use on redirect callback pages
 * @param {string} refreshUrl
 */
export async function handleTokenRefresh(refreshUrl) {
  try {
    const cached = localStorage.getItem(STORAGE_KEY);
    /** @type Ticket */
    const ticket = JSON.parse(cached);
    await refreshToken(ticket, refreshUrl);
  } catch (err) {
    // TODO: When there is a standard design for this page, handle error messaging in a more helpful way
    console.error(err.message);
    const div = document.createElement("div");
    div.innerText = "Unauthorized WR0001";
    document.body.insertBefore(div, document.body.firstChild);
  }
}
