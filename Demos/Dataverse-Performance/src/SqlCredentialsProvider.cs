using System;

namespace DataversePerformance
{
    public class SqlCredentialsProvider
    {
        public static string GetSqlConnectionString()
        {
            return "Server=tcp:20220502performancetesting.database.windows.net,1433;Initial Catalog=20220502performancetest;Persist Security Info=False;User ID=pt20220505;Password=59Db7194497d;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }
    }
}