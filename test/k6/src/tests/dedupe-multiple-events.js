/*
    Test script to platform events api with user token
    Command:
    docker-compose run k6 run /src/tests/new-da-case-and-heirs.js `
    -e tokenGeneratorUserName=autotest `
    -e tokenGeneratorUserPwd=*** `
    -e env=*** `
    -e runFullTestSet=true

    For use case tests omit environment variable runFullTestSet or set value to false
*/
import { check } from "k6";
import * as setupToken from "../../setup.js";
import * as eventsApi from "../api/events.js";
import * as subscriptionsApi from "../api/subscriptions.js";
import { uuidv4 } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
const daEventFeed = JSON.parse(open("../../data/da-event-feed/01-single-case-multiple-events.json"));
import { generateJUnitXML, reportPath } from "../../report.js";
import { addErrorCount } from "../../errorhandler.js";
import { oedApis } from "../config.js";

const scopes = "altinn:events.subscribe"

export const options = {
  thresholds: {
    errors: ['count<1'],
  },
};

export function setup() {
  var token = setupToken.getAltinnTokenForOrg(scopes);

  var cloudEvent = daEventFeed;
  cloudEvent.id = uuidv4();

  const runFullTestSet = __ENV.runFullTestSet
    ? __ENV.runFullTestSet.toLowerCase().includes("true")
    : false;

  var data = {
    daEventFeed: daEventFeed,
    token: token,
  };

  return data;
}


// 01 - Convert multiple incoming DA event objects to single cloud event
function TC01_DeduplicateMultipleEventsToSingleEvent(data) {
  var response, success;

/*

In-progress:
1. Setup altinn-events subscriptions for outgoing event
2. Submit simulated DA event feed data to oed-events /test endpoint
3. Validate that correct cloud events are published to altinn-events 
4. Confirm that the correct cloud events were forwarded to oed-feedpoller /cloudevents endpoint

TODO:
5. Validate that end user can login to OED master app and see the correct data
6. Perform changes to OED master app data, including confirming private estate resolution.
7. Validate that oed.declaration-submitted event is published to altinn-events.
8. Clean up test data

*/

  // ensure subscription is setup
  var subscription = {
    "endPoint": oedApis.feedPoller + "cloudevents",
    "resourceFilter": "urn:altinn:resource:dodsbo.domstoladmin.api",
    "typeFilter": "no.altinn.events.digitalt-dodsbo.v1.case-status-updated"         
}

  response = subscriptionsApi.postSubscription(subscription, data.token)

  success = check(response, {
    "verify subscription for case-status-updated. Status is 200 or 201": (r) =>
      r.status === 200 || r.status === 201,
  });

  addErrorCount(success);


  // send DA event objects

  response = eventsApi.postDaEvents(
    JSON.stringify(data.daEventFeed)
  );

  success = check(response, {
    "POST multiple DA event objects. Status is 200": (r) =>
      r.status === 200,
  });

  addErrorCount(success);
}


/*
 * 01 - Convert multiple incoming DA event objects to single cloud event
 */
export default function (data) {
  if (data.runFullTestSet) {
    TC01_DeduplicateMultipleEventsToSingleEvent(data);

  } else {
    // Limited test set for use case tests

    TC01_DeduplicateMultipleEventsToSingleEvent(data);
  }
}


/*
export function handleSummary(data) {
  let result = {};
  result[reportPath("events.xml")] = generateJUnitXML(data, "events");

  return result;
}
*/
