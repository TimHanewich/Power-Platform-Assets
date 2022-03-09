# Bring your own API: Creating a Custom Connector
Oftentimes we hear customers ask "what if I need to connect to ____ system and there isn't a connector?". Or, "We use our own proprietary API in our applications. Can your system leverage our API?". This session is to demonstrate that yes, we can. In this session we create a custom connector from scratch and demonstrate how a business developer could leverage the API through a Power App and Power Automate workflow.

## Aletheia
We use the [**Aletheia API**](https://aletheiaapi.com) to demonstrate integrating a 3rd party API into the platform via connector. Aletheia is a financial data provider with many endpoints providing everything from a stock quote to insider trading history for C-suite executives. In the interest of time, we will only demonstrate adding **one** action to the custom connector, the Stock Data Quote endpoint.

You will need an API key to demonstate using Aletheia through the connector. You can register (free) at https://aletheiaapi.com/login.

**Please note that Aletheia is now a certified connector (out of the box) in the Power Platform. However, this does NOT prevent us from creating a custom connector for it's endpoints anyway. Simply ignore the fact that it already exists OOTB while presenting.**

## Content
- [Script](./Script.docx)
- [Icon for custom connector](./icon.png)

## Session Recordings
- Tim Hanewich, New Mexico, March 2021: https://youtu.be/jnwuGLwmrdk