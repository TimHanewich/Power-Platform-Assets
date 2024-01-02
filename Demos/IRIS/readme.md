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

### AI-driven functionality is achieved via the following architecture:
![IRIS AI](https://i.imgur.com/6wyEJpC.png)

### Satellite and birds eye view imagery is presented via Bing Maps:
![IRIS Mapping](https://i.imgur.com/tc8CJZL.png)

You can download the architecture diagrams above (PowerPoint deck) [here](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/9/architecture.pptx).

## Credits
- IRIS architecture, logic, code, and AI integration by [Tim Hanewich](https://github.com/TimHanewich)
- IRIS embedded application UI/UX designed by [Kelly Cason](https://www.linkedin.com/in/kellycason/)