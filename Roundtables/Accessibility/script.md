## Script
- Welcome
- Recap of last session
- Intro to Accessibility
    - What is Accessibility?
    - Why is Accessibility important?
        - In the US, 57 million people have a disability.
            - 20M have trouble lifting or gripping, hinder their ability to hold a cell phone.
            - 8M have vision problems
            - 7.5M have hearing problems
        - Captioning on videos has increased engagement by 500% in the last 6 years.
        - 15% of the world's population lives with some form of disability
        - Accessibility impacts all of us
            - As we get older, these technologies are assistive
            - High contrast mode: view in high light setting
            - Video captioning: view video in loud setting
        - Colorado bill passing
    - In Microsoft's DNA
        - https://cdn.mos.cms.futurecdn.net/oiaTuCodgrhPApGPDARUMY-970-80.jpg.webp
        - Satya Nadella - "Inclusive design principles will have the deepest impact in building products designed for everyone."
        - Also in the DNA of the Power Platform - built from the GROUND UP with Accessibility in mind
- How Accessibility is measured: Web Content Accessibility Guidelines (WCAG)
    - Published by the World Wide Web Consortium
    - Latest version: 2.1
    - We are on the 2.1 standard, other major low code systems are on the 2.0 standard
- Our platform and accessibility
    - Dually accessible: **use apps** and **create apps**
    - Features
        - AccessibleLabel
        - Live
        - TabIndex
        - Role
        - The Accessibility Checker Feature


## Live Build Accessibility Feature checklist
- Create Welcome Screen
    - Name the screen
    - State Seal picture has **AccessibleLabel**
    - State Seal picture has **TabIndex**
    - Set two labels and button **TabIndex** to 0
    - Set welcome label **Role** to Role.Heading1 and description label **Role** to Role.Heading2
- Time Off Request Screen
    - Name the screen
    - `Please fill out the form below with information about your leave` instructions label
        - **TabIndex** = *0*
        - **Role** = *Role.Heading1*
    - Leave Request Form
        - Every field has **AccessibleLabel** filled out
        - `Starting Monday` Date Picker **IsEditable** = *true*
        - `Starting Monday` hour & time picker
            - **DisplayMode** = *DisplayMode.Disabled*
            - **TabIndex** = *-1*
        - Every other field has **TabIndex** = *0*
    - `This request will use __ hours` label
        - **Role** = *Role.Heading2*
        - **TabIndex** = *0*
        - **Live** = *Live.Assertive*
    - `Submit` button
        - **TabIndex** = *0*
- Success Screen
    - Name the screen
    - `Your time-off request was submitted! You may now close this app.` label
        - **TabIndex** = *0*
        - **Role** = *Role.Heading1*
    - Home icon
        - **TabIndex** = *0*
        - **AccessibleLabel** = *Return home icon to go back to the welcome screen*




## Resources
Stating the importance of accessibility with some great stats: https://buildfire.com/app-accessibility-mobile-development/