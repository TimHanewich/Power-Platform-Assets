## All of the default table attributes
It goes `schema name` - `attribute type` - `IsCustomAttribute`
`cr97e_Id` is the primary field (autonumber)
```
cr97e_Id - String - True
CreatedOnBehalfByYomiName - String - False
ModifiedOnBehalfBy - Lookup - False
statecode - State - False
OwnerIdName - String - False
statecodeName - Virtual - False
OwningUser - Lookup - False
CreatedOnBehalfBy - Lookup - False
ImportSequenceNumber - Integer - False
UTCConversionTimeZoneCode - Integer - False
CreatedByYomiName - String - False
OwningBusinessUnit - Lookup - False
ModifiedByName - String - False
OwningTeam - Lookup - False
ModifiedBy - Lookup - False
ModifiedByYomiName - String - False
CreatedBy - Lookup - False
TimeZoneRuleVersionNumber - Integer - False
cr97e_AnimalId - Uniqueidentifier - False
OwnerIdType - EntityName - False
statuscodeName - Virtual - False
OwnerIdYomiName - String - False
ModifiedOn - DateTime - False
ModifiedOnBehalfByYomiName - String - False
statuscode - Status - False
CreatedByName - String - False
CreatedOn - DateTime - False
OwningBusinessUnitName - String - False
CreatedOnBehalfByName - String - False
ModifiedOnBehalfByName - String - False
VersionNumber - BigInt - False
OwnerId - Owner - False
OverriddenCreatedOn - DateTime - False
```


## Questions
- Where is the metadata about the table being stored?
    - Column Name, Display Name, etc.
- Are there any "logs" being created and stored (and counting towards storage costs) upon each CRUD operation taking place?
- Do many of the OOTB string attributes like `CreatedByName`, `CreatedOnBehalfByName` contain data?
- Since the "Owner" can be a user, team, business unit, in addition to the 16-byte GUID value, is anything being stored to point to the table it belongs to? (polymorphism)
- In the money attribute:
    - Are the "base" field", "_transactioncurrencyid_value" and "exchangerate" field stored or calculated and return upon the API call being made?