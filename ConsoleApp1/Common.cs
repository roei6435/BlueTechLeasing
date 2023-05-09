﻿using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Async;
using Newtonsoft.Json.Linq;
using System.Web;

namespace ConsoleApp1
{
    public class Common
    {
        public static IOrganizationService service = GetService();
        public static readonly Dictionary<string, string> Urls = new Dictionary<string, string>
        {
            {"getAllManufacturers", "https://data.gov.il/api/3/action/datastore_search?resource_id=142afde2-6228-49f9-8a29-9b6c3a0cbe40&limit=100000&fields=tozeret_nm,tozeret_cd,tozeret_eretz_nm&distinct=true&sort=tozeret_cd%20asc"},
            {"getAllCountries", "https://data.gov.il/api/3/action/datastore_search?resource_id=142afde2-6228-49f9-8a29-9b6c3a0cbe40&fields=tozeret_eretz_nm&distinct=true"},
            {"getAllModels", "https://data.gov.il/api/3/action/datastore_search?resource_id=142afde2-6228-49f9-8a29-9b6c3a0cbe40&limit=2000&distinct=true&filters={%22shnat_yitzur%22:%222022%22,%22sug_degem%22:%22P%22}&fields=tozeret_cd,tozeret_nm,degem_nm,degem_cd,kinuy_mishari,shnat_yitzur" }
        };

        public static IOrganizationService GetService()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var client = new CrmServiceClient(connectionString);
            if (client.IsReady == false)
            {
                throw new Exception($"Crm Service Client is not ready: {client.LastCrmError}", client.LastCrmException);
            }
            service = client;
            return client;

        }
        public static bool CheckConnectToServiceSucssesfully()
        {
            IOrganizationService service = GetService();
            Guid leadId = new Guid("6d7b103d-53c4-ed11-9886-000d3add8f0a"); //Exist Lead From System
            try
            {
                Entity lead = service.Retrieve("lead", leadId, new ColumnSet(true));
                return true;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
        public static async Task<JObject> HttpRequest(string url)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string stringResponse= await response.Content.ReadAsStringAsync();
                return JObject.Parse(stringResponse);
            }
            else
            {
                throw new Exception($"Request failed with status code {response.StatusCode}.");
            }
        }
        public static async Task<ExecuteMultipleResponse> MultipleCreateRequests<T>(IOrganizationService service, List<T> requests) where T : OrganizationRequest
        {
            var executeMultipleRequest = new ExecuteMultipleRequest()
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                }
            };

            foreach (var request in requests)
            {
                executeMultipleRequest.Requests.Add(request);
            }

            var response = await service.ExecuteAsync(executeMultipleRequest);

            return (ExecuteMultipleResponse)response;
        }
        public static void HandlingMultipleResponse(ExecuteMultipleResponse response, int countOfRequest)
        {
            if (!response.IsFaulted && response.Responses.Count == countOfRequest)
            {
                Console.WriteLine("All requests were successful.");
            }
            else
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (response.Responses[i].Fault != null)
                    {
                        Console.WriteLine($"Request {i + 1} failed with error code {response.Responses[i].Fault.ErrorCode}.");
                    }
                }
            }
        }


        public static Dictionary<string, Guid> GetAllManufacturer()
        {
            var manufacturerInSystem = new Dictionary<string, Guid>();
            try
            {
                QueryExpression query = new QueryExpression("roe_manufacturer");
                query.ColumnSet = new ColumnSet("roe_code", "roe_manufacturerid");
                query.AddOrder("roe_code", OrderType.Ascending);
                EntityCollection ec = Common.service.RetrieveMultiple(query);
                if (ec != null && ec.Entities.Count > 0)
                {
                    foreach (var entity in ec.Entities)
                    {
                        string code = (string)entity.Attributes["roe_code"];
                        Guid id = entity.Id;
                        manufacturerInSystem.Add(code, id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return manufacturerInSystem;
        }

        public static Dictionary<string, Guid> GetAllCountries()
        {
            var countriesInSystem = new Dictionary<string, Guid>();
            try
            {
                QueryExpression query = new QueryExpression("roe_countries");
                query.ColumnSet = new ColumnSet("roe_name", "roe_countriesid");
                EntityCollection ec = Common.service.RetrieveMultiple(query);
                if (ec != null && ec.Entities.Count > 0)
                {
                    foreach (var entity in ec.Entities)
                    {
                        string name = (string)entity.Attributes["roe_name"];
                        Guid id = entity.Id;
                        countriesInSystem.Add(name, id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return countriesInSystem;
        }

        public static Dictionary<string, string> DictionaryCodesMM()
        {
            var modelsInSystem = new Dictionary<string, string>();
            try
            {
                QueryExpression query = new QueryExpression("roe_model");
                query.ColumnSet = new ColumnSet("roe_code");

                LinkEntity manufacturerLink = query.AddLink("roe_manufacturer", "roe_manufacturer", "roe_manufacturerid");
                manufacturerLink.Columns = new ColumnSet("roe_code");

                EntityCollection ec = Common.service.RetrieveMultiple(query);

                if (ec != null && ec.Entities.Count > 0)
                {
                    foreach (var entity in ec.Entities)
                    {
                        string modelCode = (string)entity.Attributes["roe_code"];
                        AliasedValue aliasedValue = entity.Attributes["roe_manufacturer1.roe_code"] as AliasedValue;
                        string manufacturerCode = aliasedValue.Value.ToString();
                        modelsInSystem.Add(modelCode, manufacturerCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return modelsInSystem;
        }

        public static bool DeleteAllRecords(string entityName)
        {
            var query = new QueryExpression(entityName);

            var entities = Common.service.RetrieveMultiple(query).Entities;

            foreach (var entity in entities)
            {
                Common.service.Delete(entity.LogicalName, entity.Id);
            }
            return true;
        }


    }
}