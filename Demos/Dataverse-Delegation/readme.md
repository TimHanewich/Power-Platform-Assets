# Dataverse Delegation Testing & Demonstration
This test was specifically designed to test Power Apps'/Fx's **delegation** capability against both SQL and Dataverse. Read more about delegation [here](https://learn.microsoft.com/en-us/power-apps/maker/canvas-apps/delegation-overview).

## The Test & Results
**The complete test & result PDF can be downloaded [here](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/12/Dataverse.vs.SQL.Delegation.Testing.pdf).**

### TL;DR
I uploaded > 30,000 records to both SQL and Dataverse. I then proceeded to perform **UpdateIf** Power Fx queries against both data sources, observing the results. 

As predicted, both SQL and Dataverse did not update *every* record of the table that *should have been updated*. Instead, it stuck to a delegation window of only 500 records (default, but can be expanded to 2,000) and updated the pertaining records within that window.

However, once activating [a currently in-preview feature](https://learn.microsoft.com/en-us/power-platform/release-plan/2023wave1/power-apps/enhance-delegation-updateif-removeif), Power Fx then *did* delegate the Dataverse database, updating every record accordingly.

