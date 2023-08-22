# Waiting Queue Frontend

A simple web component that communicates with a ticketing API to relay a user's current place in line when accessing a busy service.

The application is built of:

- A web component
- A Node.js script to build static HTML and compile assets
- A development server

## Usage

### `queue-poller`

Example usage:

```html
<queue-poller
	id="poller"
    lang="en-CA"
	redirect-url="http://localhost:8000/redirect-test.html"
	room="HealthGateway"
	poll-url="/Ticket"
	refresh-url="/Ticket/check-in">
</queue-poller>
```

#### Attributes 
`lang` - Sets the current language on the component, and is the only observed attribute.

`room` - the component can only poll the ticket status of a single room.

`poll-url` - the initial endpoint to obtain the user's ticket.

`refresh-url` - this endpoint updates the status of the ticket on a regular interval.

`redirect-url` - the URL the user originally tried to visit, now ready for them to access.

The templates for the different UI states are located in the HTML template as `template` tags.

Language is switched dynamically by a dropdown, with `en-CA` being the default language.

### Bundling
Run the following to build the project for the first time on your location machine, a Docker container or as part of a deployment.

```shell
$ npm install
$ npm run build
```

Building will parse the `config.yaml` and output JavaScript and CSS minified files in `/dist` as well as HTML files in the following folder structure `/dist/[project_name]/[environment]/index.html` (the values in brackets refer to the bracketed properties in the example below), which will reference the assets in the `/dist` root folder. This way there can be multiple projects which contain multiple environments with any number of languages. Because the component templates are composed as `<template>` tags in the HTML template, there is no need for multiple localizations of the JavaScript bundle.

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
            incidentTitle: string
            incidentText: string
			componentFallbackText: string
			statusTitle: string
			statusPositionText: string
			errorTitle: string
			errorRetryText: string
			redirectText: string
```

> **Note:** the root level properties are the project name/folder only

Refer to `tools/template.ejs` to see where the `lang` properties are rendered.

> **Note on cache-busting:** the `package.json` `version` |property is used to append the `v` query string to static assets. Because this isn't a library and other commits and tags might affect the commit tree, use `npm --no-git-tag-version version` to upgrade via `npm`.

### Development

The development server uses esbuild's built-in dev server.

```shell
$ cd ./Apps/Frontend
$ npm run dev
```

The server responses are mocked using [Mock Service Worker](https://mswjs.io/). Append `?unhappy=` to any `<queue-poller>` attribute's URL to simulate an error response. The service worker will use `localStorage` as a database to keep track of the user's place in the queue. Set the `TICKET_INTERVAL` in `src/mocks/handlers.js` to speed up or slow down the space between pings.

Unit tests are accessible at `http://localhost:[port]/test.html`. They use Mocha, Sinon and testing-library, browser only.

In the interest of keeping overhead low, JSDoc is use in lieu of TypeScript. That being said, with the presence of the `jsconfig.json` file, the TypeScript Language Server will run using JSDoc comments.

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
