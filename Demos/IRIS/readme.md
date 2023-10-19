# I.R.I.S. - Public Safety & Justice Investigation Support AI

*IRIS* - your **I**ntelligent, **R**esourceful **I**nvestigation **S**upport bot is an embedded AI system within an investigations case management system. IRIS uses next-generation generative AI models to assist public investigators with their investigations in the following ways:

- 🌎 Provides ariel imagery of case points of interest, viewable from satellite or birds eye view imagery.  
- ⚖️ Uses generative AI to **compare witness testimonies** and note **corroborating details** and **discrepancies**.
- 🎥 Analyzes hours-worth of case evidence (body cam footage, 911 calls, etc.) and extracts meaning: summary of event, important details, situation resolution, and more.
- 🎨 Draws composite sketches of suspects based on witness descriptions (using DALL-E)
- 📝 Logs any findings/analysis to case notes.

## Demo Video
Click the image below for a short demonstration video with the following linked timestamps:
- [**0:00**](https://youtu.be/5KNDsAtLkJU?t=5) - Investigations Case Management System Foundations
- [**0:31**](https://youtu.be/5KNDsAtLkJU?t=31) - Interactive Satellite Imagery
- [**0:48**](https://youtu.be/5KNDsAtLkJU?t=48) - Witness Testimony Analysis & Comparison
- [**1:40**](https://youtu.be/5KNDsAtLkJU?t=100) - Evidence Analysis: Officer Body Cam Video
- [**2:18**](https://youtu.be/5KNDsAtLkJU?t=138) - Evidence Analysis: 911 Call Audio
- [**2:45**](https://youtu.be/5KNDsAtLkJU?t=165) - Generate Suspect Composite Sketches
- [**4:09**](https://youtu.be/5KNDsAtLkJU?t=249) - Archive Generations to Case Notes

[![IRIS Demo Video](https://i.imgur.com/hPlxqDv.png)](https://youtu.be/5KNDsAtLkJU)

## Architecture
If running in Microsoft's **Government Community Cloud (GCC)**, this architecture implements a cross-cloud communication technique to "reach across" the boundaries of the Azure Government Cloud, into Azure Commercial, to consume Azure OpenAI service (Azure OpenAI service is not available in Azure Government as of the time of this writing). More on this cross-cloud architecture can be found [here](https://azure.microsoft.com/en-us/blog/unlock-new-insights-with-azure-openai-service-for-government/).

I've included a few diagrams below that depict how I.R.I.S. works below. 

AI-driven functionality is achieved via the following architecture:
![IRIS AI](https://i.imgur.com/6wyEJpC.png)

Satellite and birds eye view imagery is presented via Bing Maps:
![IRIS Mapping](https://i.imgur.com/tc8CJZL.png)

You can download the architecture diagrams above (PowerPoint deck) [here](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/9/architecture.pptx).

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
    1. Add your Azure AD **username**, **password**, and **Dataverse URL** to the `DataverseCredentialsProvider` class constructor [here](./src/engine/DataverseCredentialsProvider.cs).
    2. Add your Azure OpenAI **endpoint URL (for text generations)** and **subscription key** to the `PromptAsync` method at the top of the [SimpleGPT.cs](./src/engine/SimpleGPT.cs) file.
    3. Add your Azure OpenAI DALLE-E **generation URL (for image generations)** and **subscription key** to the `DALLECredentialsProvider` class constructor [here](./src/engine/DALLECredentialsProvider.cs).
2. Deploy the [Azure Function Source App Code](./src/api/) to a new Azure Function App. This will create three unique endpoints:
    - `/summarize`: used to summarize media transcripts (911 calls, bodycam footage audio, etc.)
    - `/compare`: used to compare multiple testimonies and note corroborations and discrepencies
    - `/draw`: used to trigger a workflow that performs generation of composite sketches based on witness descriptions using DALL-E
3. Import the Power Platform solution to a new enviornment. See the link above to download this solution.
4. Open the `Embedded Investigation Interface` canvas app. In the `OnSelect` property of the log in button on the first page of the app, replace `<INSERT YOUR BING MAPS API KEY HERE>` with your Bing Maps API key.
5. Open the "PSJ AI" custom connector in the solution you just imported. Replace the URL endpoints for the `Compare` and `Summarize` action with the respective URL endpoints you noted from step 1.
6. Open the `Embedded Investigation Interface` canvas app within the solution. When it prompts you to "sign in" to the PSJ AI custom connector, click on "do not allow". Once the app opens fully, delete the "PSJ AI" connection from the list of data connections. Re-add it and sign in (establish a connection). This will refresh and reconnect to the proper endpoints.
7. Import the sample data from the links listed above.
8. Open the `Investigations Management` model-driven app to test all functionality.

## Credits
- IRIS architecture, logic, code, and AI integration by [Tim Hanewich](https://github.com/TimHanewich)
- IRIS embedded application UI/UX designed by [Kelly Cason](https://www.linkedin.com/in/kellycason/)