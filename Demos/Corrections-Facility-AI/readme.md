Documentation: https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523c

Steps:
1. Create a PersonGroup (i.e. "residents", "officers")
    - https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395244
2. Create a Person for each person you want to store. They must be associated to a PersonGroup
    - https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523c
3. Add several faces (called "Persistent Face") for each of the people you added.
    - https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523b
4. Detect a face by providing it
    - https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236
    - Make sure you specify a recognition model. The recognition model you specify here must MATCH the one you use later on for the actual identification.
    - This will return an ID of the detection you just did. You will pass this to the **identify** endpoint to identify who that is (what person it corresponds to)
5. Identify a face
    - https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395239
    - The recognitionModel you are using (a parameter) must MATCH the recognitionModel you specified in the step before this, the **Detection** step.