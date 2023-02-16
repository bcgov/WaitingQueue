const { rest, setupWorker } = MockServiceWorker;
const DB_STORAGE_KEY = "WaitingQueue.mockdb";

let ticket = JSON.parse(localStorage.getItem(DB_STORAGE_KEY)) ?? null;

/** @param {number} status
 * @param {string} detail
 * @returns {Object}
 */
function makeErrorResponse(status, detail) {
  return {
    type: "Waiting Queue Exception",
    title: "Error during processing",
    status,
    detail,
    instance: "TicketCheckin.MethodName",
  };
}

const handlers = [
  rest.post("/Ticket", (req, res, ctx) => {
    const id = crypto.randomUUID();
    const nonce = crypto.randomUUID();
    const token = crypto.randomUUID();
    const unhappy = req.url.searchParams.get("unhappy");
    const room = req.url.searchParams.get("room");

    if (room !== "HealthGateway") {
      return res(
        ctx.status(404),
        ctx.json(makeErrorResponse(404, "The requested room was not found."))
      );
    }

    if (unhappy) {
      return res(ctx.status(503));
    }

    const createdTime = Math.floor(Date.now() / 1000);
    ticket = {
      id,
      room,
      nonce,
      createdTime,
      checkInAfter: createdTime + 120,
      tokenExpires: 0,
      queuePosition: 5,
      status: "Queued",
      token,
    };

    localStorage.setItem(DB_STORAGE_KEY, JSON.stringify(ticket));
    return res(ctx.json(ticket));
  }),
  rest.delete("/Ticket", (req, res, ctx) => {
    const { nonce } = req.body;

    if (!ticket || nonce !== ticket.nonce) {
      return res(
        ctx.status(404),
        ctx.json(
          makeErrorResponse(404, "The requested ticket no longer exists.")
        )
      );
    }

    ticket = null;
    localStorage.removeItem(DB_STORAGE_KEY);
    return res(ctx.status(200));
  }),

  rest.put("/Ticket/check-in", (req, res, ctx) => {
    const { unhappy } = req.params;

    if (unhappy) {
      return res(ctx.status(500));
    }

    if (!ticket) {
      return res(ctx.status(404));
    }

    if (Date.now() < ticket.checkInAfter) {
      return res(
        ctx.status(412),
        ctx.json(makeErrorResponse(412, "The check-in request was too early"))
      );
    }

    if (ticket.nonce !== req.body.nonce) {
      return res(
        ctx.status(412),
        ctx.json(makeErrorResponse(412, "Invalid nonce"))
      );
    }

    const queuePosition = ticket.queuePosition - 1;
    const isReady = queuePosition === 0;
    let status = isReady ? "Processed" : "Queued";
    const nonce = crypto.randomUUID();
    const createdTime = Math.floor(Date.now() / 1000);
    const checkInAfter = createdTime + 120;
    ticket = {
      ...ticket,
      nonce,
      createdTime,
      checkInAfter,
      queuePosition,
      status,
    };
    localStorage.setItem(DB_STORAGE_KEY, JSON.stringify(ticket));

    return res(ctx.json(ticket));
  }),
];

const worker = setupWorker(...handlers);

worker.start();
