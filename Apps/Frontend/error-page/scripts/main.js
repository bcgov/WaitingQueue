import "./error-display.js";
import "./error-app.js";

new EventSource("/esbuild").addEventListener("change", () => location.reload());

const css = String.raw;
const globalStyles = document.createElement("style");
globalStyles.innerText = css`
  body,
  html {
    height: 100%;
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
    overflow: hidden;
  }

  img {
    display: block;
    max-width: 100%;
  }
`;
document.head.appendChild(globalStyles.cloneNode(true));
