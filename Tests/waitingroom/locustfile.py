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
        "/src/main.css",
        "/src/queue-poller.js"
      ]

      try:
        # Go get all the static content
        for doc in documents:
          response = self.client.get("%s" % doc)

        # Ask for a ticket
        response = self.client.post("%s?room=%s" % (poll_url, room), headers = headers)

        ticket = response.json()

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
            print("[%d] Wait for %d seconds" % (attempts, wait_t))
            time.sleep(wait_t)

            data = {
              "id": ticket["id"],
              "nonce": ticket["nonce"],
              "room": ticket["room"]
            }
            response = self.client.put("%s" % (refresh_url), json = data, headers = headers)
            response.raise_for_status()

            ticket = response.json()

        # wait some time before we go to the next Task
        wait_t = random.uniform(2, 4)
        time.sleep(wait_t)

      except Exception as ex:
        print("Exception.. Sleep for a bit..")
        print(ex)
        time.sleep(5)
        raise ex
