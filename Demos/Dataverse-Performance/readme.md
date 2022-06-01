# Comparing the performance of Microsoft Dataverse

### Example Contact Record:
```
{
   "FirstName":"Rhoda",
   "LastName":"Stott",
   "BirthDate":"1953-07-25T00:00:00",
   "MobilePhone":7768294369,
   "AddressCity":"Waco",
   "AddressLatitude":31.634629,
   "AddressLongitude":-97.265100,
   "AnnualIncome":64000
}
```

## Playing Field
- Number of Records
   - SQL: 5,809,985
   - Dataverse: 5,809,985 (waiting to verify)
   - Sharepoint: 



### Dataverse Configurations
Since Dataverse is a database as a service, no configurations or tunes *can* be made. We will rely on Dataverse to allocate and arrange resources to best serve the request.

### SQL Configurations & Hardware
The Dataverse performance was directly compared to a Microsoft SQL database hosted on Microsoft Azure.
The SQL database tested against was **Standard Tier** (S0). According to Microsoft, this is the "go-to option for getting started with cloud-designed business applications. It offers mid-level performance and business continuity features".
The **Standard Tier** allocates 10 **DTU's** to the SQL Database. You can read more about DTU as a measure of performance [here](https://docs.microsoft.com/en-us/azure/azure-sql/database/service-tiers-dtu?view=azuresql).

The SQL database that was tested against has not employed any "SQL tuning" techniques. Apart from the primary key, no indexes were made, no manual optimizations were made (to better suit the queries made below), no use of temp tables, etc.


## Test Results
- Count rows in table
- Get most recent 10 records
- Latitude within certain boundary
- Living in a certain city
- Last name begins with
- Large data download

|Test|SQL Query|Time|Dataverse Query|Time|
|-|-|-|-|-|
|Count rows in table|SELECT COUNT(*) FROM Contact|0:09|https://org1ceaa16f.crm.dynamics.com/api/data/v9.1/RetrieveTotalRecordCount(EntityNames=['contact'])|0:0.4|
|Latitude within certain boundary|SELECT * FROM Contact WHERE AddressLatitude > 0.2 AND AddressLatitude < 0.4|1:27|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=address1_latitude gt 0.2 and address1_latitude lt 0.4|0:09|
|Living in a certain city|SELECT TOP 50 * FROM Contact WHERE AddressCity = 'Los Angeles'|0:03|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=address1_city eq 'Los Angeles'&$top=50|0:01|
|First name contains|SELECT * FROM Contact WHERE FirstName LIKE '%tim%'|0:10|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=contains(firstname, 'tim')|0:09|
|Search by last name|select * from Contact where LastName = 'Scott'|0:06|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=lastname eq 'Scott'|0:01|
|Select by first and last name|select * from Contact where FirstName = 'Elroy' and LastName = 'Zucker'|0:07|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=firstname eq 'Elroy' and lastname eq 'Zucker'|0:00.1|
|Get ID's of all people living in Chicago|select Id from Contact where AddressCity = 'Chicago'|0:07|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=address1_city eq 'Chicago'&$select=contactid|0.01|
|Complex filtering|SELECT top 15 * FROM Contact WHERE LastName = 'Kimm' AND BirthDate > '19930504' AND AnnualIncome > 13000 order by BirthDate desc|0:06|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=15&$orderby=birthdate desc&$filter=lastname eq 'Kimm' and birthdate gt 1993-05-04 and annualincome gt 13000|0:00.1|
|Find contact by phone number|select * from Contact where MobilePhone = 9364652201|0:12|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=mobilephone eq '2786139268'|0:00.07|
|Contacts earnings > $50,000 and < $60,000|select * from Contact where AnnualIncome > 50000 and AnnualIncome < 60000|0:08|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=10&$orderby=lastname desc&$filter=annualincome gt 50000 and annualincome lt 60000|0:03.5|
|Contacts earnings less than $32,500 in the Washington metropolitan area|select Id, FirstName, LastName from Contact where AddressCity = 'Washington metropolitan area' and AnnualIncome < 32500|0:05|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=10&$orderby=lastname desc&$filter=address1_city eq 'Washington metropolitan area' and annualincome lt 32500&$select=contactid,firstname,lastname|0:01.8|
|Contacts with 941 area code (phone number)|select Id from Contact where convert(varchar, MobilePhone) like '941%'|0:08|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=startswith(mobilephone, '941')&$select=contactid|0:00.4|
|Find contact by their last name and birthday|select * from Contact where LastName = 'Costa' and BirthDate = '1966-09-03'|0:05|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$filter=lastname eq 'Costa' and birthdate eq 1996-09-03|0:00.1|
|Large data download|SELECT TOP 10000 * FROM Contact|0:01|https://org1ceaa16f.crm.dynamics.com/api/data/v9.0/contacts?$top=10000|0:10|

## Other Learnings
- The 12-month demo tenants we receive are designated "trial" tenants. Compared to a production tenant, the trial tenant receives less resources and has limitations:
   - There is a limit on the total number of records you can place in the tenant. This is a limit on **the entire tenant**, not for a specific environment or specific table. Meaning, if you hit the limit across the tenant, you will not be able to upload a single record in *any* environment in *any* table.
      - Error message when attempting to upload a record via the web API: *Unable to create new record of type 'contacts'. Content: {"error":{"code":"0x80072325","message":"Storage quota exceeded. Your organization instance has reached the storage limit for a trial or demo instance. Please reach out to your administrator, to convert this instance into paid one or create a service request with Microsoft support team. SqlException: The database 'db_crmcorenam_20220505_03304536_f788' has reached its size quota. Partition or delete data, drop indexes, or consult the documentation for possible resolutions."}}*
   - The Dataverse DB in trial tenants/environments exhibits poor performance. When performing a query on the contacts table (sorting by a DateTime field) containing about 6,000,000 records, the query timed out:
      - ![dataverse timeout](./images/dataverse-timeout.png)


## Notes
- Counting the number of records in the contacts table:
    - https://org1ceaa16f.crm.dynamics.com/api/data/v9.1/RetrieveTotalRecordCount(EntityNames=['contact'])
    - https://crmtipoftheday.com/1375/get-record-count-for-entities/
- SharePoint does not support numbers with > 5 decimal points. Not good for precise things like Lat/Lon - mine here use 6!
- Example sharepoint query: https://graph.microsoft.com/v1.0/sites/2e069086-c6f2-4735-a728-eb33b8347842/lists/1c39d991-6ed6-4c34-87b2-f91bbb8212f5/items?$expand=fields&$top=3


## Resources
- [Sharepoint REST API Intro](https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/get-to-know-the-sharepoint-rest-service?tabs=csom)
- [Sharepoint REST OData Doc](https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/use-odata-query-operations-in-sharepoint-rest-requests)
- [How to "swap" Microsoft Graph Refresh Token for Sharepoint REST API token](https://stackoverflow.com/questions/63321532/sharepoint-rest-api-how-to-get-access-token)