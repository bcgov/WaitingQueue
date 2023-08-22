// @ts-nocheck
const { rest, setupWorker } = MockServiceWorker;
const { assert, expect } = chai;

import { request, IncidentError, COOKIE_KEY, STORAGE_KEY } from "./request.js";

describe("request", () => {
  before(() => {
    server.start();
  });

  afterEach(() => {
    server.restoreHandlers();
    localStorage.removeItem(STORAGE_KEY);
  });

  after(() => {
    server.resetHandlers();
    server.stop();
  });

  describe("#request", () => {
    it("successfully make a request", async () => {
      const result = {
        id: 1,
      };
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          return res.once(ctx.json(result));
        })
      );
      const req = await request({
        url: "/Ticket",
        fetchOptions: {
          method: "POST",
        },
      });
      assert.notStrictEqual(result, req);
    });

    it("should handle errors", async () => {
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          return res.once(
            ctx.status(404),
            ctx.json({
              type: "error",
              status: 404,
            })
          );
        }),
        rest.post("/Ticket/check-in", (req, res, ctx) => {
          return res.once(
            ctx.status(412),
            ctx.json({
              type: "error",
              status: 412,
            })
          );
        })
      );
      await expect(
        request({
          url: "/Ticket",
          fetchOptions: {
            method: "POST",
          },
        })
      ).to.be.rejected;
      await expect(
        request({
          url: "/Ticket/check-in",
          fetchOptions: {
            method: "POST",
          },
        })
      ).to.be.rejected;
    });

    it("should handle x-incident header", async () => {
      server.use(
        rest.post("/Ticket", (req, res, ctx) => {
          return res.once(
            ctx.set("X-INCIDENT", "Y"),
            ctx.json({
              id: 1,
            })
          );
        })
      );
      await expect(
        request({
          url: "/Ticket",
          fetchOptions: {
            method: "POST",
          },
        })
      ).to.be.rejectedWith(IncidentError);
    });

    it("should compose the correct search params", async () => {
      server.use(
        rest.get("/Ticket", (req, res, ctx) => {
          if (
            req.url.searchParams.has("room") &&
            req.url.searchParams.has("total")
          ) {
            return res.once(
              ctx.json({
                id: 1,
              })
            );
          }
          return res(ctx.status(500));
        })
      );
      await expect(
        request({
          url: "/Ticket",
          fetchOptions: {
            method: "GET",
          },
          params: {
            room: "Room123",
            total: 2,
          },
        })
      ).to.be.fulfilled;
    });
  });
});
