# sentry-to-webex-poc

POC of a Sentry to Webex integration. It stands up an HTTP Server that listens for a single POST Route, `/api/SentryWebhook` that listens for Sentry webhook calls (an internal integration has been set up in our [Sentry](https://cisco-fj.sentry.io) instance), translates them into Markdown, and posts them to the `Test Sentry Space` in Webex via an incoming Webhook.

To run it, copy `appsettings.json` to `appsettings.Development.json` and fill out a Webex webhook URL and Sentry secret. You'll also need to use something like [ngrok](https://cisco-sbg.atlassian.net/wiki/spaces/dev/pages/895058419/How+to+use+ngrok+for+local+development) to expose your local server to Sentry.

Next Steps: I reached out to SRE about hosting such a bot. Assuming that gets cleared, we'll want to shore this up from a POC to a for reals implementation, add some additional filtering for events so we don't make it too noisy, and then make it live.
