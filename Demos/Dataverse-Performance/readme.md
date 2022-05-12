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
|Count rows in table|SELECT COUNT(Id) FROM CONTACT|6:40|||
|Select oldest 10 people|SELECT TOP 10 * FROM CONTACT ORDER BY BirthDate ASC|0:28|||
|Latitude within certain boundary|SELECT TOP 50 * FROM CONTACT WHERE Latitude > 0.1 AND Latitude < 0.4|1:26|||
|Living in a certain city|SELECT TOP 50 * FROM CONTACT WHERE AddressCity = 'Los Angeles'|0:02|||
|Last name begins with|SELECT * FROM CONTACT WHERE LastName LIKE 'at%'|0:10|||
|First name contains|SELECT * FROM CONTACT WHERE FirstName LIKE '%tim%'|0:09|||
|Large data download|SELECT TOP 3000 * FROM CONTACT|0:00|||


## Notes
- Counting the number of records in the contacts table:
    - https://orgde82f7a5.crm.dynamics.com//api/data/v9.1/RetrieveTotalRecordCount(EntityNames=['contact'])
    - https://crmtipoftheday.com/1375/get-record-count-for-entities/