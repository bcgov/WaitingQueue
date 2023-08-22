// @ts-nocheck
const { rest, matchRequestUrl } = MockServiceWorker;
import sinon from "sinon";
import {
  queryByText,
  queryByRole,
  waitFor,
  getByText,
  getByRole,
} from "@testing-library/dom";
const { assert, should } = chai;

import "./queue-poller.js";
import {
  request,
  IncidentError,
  COOKIE_KEY,
  STORAGE_KEY,
  wait,
} from "./request.js";
import utils from "./utils.js";
const html = String.raw;

describe("<queue-poller>", () => {
  const fixture = document.querySelector("#fixture");
  /** @type import("sinon").SinonFakeTimers */
  let clock = null;
  let openStub = sinon.stub(utils, "open");

  before(() => {
    server.start();
    // clock = sinon.useFakeTimers();
  });

  afterEach(() => {
    // clock.restore();
    server.restoreHandlers();
    server.resetHandlers();
    const child = fixture.querySelector("queue-poller");
    localStorage.removeItem(STORAGE_KEY);
    fixture.removeChild(child);
  });

  after(() => {
    server.resetHandlers();
    server.stop();
    openStub.restore();
  });

  describe("api", () => {
    it("should complete the happy path", async () => {
      let queuePosition = 2;
      let checkInAfter = null;
      clock = sinon.useFakeTimers();
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          const createdTime = Math.floor(Date.now() / 1000);
          checkInAfter = createdTime + 1;
          return res.once(
            ctx.json({
              id: 1,
              queuePosition,
              checkInAfter,
              status: "Queued",
            })
          );
        }),
        rest.put("/Ticket/check-in", (req, res, ctx) => {
          queuePosition = Math.max(queuePosition - 1, 0);
          checkInAfter = checkInAfter + 1;
          return res(
            ctx.json({
              id: 1,
              queuePosition,
              checkInAfter,
              status: queuePosition === 0 ? "Processed" : "Queued",
            })
          );
        })
      );
      fixture.innerHTML = html`
        <queue-poller
          redirect-url="http://test.com/i"
          room="HealthGatewayDev"
          refresh-url="/Ticket/check-in"
          poll-url="/Ticket"
        ></queue-poller>
      `;

      await waitFor(() => assert.exists(queryByText(fixture, "2")));
      clock.tick(1000);
      await waitFor(() => assert.exists(queryByText(fixture, "1")));
      clock.tick(1000);
      await waitFor(() =>
        assert.exists(queryByText(fixture, "You're all set, redirecting..."))
      );
      await waitFor(() => assert.exists(queryByRole(fixture, "alert")));
      assert.equal(
        utils.open.firstCall.firstArg.toString(),
        "http://test.com/i/test.html"
      );
      // TODO: Test cookie value
      clock.restore();
    });

    it("should refresh based on `checkInAfter` value");
    it("should display incident banner", async () => {
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          return res.once(ctx.set("X-INCIDENT", "Y"));
        })
      );
      fixture.innerHTML = html`
        <queue-poller
          redirect-url="http://test.com/i"
          room="HealthGatewayDev"
          refresh-url="/Ticket/check-in"
          poll-url="/Ticket"
        ></queue-poller>
      `;
      await waitFor(() =>
        assert.exists(queryByText(fixture, "There was an incident"))
      );
    });
    it("should display error if first request fails", async () => {
      let isError = true;
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          if (isError) {
            isError = false;
            return res(ctx.status(500));
          } else {
            return res(
              ctx.json({
                id: 1,
                queuePosition: 5,
                checkInAfter: Date.now() + 1000,
                status: "Queued",
              })
            );
          }
        })
      );
      fixture.innerHTML = html`
        <queue-poller
          redirect-url="http://test.com/i"
          room="HealthGatewayDev"
          refresh-url="/Ticket/check-in"
          poll-url="/Ticket"
        ></queue-poller>
      `;
      await waitFor(() =>
        assert.exists(queryByText(fixture, "An error has occurred"))
      );
      getByRole(fixture, "button").click();
      wait(4);
      return await waitFor(() => assert.exists(queryByText(fixture, "5")));
    });
    it("should display error if refresh-url fails");
  });

  describe("language", () => {
    it("shoud change the language", async () => {
      fixture.innerHTML = html`
        <queue-poller
          lang="en-CA"
          redirect-url="http://test.com/i"
          room="HealthGatewayDev"
          refresh-url="/Ticket/check-in"
          poll-url="/Ticket"
        ></queue-poller>
      `;
      await waitFor(() => fixture.querySelector("mark"));
      const el = fixture.querySelector("queue-poller");
      el.setAttribute("lang", "fr-CA");
      await waitFor(() =>
        assert.exists(
          queryByText(
            fixture,
            "BC General Survey connaît actuellement une forte demande, ce qui entraîne des retards."
          )
        )
      );
    });

    it("shoud keep the language when changing DOM state", async () => {
      clock = sinon.useFakeTimers();
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          const createdTime = Math.floor(Date.now() / 1000);
          return res.once(
            ctx.json({
              id: 1,
              queuePosition: 1,
              checkInAfter: createdTime + 1,
            })
          );
        }),
        rest.put("/Ticket/check-in", (req, res, ctx) => {
          return res.once(
            ctx.json({
              id: 1,
              queuePosition: 0,
              status: "Processed",
            })
          );
        })
      );
      fixture.innerHTML = html`
        <queue-poller
          lang="en-CA"
          redirect-url="http://test.com/i"
          room="HealthGatewayDev"
          refresh-url="/Ticket/check-in"
          poll-url="/Ticket"
        ></queue-poller>
      `;
      await waitFor(() => assert.exists(queryByText(fixture, "1")));
      const el = fixture.querySelector("queue-poller");
      el.setAttribute("lang", "fr-CA");
      await waitFor(() =>
        assert.exists(
          queryByText(
            fixture,
            "Le système de sondage connaît actuellement une forte demande et vous a placé dans une file d'attente."
          )
        )
      );
      clock.tick(1062);
      await waitFor(() => {
        return assert.exists(
          queryByText(fixture, "Vous êtes prêt, redirection...")
        );
      });
      clock.restore();
    });
  });
});
