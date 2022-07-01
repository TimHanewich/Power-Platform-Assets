## What our platform does
- Image, Icon, Shape controls have property "AccessibleLabel" for providing alt text.
- Live
    - Off: Screen reader does NOT announce changes to labels
    - Polite: Screen reader will announce change to labels after it is done with what it is currently saying
    - Assertive: Screen reader will drop what it's doing to announce the change to the label
- Role
    - The role of a label. Allows quicker navigation with screen reader.
    - Rules:
        - Only 1 **Heading 1** in each screen
        - **Heading 2** = Subheading
        - **Heading 3** & **Heading 4** = finer hierarchies within a heading 2.
        - **Default** = Normal text
- TabIndex
    - Codes
        - **0** = participates in tab index (can be "tabbed" to). For things like buttons, text inputs, combo boxes, etc.
        - **-1** = does NOT participate in tab index (will be skipped). For things like labels, images, icons, etc. Typically descriptive, yet non-interactive controls.
    - When TabIndex of ALL controls are set to -1 or 0, order will go from left-to-right and then from top-to-bottom. Based on the X and Y values of each control.
    - If you want to bundle controls together (and thus ignore the default ordering), use the **Container** control.
        - Controls in **Form Cards** and **Galleries** are automatically grouped.


## Tips
- Use pascal case. Capitalize first word in a string with multiple words. Helps screen reader know where the word starts and ends.