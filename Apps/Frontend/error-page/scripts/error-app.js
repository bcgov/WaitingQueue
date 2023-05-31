import logo from "../images/bc_logo_header.svg";

const html = String.raw;
const template = document.createElement("template");
template.innerHTML = html`
  <style>
    header {
      background-color: #036;
      border-bottom: 2px solid #fcba19;
      color: #fff;
      display: flex;
      justify-content: center;
      height: 65px;
      top: 0px;
      position: fixed;
      width: 100%;
    }

    @media (min-width: 30em) {
      header {
        padding: 0 65px;
        justify-content: flex-start;
      }
    }

    hgroup {
      display: flex;
      align-items: center;
    }

    header + div {
      padding-top: 64px;
    }

    header img {
      display: inline-block;
      width: 154px;
      height: 43px;
    }

    .center {
      height: calc(100svh - 64px);
      display: grid;
      place-items: center;
    }
  </style>
  <header>
    <hgroup>
      <img role="image" alt="Government of British Columbia" />
    </hgroup>
  </header>
  <div class="center">
    <error-display></error-display>
  </div>
`;

class ErrorApp extends HTMLElement {
  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadowRoot.appendChild(template.content.cloneNode(true));
    this.errorDisplay = this.shadow.querySelector("error-display");
    const action = this.getAttribute("action");
    const message = this.getAttribute("message");
    const title = this.getAttribute("title");
    const url = this.getAttribute("url");
    this.errorDisplay.setAttribute("message", message);
    this.errorDisplay.setAttribute("action", action);
    this.errorDisplay.setAttribute("title", title);
    this.errorDisplay.setAttribute("url", url);
    this.shadow.querySelector("img").src = logo;
  }

  connectedCallback() {}
}

customElements.define("error-app", ErrorApp);
