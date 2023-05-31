import icon from "../images/error_icon.svg";

const html = String.raw;
const template = document.createElement("template");
template.innerHTML = html`
  <style>
    .panel {
      text-align: center;
      padding: 0 2em;
    }

    h1 {
      font-size: clamp(1.5em, 6vw, 2.074em);
      margin-block: 0.4em;
    }

    p {
      margin-top: 0;
      margin-bottom: 1.75em;
    }

    img {
      display: inline-block;
      width: clamp(4em, 12vw, 8em);
    }

    .btn-group {
      display: flex;
    }

    .btn-secondary {
      width: 100%;
      background: none;
      border-radius: 4px;
      border: 2px solid #fff;
      padding: 10px 30px;
      text-align: center;
      text-decoration: none;
      display: block;
      font-size: 18px;
      font-family: ‘Noto Sans’, Verdana, Arial, sans-serif;
      font-weight: 700;
      letter-spacing: 1px;
      cursor: pointer;
      color: #fff;
      text-transform: capitalize;
    }

    .btn-secondary:hover {
      background-color: #fff;
      color: #036;
    }

    @media (min-width: 30em) {
      .btn-group {
        justify-content: center;
      }
      .btn-secondary {
        width: auto;
      }
    }
  </style>
  <div class="panel">
    <img role="image" alt="error icon" />
    <h1 data-title></h1>
    <div class="btn-group">
      <a class="btn-secondary"></a>
    </div>
  </div>
`;

class ErrorDisplay extends HTMLElement {
  static get observedAttributes() {
    return ["action", "message", "title", "url"];
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.titleEl = this.shadow.querySelector("[data-title]");
    this.linkEl = this.shadow.querySelector("a.btn-secondary");
    this.shadow.querySelector("img").src = icon;
  }

  connectedCallback() {
    this.linkEl.addEventListener("click", this.handleLinkClick);
  }

  attributeChangedCallback(name, _, value) {
    if (!Boolean(value) || value === "null") {
      return;
    }

    switch (name) {
      case "title":
        this.titleEl.innerText = value;
        break;
      case "message":
        const p = document.createElement("p");
        p.innerText = value;
        this.titleEl.insertAdjacentElement("afterend", p);
        break;
      case "action":
        this.linkEl.innerText = value;
        break;
      case "url":
        this.linkEl.setAttribute("href", value);
        break;
      default:
        break;
    }
  }

  handleLinkClick = (event) => {
    const action = this.getAttribute("action");
    if (action === "back") {
      history.back();
    }
  };
}

customElements.define("error-display", ErrorDisplay);
