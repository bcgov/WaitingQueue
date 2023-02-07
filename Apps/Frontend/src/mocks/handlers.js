const { rest, setupWorker } = MockServiceWorker;
const DB_STORAGE_KEY = "WaitingQueue.mockdb";

let ticket = JSON.parse(localStorage.getItem(DB_STORAGE_KEY)) ?? null;

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
        ctx.json({
          type: "NOT_FOUND",
          title: "The requested room was not found.",
          status: 404,
          detail: "The requested room was not found.",
        })
      );
    }

    if (unhappy) {
      return res(ctx.status(503));
    }

    ticket = {
      id,
      room,
      nonce,
      createdTime: Date.now(),
      checkInAfter: 100,
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
        ctx.json({
          type: "NOT_FOUND",
          title: "Ticket not found",
          status: 404,
          detail: "The requested ticket no longer exists.",
        })
      );
    }

    ticket = null;
    localStorage.removeItem(DB_STORAGE_KEY);
    return res(ctx.status(200));
  }),

  rest.put("/Ticket/check-in", (req, res, ctx) => {
    const { unhappy } = req.params;
    if (!ticket) {
      return res(ctx.status(404));
    }

    if (ticket.nonce !== req.body.nonce) {
      return res(
        ctx.status(412),
        ctx.json({
          type: "Unable to complete request",
          title: "Invalid nonce",
          status: 412,
          detail: "...",
        })
      );
    }

    const queuePosition = ticket.queuePosition - 1;
    const isReady = queuePosition === 0;
    let status = isReady ? "Processed" : "Queued";
    const nonce = crypto.randomUUID();
    ticket = {
      ...ticket,
      nonce,
      createdTime: Date.now(),
      queuePosition,
      status,
    };
    localStorage.setItem(DB_STORAGE_KEY, JSON.stringify(ticket));

    return res(ctx.json(ticket));
  }),
];

const worker = setupWorker(...handlers);

worker.start();
