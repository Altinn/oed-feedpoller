import http from "k6/http";

import * as config from "../config.js";

import * as apiHelpers from "../apiHelpers.js";

export function postDaEvents(daEventList) {
  var endpoint = config.oedApis.events + "test"

  var response = http.post(endpoint, daEventList);

  return response;
}


export function getCloudEvents(queryParams, token) {
  var endpoint = config.platformApis.events;
  return getEvents(endpoint, queryParams, token);
}

export function getEventsFromNextLink(nextLink, token) {
  return getEvents(nextLink, null, token);
}

function getEvents(endpoint, queryParams, token) {
  endpoint +=
   queryParams != null
    ? apiHelpers.buildQueryParametersForEndpoint(queryParams)
    : "";

  var params = apiHelpers.buildHeaderWithBearerContentType(
    token,
    "application/cloudevents+json"
  );

  var response = http.get(endpoint, params);

  return response;
}

