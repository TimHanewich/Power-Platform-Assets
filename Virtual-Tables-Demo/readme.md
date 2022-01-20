# Virtual Tables Demo
A demonstration of using virtual tables in a Power Apps Portal.

## Resources
There are no guarentees these resources will exist/work in the future.
- Rate Requests endpoint: https://nmosi2.azurewebsites.net/RateRequest
    - GET request, anonymous
- NMOSI Endpoint (follows OData V4):
    - Directory (to provide to OData V4 Provider in D365 Virtual Tables:)
        - https://nmosi2.azurewebsites.net/nmosi
    - RateRequests table content:
        - https://nmosi2.azurewebsites.net/nmosi/RateRequests
    - Metadata
        - https://nmosi2.azurewebsites.net/nmosi/$metadata
- "sample" endpoint - a reconstruction of a functional OData V4 API
    - Directory
        - https://nmosi2.azurewebsites.net/sample
    - Advertisements table content:
        - https://nmosi2.azurewebsites.net/sample/Advertisements
    - Metadata
        - https://nmosi2.azurewebsites.net/sample/$metadata


## Tutorials used for learning
- https://dynamicsninja.blog/2018/09/30/virtual-entities-part-1-odata-v4/
- https://docs.microsoft.com/en-us/powerapps/maker/data-platform/virtual-entity-walkthrough-using-odata-provider#create-the-virtual-table