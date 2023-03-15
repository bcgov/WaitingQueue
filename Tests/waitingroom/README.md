# Waiting Room Performance Testing

The following performance test is configured to use the `BC Stats Survey` application at https://survey-bcstats-dev.apps.gov.bc.ca.

```
ROOM=HealthGateway \
COOKIE_NAME=WAITINGROOM \
TICKET_POLL_URL=https://tickets.healthgateway.gov.bc.ca/Ticket \
TICKET_REFRESH_URL=https://tickets.healthgateway.gov.bc.ca/Ticket/check-in \
REDIRECT_PATH=/survey/cwx.cgi?_proj=cwde_2022 \
locust --config ./master.conf

```
