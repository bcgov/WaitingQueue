import os
from locust import HttpUser, task
import time
import random

class WaitingRoom(HttpUser):

    @task
    def pass_through_waitroom(self):

      room = os.environ['ROOM']
      cookie_name = os.environ['COOKIE_NAME']
      poll_url = os.environ['TICKET_POLL_URL']
      refresh_url = os.environ['TICKET_REFRESH_URL']
      redirect_path = os.environ['REDIRECT_PATH']

      headers = {
        "Content-Type": "application/json"
      }

      documents = [
        "/",
        "/main.css",
        "/main.js"
      ]

      try:
        # Go get all the static content
        for doc in documents:
          response = self.client.get("%s" % doc)

        busy = 0
        while True:
          # Ask for a ticket
          response = self.client.post("%s?room=%s" % (poll_url, room), headers = headers)

          ticket = response.json()

          # The waiting queue has exceeded maximum capacity, try again later
          if response.status_code == 503:
            busy = busy + 1
            print("[%d] Exceeded capacity - sleeping 10 seconds" % busy)
            time.sleep(10)
          elif response.status_code == 429:
            busy = busy + 1
            wait_t = random.uniform(0, 2)
            print("[%d] Too Many Requests - sleeping %f seconds" % (busy, wait_t))
            time.sleep(wait_t)
          else:
            attempts = 1
            while True:
              print(ticket["id"])

              if ticket["status"] == "Processed":
                # If processed, then redirect back with the set cookie
                cookies = {}
                cookies[cookie_name] = ticket["token"]

                response = self.client.get(redirect_path, headers = headers, cookies = cookies)

                break
              else:
                # else wait until next checkin time, to refresh the queue position
                attempts = attempts + 1

                wait_t = ticket["checkInAfter"] - time.time()
                print("[%d] Wait for %f seconds" % (attempts, wait_t))
                time.sleep(wait_t)

                data = {
                  "id": ticket["id"],
                  "nonce": ticket["nonce"],
                  "room": ticket["room"]
                }
                response = self.client.put("%s" % (refresh_url), json = data, headers = headers)
                response.raise_for_status()

                ticket = response.json()

            break

        # wait some time before we go to the next Task
        wait_t = random.uniform(2, 4)
        time.sleep(wait_t)

      except Exception as ex:
        print("Exception.. Sleep for a bit..")
        print(ex)
        time.sleep(5)
        raise ex
