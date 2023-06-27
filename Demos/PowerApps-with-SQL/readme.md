# Power Apps with SQL Demonstration
The assets in this folder demonstrate how to leverage a SQL database in a Power Apps Canvas App. This demonstrates bidirectional communication between the Power App and SQL - both reading data *from* SQL and writing data *to* SQL. More specifically, all four of the **CRUD** operations are demonstrated:
- Creating a record
- Reading records
- Updating a record
- Deleting a record
This is designed to only be a **functional demonstration** of these capabilities; this is why and UI/UX of the app is rather bland.

## Architecture
To demonstrate this, we are using a basic **Restaurant Health Inspection** use case. The entity relationship diagram can be found below:
![erd](https://i.imgur.com/MftCzGG.png)
Two tables are used (`restaurant` and `inspection`) to demonstrate parent/child, PK/FK relationships. The `id` field of each table (primary key) is set to *auto-increment*, meaning that this does not need to be specified by the Power App during an insert operation.

## App Screenshots
Browsing restaurants:  
![img](https://i.imgur.com/ZnZ9kVA.png)
Reviewing inspections for a restaurant:  
![img](https://i.imgur.com/6PWc0D4.png)
Logging an inspection for a restaurant:  
![img](https://i.imgur.com/FEXL3x2.png)

## Assets
- The SQL script to create the `restaurant` and `inspection` table can be found [here](./script.sql). Run this to create the tables in your SQL database.
- You can download the Power Apps Canvas App [here](https://github.com/TimHanewich/Power-Platform-Assets/releases/download/4/Restaurant.Health.Inspection.msapp). Note, you will have to set up your SQL integration be adding the "SQL Server" connector and walking through the prompt to establish a connection.

## Required: Whitelist Azure IP Addresses
Whether you are using an on-premises or cloud-based SQL server, you must **whitelist** the IP addresses that the Microsoft Azure Cloud uses to *call* to your SQL server to retrieve data *for* your Power App. Navigate [here](https://learn.microsoft.com/en-us/power-platform/admin/online-requirements#ip-addresses-required) to find a list of IP addresses that you will need to whitelist for your particular cloud and region.  
![whitelist](https://i.imgur.com/R4uEf0u.png)
