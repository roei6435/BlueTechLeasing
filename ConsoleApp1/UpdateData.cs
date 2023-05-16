using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace ConsoleApp1
{
    class UpdateData:Common
    {
        public static async Task Execute()
        {
            
            logger.Info("Waiting to connected...");
            if (CheckConnectToServiceSucssesfully())
            {
                logger.Info($"Connected successfully. Job started.");

                var functions = new Func<Task<ExecuteMultipleResponse>>[] { UpdateCountries, UpdateManufacturers, UpdateModels };

                foreach (var func in functions)
                {
                    ExecuteMultipleResponse response = await func();

                    if (response.IsFaulted)
                    {
                        logger.Warn($"Faulted response - {func.Method.Name}");
                    }
                }
                logger.Info("Job finished running.\n============================================================================================================================================================================="); 

            }
            else
            {
                logger.Info("Feild connect to server.");
            }

    
        }


        //מדינות
        private static async Task<ExecuteMultipleResponse> UpdateCountries()
        {

            var json = await HttpRequest(Urls["getAllCountries"]);
            var records = json["result"]["records"];
            var requests = new List<CreateRequest>();
            var allContries = GetAllCountries();

            foreach (var record in records)
            {
                try
                {
                    string countryName = (string)record["tozeret_eretz_nm"];
                    if (countryName != null && !allContries.ContainsValue(countryName))
                    {
                        Entity countryToCreate = new Entity("roe_countries");
                        countryToCreate.Attributes.Add("roe_name", countryName);
                        var request = new CreateRequest { Target = countryToCreate };
                        requests.Add(request);
                    }
                }
                catch (Exception err)
                {
                    logger.Error($"Error in loop: record:{record} exeption:{err.Message}");
                }


            }
            var response = await MultipleCreateRequests(service, requests);
            HandlingMultipleResponse(response, requests.Count,1);
            logger.Info($"WE NEED TO UPDATED: {requests.Count} NEW  COUNTRIES.");
            return response;
        }
        //יצרנים
        private static async Task<ExecuteMultipleResponse> UpdateManufacturers()
        {
            var json = await HttpRequest(Urls["getAllManufacturers"]);
            var records = json["result"]["records"];
            var requests = new List<CreateRequest>();
            var allContries = GetAllCountries();
            var allManufacturers = GetAllManufacturer();
            foreach (var record in records)
            {
                try
                {
                    string codeManufacturer = (string)record["tozeret_cd"];
                    if (!allManufacturers.ContainsValue(codeManufacturer))
                    {
                        Entity newManufacturer = BuildManufacturer(record, allContries);
                        if (newManufacturer != null)
                        {
                            var request = new CreateRequest { Target = newManufacturer };
                            requests.Add(request);
                        }
                    }
                }
                catch (Exception err)
                {
                    logger.Error($"Error in loop: record:{record} exeption:{err.Message}");
                }


            }
            var response = await MultipleCreateRequests(service, requests);
            HandlingMultipleResponse(response, requests.Count,1);
            logger.Info($"WE NEED TO UPDATED: {requests.Count} NEW NANUFACTORER.");
            return response;
        }
        //דגמים
        private static async Task<ExecuteMultipleResponse> UpdateModels()
        {
            var json = await HttpRequest(Urls["getAllModels"]);
            var records = json["result"]["records"];
            var requests = new List<CreateRequest>();
            var allManufacturerFromCRM = GetAllManufacturer();
            var allModelsFromCRM = GetAllModels();
            var recordAlreadyCreated = new List<string>();
            foreach (var record in records)
            {
                try
                {
                    string modelFullNameUnique = GetMFullNameModelUnique(record);
                    if (!recordAlreadyCreated.Contains(modelFullNameUnique) && !allModelsFromCRM.ContainsValue(modelFullNameUnique))
                    {

                        string manufacturerCode = (string)record["tozeret_cd"];
                        Guid manufacturerGuid = allManufacturerFromCRM.FirstOrDefault(x => x.Value == manufacturerCode).Key;
                        Entity newModel = BuildModel(record, manufacturerGuid);
                        if (newModel != null)
                        {
                            var request = new CreateRequest { Target = newModel };
                            requests.Add(request);
                            recordAlreadyCreated.Add(modelFullNameUnique);

                        }
                    }
                }
                catch (Exception err)
                {
                    logger.Error($"Error in loop: record:{record} exeption:{err.Message}");
                }

            }
            logger.Info($"WE NEED TO UPDATED: {requests.Count} NEW MODELS");
            ExecuteMultipleResponse response = await SplitListRequestsAndGetResponse(requests);
            HandlingMultipleResponse(response, requests.Count, 1);
            return response;

        }
        private static Entity BuildManufacturer(JToken record, Dictionary<Guid,string> dictOfContries) 
        {
            Entity manufacturerToCreate;
            try
            {
                manufacturerToCreate = new Entity("roe_manufacturer");
                manufacturerToCreate.Attributes.Add("roe_code", (string)record["tozeret_cd"]);
                manufacturerToCreate.Attributes.Add("roe_name", (string)record["tozeret_nm"]);
                string countryName = (string)record["tozeret_eretz_nm"];
                Guid guidContry  = dictOfContries.FirstOrDefault(x => x.Value == countryName).Key;
                if (guidContry!= Guid.Empty)
                {
                    EntityReference countryRef = new EntityReference("roe_countries", guidContry);
                    manufacturerToCreate.Attributes.Add("roe_countries", countryRef);
                }

            }
            catch { 
                manufacturerToCreate= null;
            }
            return manufacturerToCreate;
          
        }
        private static Entity BuildModel(JToken record,Guid guidManufacturer)
        {

            Entity modelToCreate;
            try
            {
                modelToCreate = new Entity("roe_model");
                string modelNameUnique = GetMFullNameModelUnique(record);
                modelToCreate.Attributes.Add("roe_name", modelNameUnique);
                modelToCreate.Attributes.Add("roe_year", (string)record["shnat_yitzur"]);
                modelToCreate.Attributes.Add("roe_code", (string)record["degem_cd"]);
                modelToCreate.Attributes.Add("roe_trade_name", (string)record["kinuy_mishari"]);
                EntityReference manufacturerRef = new EntityReference("roe_manufacturer", guidManufacturer);
                modelToCreate.Attributes.Add("roe_manufacturer", manufacturerRef);

            }
            catch
            {
                modelToCreate = null;
            }
            return modelToCreate;

        }
        private static string GetMFullNameModelUnique(JToken record)
        {
            string tradeName = (string)record["kinuy_mishari"];
            string modelNmae = (string)record["degem_nm"];
            string modelCode = (string)record["degem_cd"];
            string manufacturerCode = (string)record["tozeret_cd"];
            string year = (string)record["shnat_yitzur"];
            string fullNameModelUnique = $"{tradeName}-{modelNmae}-{manufacturerCode}-{year}";
            return fullNameModelUnique;

        }
















    }
}
