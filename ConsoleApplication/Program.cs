
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Internal;
using Microsoft.Graph;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    class Program
    { 
        static void Main(string[] args)
        {
            Console.WriteLine($"Started on {DateTime.Now}");

            ClassTest test = new ClassTest(GetService());
            test.GetLeadById(new Guid("6d7b103d-53c4-ed11-9886-000d3add8f0a"));




            Console.ReadKey();  
        }


        public static IOrganizationService GetService()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var client = new CrmServiceClient(connectionString);
            if (client.IsReady == false)
            {
                throw new Exception($"Crm Service Client is not ready: {client.LastCrmError}", client.LastCrmException);
            }
            return client;
        }
         
    }
}
