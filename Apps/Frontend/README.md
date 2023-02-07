# Waiting Queue Frontend

> NOTE: This is a simple proof of concept, starting with the simplest stack possible and expanding as needed.

The quickest way to preview for development is starting a Python server like so in `Apps/Frontend`:

```shell
$ python3 -m http.server
```

To keep latency as low as possible we'll implement simple CSS and Web Components.

`<template>` tags are used because tagged template literals in the JavaScript will cost more in JavaScript parse time,
but template tags are ignored by the browser while evaluating the HTML. Code sharing is more difficult however,
so this could be over-engineering the solution...

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

-   [ ] Wire up a proper `poll-url` endpoint and handle the response in `#fetchTicket`.
-   [ ] Minification of HTML/CSS/JS
-   [ ] Handle disabled JS
-   [ ] Styling
-   [ ] Determine the build requirements. Will the HTML be generated dynamically?
-   [ ] Research sharing `<template>` tags.
