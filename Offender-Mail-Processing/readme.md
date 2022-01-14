# Offender Mail Processing and Badge Generation POC
This is a proof-of-concept Power Platform solution that handles inmate mail processing and inmate badge generation:

## Mail Processing
A staff member with the Department of Corrections will "scan" incoming inmate mail by taking a picture of each mail page with the **Resident Mail Scanning** canvas app. The mail will be associated with the inmate via the inmate's ID number. Each page the inmate received will go through an analysis process that is driven by a Power Automate flow. The Power Automate workflow will search for key phrases and words (customizable) that indicate risk across five categories: Gang activity, Violence, Drugs, Weapons, or Conduct. Each letter will be rated according to the risk factor in each of these five categories. If one of these categories surpasses a predetermined risk threshold, administrators at the facility will be alerted via email. The administrators can review the letter in the **Resident Mail Management** model-driven application.

## Badge Generation
Staff members can generate an inmates badge via the **Resident Badge Generation** canvas app. This app will generate a unique QR code that will be placed on the badge and uniquely identifies that inmate. Officers within the facility can later scan this badge, review and document employee behavior or infractions, and review basic information about the inmate, via the **Officer Badge Verifier** canvas mobile app (phone form factor).

## Solutions
You can download the .ZIP Power Platform solution file [here](./solutions/ResidentMailProcessing.zip).

## Deployment Instructions
1. Import the solution ZIP file that can be found above.
2. The workflow that assesses risk levels for each letter identifies risk by searching for **Phrase Flags** within each letter. Therefore, the user should create multiple **Phrase Flag** records with phrases and key words that they deem risky. To save time, I am supplying 44 sample phrase flags that **you can import**. You can find the sample data [here](./sample-data/cr0d5_phraseflags.csv).
3. 