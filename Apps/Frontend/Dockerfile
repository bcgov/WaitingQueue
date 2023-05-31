FROM node:19.8.1-alpine3.16 AS BUILD

WORKDIR /source
COPY package.json package.json
COPY package-lock.json package-lock.json
RUN npm install
COPY . .
RUN npm run build
RUN touch /source/dist/index.html

##################################################
## Error Page UI Component Build
FROM node:19.8.1-alpine3.16 AS BUILD_ERR_PAGE

COPY error-page/ /error-page
RUN cd /error-page && ./build.sh

##########
## RUNTIME
FROM nginxinc/nginx-unprivileged
COPY --from=BUILD /source/dist /usr/share/nginx/html
COPY health.html /usr/share/nginx/html/health.html

COPY --from=BUILD_ERR_PAGE /error-page/images /usr/share/nginx/html/_ep/images
COPY --from=BUILD_ERR_PAGE /error-page/error-page.js /usr/share/nginx/html/_ep/error-page.js
COPY --from=BUILD_ERR_PAGE /error-page/error-page.legacy.js /usr/share/nginx/html/_ep/error-page.legacy.js
