# I.R.I.S. - Public Safety & Justice Investigation Support AI

*IRIS* - your **I**ntelligent, **R**esourceful **I**nvestigation **S**upport bot is an embedded AI system within an investigations case management system. IRIS uses next-generation generative AI models to assist public investigators with their investigations in the following ways:

- Provides ariel imagery of case points of interest, viewable from Satellite or birds eye view imagery. 
- Uses AI to compare witness testimonies and extract corroborating aspects and discrepancies.
- Transcribes body cam footage , 911 calls, or other media using Azure Speech to Text Cognitive Services and analyzes this text and gets back to the user with a summarization of what happened in  the call, important details, and next steps.
- Logs any findings/analysis to case notes.

## Demo Video
Click the image below for a short demonstration video.
[![IRIS Demo Video](https://i.imgur.com/ziasLDu.png)](https://youtu.be/IDTXlGXEco4)

## Architecture
For learning purposes, I've included a few diagrams below that depict how I.R.I.S. works:

AI-driven functionality is achieved via the following architecture:
![IRIS AI](https://i.imgur.com/boZRM7q.png)

Satellite and birds eye view imagery is presented via Bing Maps:
![IRIS Mapping](https://i.imgur.com/tc8CJZL.png)

You can find the architecture diagrams above in [this](./architecture.pptx) PowerPoint deck.

## Assets
- The C# source code behind the Azure Function App API [here](./src/)
- You can download the Power Platform solution [here](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/8/investigations_1_0_0_3.zip). This contains all of the necessary resources to stand this up in an environment - apps, tables, flows, connectors, etc.
- Sample data for every table used in the demo video above:
    - [Contacts](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/7/contacts.csv)
    - [Cases](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/7/inv_cases.csv)
    - [Case Affiliations](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/7/inv_caseaffiliations.csv)
    - [Digital Evidence](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/7/inv_digitalevidences.csv)
    - [Testimonies](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/7/inv_testimonies.csv)

## Steps to Deploy
1. Replace the "secrets" (API keys, subscription keys, resource URL's, usernames/passwords, etc.) in the [Azure Function App Source Code](./src/api/):
    1. Add your Azure AD **username**, **password**, and **Dataverse URL** to the `CdsAuthAsync` method at the end of the [InvestigationTools.cs](./src/engine/InvestigationTools.cs) file.
    2. Add your Azure OpenAI **endpoint URL** and **subscription key** to the `PromptAsync` method at the top of the [SimpleGPT.cs](./src/engine/SimpleGPT.cs) file.
2. Deploy the [Azure Function Source App Code](./src/api/) to a new Azure Function App. Note the endpoint URL's of the `compare` and `summarize` services.
3. Import the Power Platform solution to a new enviornment. See the link above to download this solution.
4. Open the `Embedded Investigation Interface` canvas app. In the `OnSelect` property of the log in button on the first page of the app, replace `<INSERT YOUR BING MAPS API KEY HERE>` with your Bing Maps API key.
5. Open the "PSJ AI" custom connector in the solution you just imported. Replace the URL endpoints for the `Compare` and `Summarize` action with the respective URL endpoints you noted from step 1.
6. Open the `Embedded Investigation Interface` canvas app within the solution. When it prompts you to "sign in" to the PSJ AI custom connector, click on "do not allow". Once the app opens fully, delete the "PSJ AI" connection from the list of data connections. Re-add it and sign in (establish a connection). This will refresh and reconnect to the proper endpoints.
7. Import the sample data from the links listed above.
8. Open the `Investigations Management` model-driven app to test all functionality.
