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

## Test Results
- Count rows in table
- Get most recent 10 records
- Latitude within certain boundary
- Living in a certain city
- Last name begins with
- Large data download


## Notes
- Counting the number of records in the contacts table:
    - https://orgde82f7a5.crm.dynamics.com//api/data/v9.1/RetrieveTotalRecordCount(EntityNames=['contact'])
    - https://crmtipoftheday.com/1375/get-record-count-for-entities/