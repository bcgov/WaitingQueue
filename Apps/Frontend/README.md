# Waiting Queue Frontend

> NOTE: This is a simple proof of concept, starting with the simplest stack possible and expanding as needed.

### Development
Just a simple static asset web server is needed to develop the interface, in this example we just use Python 3's HTTP server:

```shell
$ cd ./Apps/Frontend
$ python3 -m http.server
```

`src/main.js` is the JS entry point, using native ES6 modules. The `<queue-poller>` web component is in charge of presentation, handling ticket requests and redirection on success.

Use like so:

```html
<queue-poller
	id="poller"
	redirect-url="http://localhost:8000/redirect-test.html"
	room="HealthGateway"
	poll-url="/Ticket"
	refresh-url="/Ticket/check-in">
</queue-poller>
```

The server responses are mocked using [Mock Service Worker](https://mswjs.io/). Append `?unhappy=` to any `<queue-poller>` attribute's URL to simulate an error response. The service worker will use `localStorage` as a database to keep track of the user's place in the queue. Set the `TICKET_INTERVAL` in `src/mocks/handlers.js` to speed up or slow down the space between pings.

### Bundling
Run the following to build the project for the first time on your location machine, a Docker container or as part of a deployment.

```shell
$ npm install
$ npm run build
```

Building will parse the `config.yaml` and output JavaScript and CSS minified files in `/dist` as well as HTML files in the following folder structure `/dist/[environment]/[project_name]/[lang]/index.html` (the values in brackets refer to the bracketed properties in the example below), which will reference the assets in the `/dist` root folder. This way there can be multiple projects which contain multiple environments with any number of languages. Because the component templates are composed as `<template>` tags in the HTML template, there is no need for multiple localizations of the JavaScript bundle.

The config file should be structured like so:

```yaml
[project_name]:
	environments:
		[environment]:
			room: string
			legacyBrowser: boolean
			redirectUrl: url
			pollUrl: url
			refreshUrl: url
	locales:
		- [lang]:
			appName: string
			title: string
			errorText: string
			componentFallbackText: string
			statusTitle: string
			statusPositionText: string
			errorTitle: string
			errorRetryText: string
			redirectText: string
```

> **Note:** the root level properties are the project name/folder only

Refer to `tools/template.ejs` to see where the `lang` properties are rendered.

> **Note on cache-busting:** the `package.json` `version` property is used to append the `v` query string to static assets. Because this isn't a library and other commits and tags might affect the commit tree, use `npm --no-git-tag-version version` to upgrade via `npm`.

### Running in a Container

```
docker build --tag frontend .
docker run -ti --rm --name waitroom-frontend -p 8800:8080 frontend
```

### Deploying to Kubernetes

```
cd Tools/Deploy/helm
helm dependency update frontend
helm upgrade --install dev-frontend ./frontend
```

**Temporary Routing**

1. Create a route for: `https://waitingqueue-fe-dev.apps.silver.devops.gov.bc.ca`

2. Add a network policy:

```
kind: NetworkPolicy
apiVersion: networking.k8s.io/v1
metadata:
  name: dev-frontend-np
  namespace: d0bc21-dev
  labels:
    app.kubernetes.io/part-of: waitingqueue-dev
spec:
  podSelector:
    matchLabels:
      app.kubernetes.io/part-of: waitingqueue-dev
      app.kubernetes.io/instance: dev-frontend
  ingress:
    - ports:
        - protocol: TCP
          port: 8080
  policyTypes:
    - Ingress
```

### TODO

-   [x] Wire up a proper `poll-url` endpoint and handle the response in `#fetchTicket`.
-   [x] Minification of HTML/CSS/JS
-   [ ] Handle disabled JS
-   [x] Styling
-   [x] Determine the build requirements. Will the HTML be generated dynamically?
-   [ ] Research sharing `<template>` tags.
