using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Async;
using Newtonsoft.Json.Linq;
using log4net;
using Google.Apis.Requests;
using Microsoft.VisualStudio.Services.Common;
using static System.Net.WebRequestMethods;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ConsoleApp1
{
    class Common
    {
        protected static readonly ILog logger = LogManager.GetLogger(typeof(Program));
        protected static IOrganizationService service = GetService();
        readonly static string baseUrl = "https://data.gov.il/api/3/action/datastore_search?resource_id=";
        readonly static string requestId = ConfigurationManager.AppSettings["requestId"];

        protected static readonly Dictionary<string, string> Urls = new Dictionary<string, string>
        {

            {"getAllManufacturers", $"{baseUrl}{requestId}&limit=100000&fields=tozeret_nm,tozeret_cd,tozeret_eretz_nm&distinct=true&sort=tozeret_cd%20asc"},
            {"getAllCountries", $"{baseUrl}{requestId}&fields=tozeret_eretz_nm&distinct=true"},
            {"getAllModels", baseUrl+requestId+"&limit=100000&distinct=true&filters={%22shnat_yitzur%22:[%222022%22,%20%222023%22,%20%222021%22,%20%222020%22],%22sug_degem%22:%22P%22}&fields=tozeret_cd,tozeret_nm,degem_nm,degem_cd,kinuy_mishari,shnat_yitzur" }
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
            Guid leadId = new Guid("ae90696b-de68-4c87-a606-4ff27918558e"); //Exist Lead From System
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
        protected static async Task<JObject> HttpRequest(string url)
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
                logger.Warn($"Request failed with status code {response.StatusCode}.");
                throw new Exception($"Request failed with status code {response.StatusCode}.");
            }
        }
        protected static async Task<ExecuteMultipleResponse> MultipleCreateRequests<T>(IOrganizationService service, List<T> requests) where T : OrganizationRequest
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

        protected static void HandlingMultipleResponse(ExecuteMultipleResponse response, int countOfRequest,int numburPackage)
        {

            if (!response.IsFaulted && response.Responses.Count == countOfRequest)
            {
                logger.Info($"All requests ({countOfRequest}) were successful : Package numbur:{numburPackage}.");
            }
            else
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (response.Responses[i].Fault != null)
                    {
                        logger.Info($"Request {i + 1} in package {numburPackage} failed with error code:,\n" +
                            $" {response.Responses[i].Fault.ErrorCode}.");
                    }
                }
            }
        }

        protected static async Task<ExecuteMultipleResponse> SplitListRequestsAndGetResponse(List<CreateRequest> requests)
        {
            ExecuteMultipleResponse response;
            if (requests.Count == 0)
            {
                response = await MultipleCreateRequests(service, requests);
                return response;
            }
            response = null;
            int maxRequests = 1000; int numburPackage = 0;
            while (requests.Any())
            {
                numburPackage++;
                var batchRequests = requests.Take(maxRequests).ToList();
                requests = requests.Skip(maxRequests).ToList();
                var batchResponse = await MultipleCreateRequests(service, batchRequests);
                HandlingMultipleResponse(batchResponse, batchRequests.Count, numburPackage);
                if (response == null)
                {
                    response = batchResponse;
                }
                else
                {
                    response.Responses.AddRange(batchResponse.Responses);
                }
            }
            return response;
        }

        protected static Dictionary<Guid, string> GetAllManufacturer()
        {
            var manufacturerInSystem = new Dictionary<Guid, string>();
            try
            {
                QueryExpression query = new QueryExpression("new_manufacturers");
                query.ColumnSet = new ColumnSet("new_code_manufacturer", "new_manufacturersid");
                query.AddOrder("new_code_manufacturer", OrderType.Ascending);
                EntityCollection ec = service.RetrieveMultiple(query);
                if (ec != null && ec.Entities.Count > 0)
                {
                    foreach (var entity in ec.Entities)
                    {
                        string code = (string)entity.Attributes["new_code_manufacturer"];
                        Guid guidManufacturer = entity.Id; 
                        manufacturerInSystem.Add(guidManufacturer, code); 
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return manufacturerInSystem;
        }

        protected static Dictionary<Guid, string> GetAllCountries()
        {
            var countriesInSystem = new Dictionary<Guid, string>();
            try
            {
                QueryExpression query = new QueryExpression("new_countries");
                query.ColumnSet = new ColumnSet("new_name", "new_countriesid");
                EntityCollection ec = service.RetrieveMultiple(query);
                if (ec != null && ec.Entities.Count > 0)
                {
                    foreach (var entity in ec.Entities)
                    {
                        string name = (string)entity.Attributes["new_name"];
                        Guid id = entity.Id;
                        countriesInSystem.Add(id, name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return countriesInSystem;
        }

        protected static Dictionary<Guid, string> GetAllModels()
        {
            var allModelsInCRM = new Dictionary<Guid, string>();
            try
            {
                QueryExpression query = new QueryExpression("new_models");
                query.ColumnSet.AddColumns("new_name", "new_modelsid");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    Count = 5000
                };
                EntityCollection results;
                do
                {
                    results = service.RetrieveMultiple(query);

                    if (results!=null&& results.Entities.Count > 0)
                    {
                        foreach (var entity in results.Entities)
                        {
                            string modelFullNameUnique = (string)entity.Attributes["new_name"];
                            Guid modelGuid = (Guid)entity.Attributes["new_modelsid"];
                            allModelsInCRM.Add(modelGuid, modelFullNameUnique);
                        }
                    }

                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = results.PagingCookie;
                } while (results.MoreRecords);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return allModelsInCRM;
        }
        protected static bool DeleteAllRecords(string entityName)
        {
            int delete = 0;
            var query = new QueryExpression(entityName);

            var entities = service.RetrieveMultiple(query).Entities;

            foreach (var entity in entities)
            {
                delete++;
                service.Delete(entity.LogicalName, entity.Id);
                if(delete > 3291)  break; 
            }
            Console.WriteLine($"DELETE {delete}");
            return true;
        } //FOR TESTING


    }
}
