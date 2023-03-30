const path = require("path");

module.exports = {
  entry: __dirname + "/src/ie.js",
  mode: "production",
  output: {
    path: path.resolve(process.cwd(), "dist"),
    filename: "legacy.js",
  },
  target: ["web", "es5"],
  module: {
    rules: [
      {
        test: /\.m?js$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: [
              [
                "@babel/preset-env",
                {
                  targets: {
                    ie: "10",
                  },
                  useBuiltIns: "entry",
                  corejs: "3.29",
                },
              ],
            ],
          },
        },
      },
    ],
  },
};
