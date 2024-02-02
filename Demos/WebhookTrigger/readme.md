# Webhook Service Example: Public Safety Alerts
![public safety alert](https://i.imgur.com/vS34Ep5.png)

This is a webhook service that serves to demonstrate the use of Power Platform's custom connector's actions & triggers against a fictitious public safety alert system.

## Endpoint Services, Documented
The [Azure Functions-based API](./src/api/) has several endpoints that are described below:

### Get Public Safety Alerts
This endpoint returns a list of Public Safety Alert records as a JSON array.

Example request:
```
GET https://ExampleService.com/alerts/
```

Example response:
```
[
    {
        "Id": "B32C83",
        "UtcTime": "2024-02-01T15:53:53.0304463Z",
        "IssuingAuthority": "Public Alert Coordination Unit (PACU)",
        "AlertType": "Power Outage Alert",
        "AffectedRegions": "Ivydale City"
    },
    {
        "Id": "F30F9B",
        "UtcTime": "2024-02-01T15:57:10.5581563Z",
        "IssuingAuthority": "Emergency Preparedness Authority (EPA)",
        "AlertType": "Evacuation Order",
        "AffectedRegions": "Sunflower County"
    },
    {
        "Id": "15834A",
        "UtcTime": "2024-02-01T15:57:14.076258Z",
        "IssuingAuthority": "Regional Emergency Response Agency (RERA)",
        "AlertType": "Civil Disturbance Advisory",
        "AffectedRegions": "Sunset Valley"
    }
]
```

**Tip: You can also extend the URL with `/clear` to delete all public safety records in the database (GET request to `https://ExampleService.com/alerts/clear`).**

### Create a new (random) Public Safety Record
This endpoint will create a new random public safety alert record. This is meant to be an internal-only endpoint, meaning it only exists for the purposes of the demonstration. After subscribing your endpoint service (Power Automate workflow) to the webhook service below, you can make a request to this endpoint to 1) have a new Public Safety Record created with random data, 2) saved to the database, and 3) alert every subscribed endpoint service of this new Public Safety Alert record.

Example request:

```
GET https://ExampleService.com/newpsa
```

Example response (this response body is purely for informational purposes):
```
200 OK

{
    "newRecord": {
        "Id": "F3C3FD",
        "UtcTime": "2024-02-01T15:57:17.1914522Z",
        "IssuingAuthority": "State Safety Commission (SSC)",
        "AlertType": "Severe Weather Warning",
        "AffectedRegions": "Hillcrest Parish"
    },
    "webhookSubscribersNotifiedCount": 2,
    "webhookSubscribersNotified": [
        "https://myurl.com/hitme",
        "https://sfklsdjknasf.com/fjdkkjaf"
    ]
}
```
You can see in the response above, it shows the new record that was made and saved to the DB, a count of how many subscribed endpoints were notified of this new Public Safety Alert record, and each "notified" subscribed endpoint listed. 

Again, this endpoint is *only* mean to be used for demo purposes. This is *not* an endpoint that is supposed to be registered with a custom connector. Simply make a GET request to this endpoint *after* your Power Automate endpoint to demonstrate the Power Automate "trigger" functionality.

### Webhook Service: Subscribe
This endpoint allows you to *subscribe* your own web service to get updates on the addition of new Public Safety Alert records. This is the endpoint that Power Automate will call to to subscribe a workflow to these updates.

Example request:

```
POST https://ExampleService.com/subscribe

{
    "url": "https://MyCoolWorkflow.com/example_workflow_trigger"
}
```

The request above will subscribe the endpoint `https://MyCoolWorkflow.com/example_workflow_trigger` to future updates of new Public Safety Alert records. 

The response to the request above will look like the following:
```
201 CREATED
Location https://ExampleService.com/unsubscribe/4BE194C820
```

The `Location` header above is crucial - this is the *exact* endpoint that the subscribed webhook service (Power Automate in the case of this demo) will send a `DELETE` HTTP request to, requesting it be unsubscribed from the webhook service. **Thus, it is vital that this service provides a `Location` endpoint that is *unique* to the newly subscribed service (from this request).**

Once subscribed, once new Public Safety Alerts are created via the `/create` endpoint above, this is what the HTTP request delivered to the subscribed service will look like:
```
POST https://MyCoolWorkflow.com/example_workflow_trigger

{
  "Id": "848903",
  "UtcTime": "2024-02-01T19:31:46.1824534Z",
  "IssuingAuthority": "Municipal Safety Council (MSC)",
  "AlertType": "Cybersecurity Breach Notification",
  "AffectedRegions": "Sunset Valley"
}
```
As seen above, it is only a single Public Safety Alert record, the new one that was just made.

And, if you're curious about how Power Automate will respond to this request, it will simply return a `202 ACCEPTED` response, indicating that it has accepted the new record and thus triggered the subscribed workflow:

![triggered workflow](https://i.imgur.com/zrkG8Vw.png)

### Webhook Service: Unsubscribe
Eventually, the subscribed webhook will no longer want updates! This could be caused from either the Power Automate dev turning off the subscribed flow or deleting it altogether. When either of these happen, Power Automate will automatically make a call to your service, asking for the endpoint (flow trigger) it previously subscribed, to be unsubscribed from future updates!

As described above, it will specifically use the **exact** URL that the webhook service provided to it as the `Location` header when it originally subscribed. It will send a single DELETE request to this URL.

In the example in the `/subscribe` endpoint description above, Power Automate would make an HTTP DELETE request to the endpoint as such:
```
DELETE https://ExampleService.com/unsubscribe/4BE194C820
```
Again, the URL must be unique to *a specific* subscribed endpoint (flow trigger); thus, in this case, the `4BE194C820` is a unique identifier, identifying the specific subscribed endpoint to unsubscribe.

So what happens if, for whatever reason, the unsubscribe process did not work? And the flow is deleted but your webhook service continues to try to make calls to it when new Public Safety Alert records are created? In this event, the Power Automate service will return a `400 BAD REQUEST` response, instead of the expected `202 ACCEPTED` if it was accepted (triggered a workflow) successfully:

![workflow has been deleted and thus has been unsubscribed](https://i.imgur.com/aGYMmaw.png)

This is Power Automate's way of reminding your webhook service that this flow has previously asked to be unsubscribed, so it no longer wants updates.

### Version
Gets the version of the API you are using (in case you want to verify a update you deployed is now live, increment this before deploying).

Example request:
```
GET https://ExampleService.com/version
```

Example response:
```
200 OK

0.1.0
```

## Appendix Resources
- Full PSA Graphic: https://i.imgur.com/QrTUJnQ.png
- Safety Shield Graphic: https://i.imgur.com/SlSyzgw.png