using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class UpdateData
    {
        //מדינות
        public static async Task<ExecuteMultipleResponse> UpdateCountries()
        {
            var json = await Common.HttpRequest(Common.Urls["getAllCountries"]);
            var records = json["result"]["records"];
            var requests = new List<CreateRequest>();
            var allContries = Common.GetAllCountries();

            foreach (var record in records)
            {
                string countryName = (string)record["tozeret_eretz_nm"];
                if (countryName!=null&& !allContries.ContainsKey(countryName))
                {
                    Entity countryToCreate = new Entity("roe_countries");
                    countryToCreate.Attributes.Add("roe_name", countryName);
                    var request = new CreateRequest { Target = countryToCreate };
                    requests.Add(request);
                }

            }
            var response = await Common.MultipleCreateRequests(Common.service, requests);
            Common.HandlingMultipleResponse(response, requests.Count);
            Console.WriteLine($"Created {requests.Count} records");
            return response;
        }
        //יצרנים
        public static async Task<ExecuteMultipleResponse> UpdateManufacturers()
        {
            var json = await Common.HttpRequest(Common.Urls["getAllManufacturers"]);
            var records = json["result"]["records"];
            var requests = new List<CreateRequest>();
            var allContries = Common.GetAllCountries();
            var allManufacturers = Common.GetAllManufacturer();
            foreach (var record in records)
            {
                string codeManufacturer = (string)record["tozeret_cd"];
                if (!allManufacturers.ContainsKey(codeManufacturer))
                {
                    Entity newManufacturer = BuildManufacturer(record, allContries);
                    if (newManufacturer != null)
                    {
                        var request = new CreateRequest { Target = newManufacturer };
                        requests.Add(request);
                    }
                }

            }
            var response = await Common.MultipleCreateRequests(Common.service, requests);
            Common.HandlingMultipleResponse(response, requests.Count);
            Console.WriteLine($"Created {requests.Count} records");
            return response;
        }
        //דגמים
        public static async Task<ExecuteMultipleResponse> UpdateModels()
        {
            int countFeild=0;
            try
            {
                var json = await Common.HttpRequest(Common.Urls["getAllModels"]);
                var records = json["result"]["records"];
                var requests = new List<CreateRequest>();
                var allManufacturers = Common.GetAllManufacturer();
                var allCodes = Common.DictionaryCodesMM();

                foreach (var record in records)
                {
                    string codeModel = (string)record["degem_cd"];
                    string codeManufacturer = (string)record["tozeret_cd"];
                    bool existSameCodeOfModel = allCodes.ContainsKey(codeModel);
                    if (!existSameCodeOfModel || existSameCodeOfModel && allCodes[codeModel] != codeManufacturer)
                    {
                        Entity newModel = BuildModel(record, allManufacturers);
                        if (newModel != null)
                        {
                            var request = new CreateRequest { Target = newModel };
                            requests.Add(request);
                        }
                    }
                }
                var response = await Common.MultipleCreateRequests(Common.service, requests);
                Common.HandlingMultipleResponse(response, requests.Count);
                Console.WriteLine($"Created {requests.Count} records");
                return response;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                countFeild++;
            }
            Console.WriteLine($"Count feild:{countFeild}");
            return null;

        }

        public static Entity BuildManufacturer(JToken record, Dictionary<string, Guid> dictOfContries) 
        {
            Entity manufacturerToCreate;
            try
            {
                manufacturerToCreate = new Entity("roe_manufacturer");
                manufacturerToCreate.Attributes.Add("roe_code", (string)record["tozeret_cd"]);
                manufacturerToCreate.Attributes.Add("roe_name", (string)record["tozeret_nm"]);
                string countryName = (string)record["tozeret_eretz_nm"];
                if (dictOfContries.ContainsKey(countryName))
                {
                    EntityReference countryRef = new EntityReference("roe_countries", dictOfContries[countryName]);
                    manufacturerToCreate.Attributes.Add("roe_countries", countryRef);
                }

            }
            catch { 
                manufacturerToCreate= null;
            }
            return manufacturerToCreate;
          
        }
        public static Entity BuildModel(JToken record,Dictionary<string, Guid> dictOfManufacturer)
        {
            Entity modelToCreate;
            try
            {
                 modelToCreate = new Entity("roe_model");
                modelToCreate.Attributes.Add("roe_year", (string)record["shnat_yitzur"]);
                modelToCreate.Attributes.Add("roe_name", (string)record["kinuy_mishari"] +" - "+ (string)record["degem_nm"]);
                modelToCreate.Attributes.Add("roe_code", (string)record["degem_cd"]);
                string codeManufacturer = (string)record["tozeret_cd"];
                if (dictOfManufacturer.ContainsKey(codeManufacturer))
                {
                    EntityReference manufacturerRef = new EntityReference("roe_manufacturer", dictOfManufacturer[codeManufacturer]);
                    modelToCreate.Attributes.Add("roe_manufacturer", manufacturerRef);
                }

            }
            catch
            {
                modelToCreate = null;
            }
            return modelToCreate;

        }














    }
}
