const ejs = require("ejs");
const fs = require("fs/promises");
const path = require("path");
const yaml = require("yaml");

const { version } = require("../package.json");

async function loadConfig() {
  const configFile = await fs.readFile("./config.yaml");
  const template = await fs.readFile("./tools/template.ejs");
  const templateCallbackPage = await fs.readFile(
    "./tools/template-callback.ejs"
  );
  const templateRefreshScript = await fs.readFile(
    "./tools/template-refresh.ejs"
  );
  const parsedConfig = yaml.parse(configFile.toString());
  const applications = Object.entries(parsedConfig);

  applications.forEach(([app, config]) => {
    const environments = Object.keys(config.environments);
    config.locales.forEach((locale) => {
      Object.entries(locale).forEach(async ([lang, strings]) => {
        environments.forEach(async (env) => {
          const data = {
            _: strings,
            data: {
              ...config.environments[env],
              env,
              locale: lang,
              version,
            },
          };
          const targetDir = path.join(process.cwd(), "dist", app, env, lang);
          await fs.mkdir(targetDir, {
            recursive: true,
          });
          await fs.writeFile(
            path.join(targetDir, "index.html"),
            ejs.render(template.toString(), data)
          );
          await fs.writeFile(
            path.join(targetDir, "callback.html"),
            ejs.render(templateCallbackPage.toString(), data)
          );
          await fs.writeFile(
            path.join(targetDir, "refresh.js"),
            ejs.render(templateRefreshScript.toString(), data)
          );
        });
      });
    });
  });
}

loadConfig();
