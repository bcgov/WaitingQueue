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
 * @property {poll-url} Target API endpoint to poll for line placement
 * @property {frequency} How often the component should check-in with the server
 * @property {redirect-url} The location the requester should be redirected to upon success
 */
class QueuePoller extends HTMLElement {
	/**
	 * Sets the polling interval
	 * @type number
	 * */
	#interval = null;
	/** @type number */
	#spot = 5;

	constructor() {
		super();
		this.replaceChildren(pollTemplate.content.cloneNode(true));
	}

	connectedCallback() {
		const assignedFrequency = this.getAttribute("frequency");
		const pollFrequency = assignedFrequency ? Number(assignedFrequency) : 5000;
		this.#fetchTicket();
		this.#interval = setInterval(this.#fetchTicket, pollFrequency);
	}

	disconnectedCallback() {
		this.cleanUp();
	}

	/**
	 * Request the status of the ticket
	 */
	#fetchTicket = async () => {
		const loadingText = this.querySelector("[data-loading-text]");
		const pollUrl = this.getAttribute("poll-url");

		try {
			loadingText.textContent = "Loading...";
			const req = await fetch(pollUrl);
			const json = await req.json();
			// TODO: Handle JSON response to update place in line, remove manual update of `#spot`
			console.log(json);
			this.#spot = this.#spot - 1;
			loadingText.textContent = "idle";
			this.#updatePosition();
		} catch {
			loadingText.textContent = "error";
			this.replaceChildren(errorTemplate.content.cloneNode(true));
		}
	};

	/**
	 * Checks the position based on the API's response
	 */
	#updatePosition = () => {
		// TODO: Not sure if the updated queue will be boolean or number based. Update accordingly
		if (this.#spot === 1) {
			// TODO: Actually handle redirect here
			const redirectUrl = this.getAttribute("redirect-url");
			console.log("redirecting to ", redirectUrl);
			this.cleanUp();
			this.replaceChildren(redirectTemplate.content.cloneNode(true));
			return;
		}
		this.querySelector("mark").innerText = this.#spot.toString();
	};

	cleanUp() {
		clearInterval(this.#interval);
		this.#interval = null;
	}
}

export default QueuePoller;

customElements.define("queue-poller", QueuePoller);
