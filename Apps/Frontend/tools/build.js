const ejs = require("ejs");
const esbuild = require("esbuild");
const fs = require("fs/promises");
const path = require("path");
const webpack = require("webpack");
const yaml = require("yaml");
const { randomUUID } = require("node:crypto");

const webpackConfig = require("../webpack.config");

async function loadConfig() {
  const hash = randomUUID().split("-")[0];
  const configFile = await fs.readFile("./config.yaml");
  const template = await fs.readFile("./tools/template.ejs");
  const parsedConfig = yaml.parse(configFile.toString());
  const applications = Object.entries(parsedConfig);
  const { languages } = await import("./shared.mjs");

  applications.forEach(([app, config]) => {
    const environments = Object.keys(config.environments);
    const supportedLanguages = config.locales
      .map((l) => {
        return Object.keys(l);
      })
      .flat();
    const languageOptions = supportedLanguages.map((lang) => {
      return {
        lang,
        name: languages[lang],
      };
    });
    config.locales.forEach((locale) => {
      Object.entries(locale).forEach(async ([lang, strings]) => {
        environments.forEach(async (env) => {
          const data = {
            _: strings,
            data: {
              ...config.environments[env],
              env,
              locale: lang,
              hash,
              supportedLanguages: supportedLanguages.map((l) => `"${l}"`),
              languageOptions,
            },
          };
          const targetDir = path.join(process.cwd(), "dist", app, env, lang);
          const refreshDir = path.join(process.cwd(), "dist", app, env);
          await fs.mkdir(targetDir, {
            recursive: true,
          });

          // Also write `en-CA` as the root
          if (lang.startsWith("en")) {
            const defaultTargetDir = path.join(process.cwd(), "dist", app, env);
            await fs.writeFile(
              path.join(defaultTargetDir, "index.html"),
              ejs.render(template.toString(), data)
            );
          }

          // Rest of the languages
          await fs.writeFile(
            path.join(targetDir, "index.html"),
            ejs.render(template.toString(), data)
          );

          // Refresh script
          await esbuild.build({
            entryPoints: ["./src/refresh.js"],
            define: { NEW_URL: `"${data.data.refreshUrl}"` },
            bundle: true,
            minify: true,
            format: "esm",
            outfile: `${refreshDir}/refresh.js`,
          });

          // Refresh legacy
          webpack(
            {
              ...webpackConfig,
              entry: path.resolve(__dirname, "..", `/src/refresh.js`),
              output: {
                path: refreshDir,
                filename: "refresh.legacy.js",
              },
            },
            (err, stats) => {
              if (err || stats.hasErrors()) {
                console.error(
                  `Unable to build ${app} - ${env} legacy.refresh.js`
                );
              }
            }
          );
        });
      });
    });
  });
}

loadConfig();
