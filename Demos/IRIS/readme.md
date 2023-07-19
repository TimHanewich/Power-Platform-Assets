# I.R.I.S. (**I**ntelligent, **R**esourceful **I**nvestigation **S**upport A.I. Bot)

*IRIS* - your **I**ntelligent, **R**esourceful **I**nvestigation **S**upport bot is an embedded AI system within an investigations case management system. IRIS uses next-generation generative AI models to assist public investigators with their investigations in the following ways:

- Provides ariel imagery of case points of interest, viewable from Satellite or birds eye view imagery. 
- Uses AI to compare witness testimonies and extract corroborating aspects and discrepancies.
- Transcribes body cam footage , 911 calls, or other media using Azure Speech to Text Cognitive Services and analyzes this text and gets back to the user with a summarization of what happened in  the call, important details, and next steps.
- Logs any findings/analysis to case notes.

## Demo Video
Click the image below for a short demonstration video.
[![IRIS Demo Video](https://i.imgur.com/iQt4ELp.png)](https://www.youtube.com/watch?v=C3EG3uIEsR8)

## Architecture
For learning purposes, I've included a few diagrams below that depict how I.R.I.S. works:

AI-driven functionality is achieved via the following architecture:
![IRIS AI](https://i.imgur.com/3MzxFTC.png)

Satellite and birds eye view imagery is presented via Bing Maps:
![IRIS Mapping](https://i.imgur.com/tc8CJZL.png)