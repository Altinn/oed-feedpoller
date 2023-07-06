// Baseurls for platform
export var baseUrls =
 {
  platform: {
  at21: "at21.altinn.cloud",
  at22: "at22.altinn.cloud",
  at23: "at23.altinn.cloud",
  at24: "at24.altinn.cloud",
  tt02: "tt02.altinn.no",
  },
  feedpoller: {
    test: "oed-test-feedpoller-func.azurewebsites.net"
  }
};

//Get values from environment
const environment = __ENV.env.toLowerCase();
export let baseUrlPlatform = baseUrls.platform[environment];
export let baseUrlFeedpoller = baseUrls.feedpoller[environment];

//AltinnTestTools
export var tokenGenerator = {
  getEnterpriseToken:
    "https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken",
  getPersonalToken:
    "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken",
};

// Platform Events
export var platformApis = {
  events: "https://platform." + baseUrlPlatform + "/events/api/v1/events/",
  subscriptions: "https://platform." + baseUrlPlatform + "/events/api/v1/subscriptions/",
};


export var oedApis = {
  feedPoller: "https://" + baseUrlFeedpoller + "/api/",
  events: "https://digdir." + baseUrlPlatform + "/digdir/oed/",
}