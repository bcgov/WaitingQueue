import * as esbuild from "esbuild";
import path from "path";
import http from "node:http";
import fs from "fs/promises";
import ejs from "ejs";
import yaml from "yaml";

import { languages } from "../tools/shared.mjs";

async function renderTemplate(config, lang) {
  const template = await fs.readFile("./tools/template.ejs");
  const supportedLanguages = Object.keys(config.locales);
  const languageOptions = supportedLanguages.map((lang) => {
    return {
      lang,
      name: languages[lang],
    };
  });
  const data = {
    _: config.locales["en-CA"],
    data: {
      ...config.settings,
      locales: JSON.stringify(config.locales),
      env: "local",
      hash: "",
      supportedLanguages: supportedLanguages.map((l) => `"${l}"`),
      languageOptions,
    },
  };
  return ejs.render(template.toString(), data);
}

// Taken directly from esbuild's docs https://esbuild.github.io/api/#serve-proxy
const files = await fs.readdir("./src");
let ctx = await esbuild.context({
  entryPoints: files
    .filter((f) => f.endsWith("js") || f.endsWith("css"))
    .map((f) => `./src/${f}`),
  bundle: true,
  define: { TEST: "true" },
  format: "esm",
  external: ["mocha", "sinon"],
  outdir: "www",
  target: ["safari14"],
});

let { host, port } = await ctx.serve({ servedir: "./www" });

/**
 * @param {string} url
 * @returns {string?}
 */
// function getLanguage(url, config) {
//   if (url === "/") {
//     return "en-CA";
//   }
//   const start = url.split("/").filter(Boolean)[0];
//   const lang = /\w{2}(-\w{2,3})?/i.exec(start);
//   if (lang) {
//     if (config.locales[lang[0]]) {
//       return lang;
//     }
//   }
//   return undefined;
// }

// Start proxy server
http
  .createServer(async (req, res) => {
    const configFile = await fs.readFile("./www/config.dev.yaml");
    const config = yaml.parse(configFile.toString());
    const options = {
      hostname: host,
      port: port,
      path: req.url,
      method: req.method,
      headers: req.headers,
    };

    if (req.url === "/") {
      const html = await renderTemplate(config);
      res.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
      res.end(html);
      return;
    }

    const proxyReq = http.request(options, async (proxyRes) => {
      // esbuild's 404 is where we handle responses
      if (proxyRes.statusCode === 404) {
        res.writeHead(404, { "Content-Type": "text/html" });
        res.end("404 not found");
        return;
      }

      // Otherwise, forward the response from esbuild to the client
      res.writeHead(proxyRes.statusCode, proxyRes.headers);
      proxyRes.pipe(res, { end: true });
    });

    // esbuild result
    req.pipe(proxyReq, { end: true });
  })
  .listen(8001);
