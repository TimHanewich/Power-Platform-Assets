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


## Research
- [Architecture design of El Dorado](https://htkarchitects.net/projects/el-dorado-correctional-facility/)
    - The El Dorado Correctional Facility consists of two general purpose housing units which are designed to accommodate minimum to maximum custody levels. Each unit is designed for a double bunk capacity of 256. The El Dorado Correctional Facility is divided into quadrants, each consisting of 31 or 33 cells with a common two level day room. The control room is centrally located with visibility into each quadrant. The inmates are housed along the exterior perimeter wall with cells constructed of masonry block. Precast hollow core slabs provide the floor system of the upper level mezzanine and the ceiling for the lower level. Colored, precast concrete sandwich panels comprise the exterior bearing walls. The roof is structural steel and bar joists while the foundation is concrete spread footings.

## Notable Scenes in Show
- Episode 6: 
    - 19:43 jaclin entering basketball court.
    - 39:23 Jacline re-entering pod
    - 39:37 Jaclin re-entering cell
    - 29:33 Matt sitting at table talking
    - 34:23 fight breaks out (no one in it)
    