import logoImage from "../images/gov3_bc_logo.png";
import iconImage from "../images/error_icon.png";

var configEl = document.getElementsByTagName("error-app")[0];
var title = configEl.getAttribute("title");
var message = configEl.getAttribute("message");
var action = configEl.getAttribute("action");
var url = configEl.getAttribute("url");

// set up styles
var globalStyles = document.createElement("style");
globalStyles.type = "text/css";
globalStyles.textContent = `
  body,
  html {
    padding: 0;
    margin: 0;
    font-family: "BCSans", "Noto Sans", Verdana, Arial, sans-serif;
  }

  * {
    box-sizing: border-box;
  }

  body {
    background-color: #003366;
    color: white;
  }

  img {
    display: inline-block;
    max-width: 100%;
  }

  header {
    background-color: #036;
    border-bottom: 2px solid #fcba19;
    padding: 0 65px 0 65px;
    color: #fff;
    display: flex;
    height: 65px;
    top: 0px;
    position: fixed;
    width: 100%;
  }

  hgroup {
    display: flex;
    align-items: center;
  }

  header img {
    width: 154px;
    height: 43px;
  }

  header + div {
    padding-top: 64px;
  }

  .center {
    min-height: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    padding: 250px 0;
  }

  .panel {
    text-align: center;
  }

  h1 {
    font-size: 2.074em;
  }

  p {
    margin-bottom: 2.074em;
  }

  .btn-group {
    display: flex;
    justify-content: center;
  }

  .btn-secondary {
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
`;
document.head.appendChild(globalStyles);

// Header
var header = document.createElement("header");
var hgroup = document.createElement("hgroup");
var logo = new Image();
logo.src = logoImage;
logo.alt = "Government of British Columbia logo";
logo.title = "Government of British Columbia";
hgroup.appendChild(logo);
header.appendChild(hgroup);
document.body.appendChild(header);

// Main element
var panel = document.createElement("div");
panel.className = "panel";

var icon = new Image();
icon.src = iconImage;
icon.role = "image";
panel.appendChild(icon);

var titleEl = document.createElement("h1");
titleEl.textContent = title;
panel.appendChild(titleEl);

if (message) {
  var messageEl = document.createElement("p");
  messageEl.textContent = message;
  panel.appendChild(messageEl);
}

var btnGroup = document.createElement("div");
btnGroup.className = "btn-group";

var button = document.createElement("a");
button.className = "btn-secondary";
button.textContent = action;

if (action === "continue") {
  button.href = url;
}

if (action === "back") {
  button.addEventListener("click", function () {
    history.back();
  });
}

btnGroup.appendChild(button);
panel.appendChild(btnGroup);

var element = document.createElement("div");
element.className = "center";
element.appendChild(panel);
document.body.appendChild(element);
