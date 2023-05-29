using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Xrm.Sdk.Async;
using Microsoft.Xrm.Sdk.Messages;
using log4net;


namespace ConsoleApp1
{
    class Program
    {
   
        static async Task Main(string[] args)
        {
            await UpdateData.Execute();
            Console.ReadKey();

        }
    }
}
