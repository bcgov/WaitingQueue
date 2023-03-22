module.exports = {
  mode: "production",
  entry: {
    legacy: __dirname + "/src/ie.js",
  },
  output: {
    path: __dirname + "/dist",
  },
};
