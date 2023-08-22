import mocha from "mocha";

mocha.setup({ ui: "bdd", global: ["supportedLanguages"] });
mocha.checkLeaks();
Promise.all([
  // import("./queue-poller.test.js"),
  import("./lang.test.js"),
]).then(() => mocha.run());
