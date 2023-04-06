FROM node:19.8.1-alpine3.16 AS BUILD

WORKDIR /source
COPY package.json package.json
COPY package-lock.json package-lock.json
RUN npm install
COPY . .
RUN npm run build
RUN touch /source/dist/index.html

##########
## RUNTIME
FROM nginxinc/nginx-unprivileged
COPY --from=BUILD /source/dist /usr/share/nginx/html
COPY health.html /usr/share/nginx/html/health.html
