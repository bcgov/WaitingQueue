class LoadingSpinner extends HTMLElement {
  constructor() {
    super();
    const el = document.createElement("div");
    let totalChildren = 4;
    el.className = "spinner";
    el.ariaHidden = "true";

    while (totalChildren > 0) {
      el.appendChild(document.createElement("div"));
      totalChildren--;
    }
    this.appendChild(el);
  }

  connectedCallback() {
    const color = this.getAttribute("color");
    if (color) {
      this.setAttribute("style", `--spinner-color: ${color};`);
    }
  }
}

customElements.define("loading-spinner", LoadingSpinner);
