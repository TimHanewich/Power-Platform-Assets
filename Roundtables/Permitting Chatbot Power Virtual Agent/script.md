- Introduction
    - Thank you for coming to one of our regular Power Platform Roundtable sessions
        - If comng back, welcome back
        - If first time, welcome - what this series is
    - Power Virtual Agents – one of the newer components of the Power Platform, now available in GCC.
    - For making chatbots
    - Introduce our use case:
        - [Well Permitting | Division of Water Resources (colorado.gov)](https://dwr.colorado.gov/services/well-permitting)
    - Make sure to record!
- Create the table “Well Permit”
    - Id (primary name column)
    - Applicant Email (Email)
    - Well Address (text)
    - Applicant First Name (text)
    - Applicant Last Name (text)
    - Anticipated Depth (ft) (Whole number)
    - Gallons per month (Whole number)
    - Home Basin (Choice)
        - Denver Basin
        - Designated Basin
- Customize the table form + view
- Create a “Well Permit Management” Model-Driven App
- Create a PVA chatbot “Permitting Assistant”
    - Introduce the components
        - Topics – different topics, or subjects the bot is capable of discussing, answering questions, taking information
            - If you had multiple permit types, each would be a topic
        - Entities – a snippet of information in a users text that the bot will look out for and easily extract into a variable that can be used later in the bot workflow.
        - Analytics
            - Total # of sessions
            - Resolution rate – the % of sessions that the user said were successful
            - Abandon rate – the % of sessions where the user left
            - Escalation rate – the % of sessions that eventually ended up in the hands of a human
            - You can also see what PARTICULAR TOPICS led to the resolution, escalation, or abandonment
        - Publish – how to publish your bot to your website
- Create new topic
    - Name: well permitting
    - Trigger phrases
- Authoring Canvas for our “Well permitting” topic
    - (build based on template I built)
    - Address example
        - 7780 E 56th Ave, Denver, CO 80216
        - 5985 W 86th Ave, Arvada, CO 80003
        - 5880 E 62nd Ave, Commerce City, CO 80022
        - 4301 E 29th Ave, Denver, CO 80207
        - 2655 Milwaukee St, Denver, CO 80205
    - When you build the action flow, you can just create it directly from the PVA authoring canvas


NEED TO HAVE OPEN AND READY FOR REFERENCE
- The authoring canvas for the “well permit” topic
- The power automate flow for creating the well permit record
