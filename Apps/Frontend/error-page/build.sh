#!/bin/sh
npx -y esbuild@0.17.18 scripts/main.js --bundle --minify \
--loader:.svg=dataurl \
--target=es2020 \
--format=esm \
--outfile=error-page.js 
npx -y esbuild@0.17.18 scripts/legacy.js --bundle --minify \
--loader:.png=dataurl \
--target=es2015 \
--supported:template-literal=false \
--supported:arrow=false \
--outfile=error-page.legacy.js 

echo "Build complete."
