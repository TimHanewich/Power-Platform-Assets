using System;

namespace PSJ
{
    public class DataverseCredentialsProvider
    {
        public string Username {get; set;}
        public string Password {get; set;}
        public string Resource {get; set;}

        public DataverseCredentialsProvider()
        {
            Username = "<AZURE AD LOGIN HERE>";
            Password = "<PASSWORD HERE>";
            Resource = "https://<YOUR ORG>.crm.dynamics.com/";
        }
    }
}