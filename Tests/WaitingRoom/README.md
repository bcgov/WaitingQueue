# Waiting Room Performance Testing

The following performance test is configured to use the `BC Stats Survey` application at https://survey-bcstats-dev.apps.gov.bc.ca.

```
ROOM=ARD \
COOKIE_NAME=WAITINGROOM \
TICKET_POLL_URL=https://bcstats-waitroom-dev.api.gov.bc.ca/Ticket \
TICKET_REFRESH_URL=https://bcstats-waitroom-dev.api.gov.bc.ca/Ticket/check-in \
REDIRECT_PATH=/i/cwx.cgi?_proj=cwde_2022 \
locust --config ./master.conf

```

## Testing Behavior

The `HealthGateway` room is configured with the following settings:

```
"ParticipantLimit": 10
"QueueThreshold": 5
"QueueMaxSize": 10
"RemoveExpiredMax": 250
```

If we run a test with 50 concurrent users, we expect that 10 will be let through immediately, and then the rest will be queued. So there will be 15 successful checkins and 10 requests to `/survey/cwx.cgi`. After 4 minutes 10 more will be let through.
