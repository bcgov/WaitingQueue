module.exports = {
  mode: "production",
  entry: {
    main: __dirname + "/src/main.js",
    ie: __dirname + "/src/ie.js",
  },
  output: {
    path: __dirname + "/dist",
  },
};
