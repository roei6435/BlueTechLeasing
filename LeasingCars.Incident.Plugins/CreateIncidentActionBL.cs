using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static LeasingCars.Incident.Plugins.CreateIncidentAction;

namespace LeasingCars.Incident.Plugins
{
    public class InputArgumemts
    {
        public string BusinessNumber { get; set; }
        public string IdNumber { get; set; }
        public string CarNumber { get; set; }
        public string DiscriptionCase { get; set; }
        public int CaseType { get; set; }
    }
    public class OutputArgumemts
    {
        public bool Success { get; set; } = false;
        public string Discription { get; set; }
        public EntityReference IncidentRef { get; set; }=null;
    }
    internal class Descriptions
    {
        public string NotExistCar { get; } = "Not exist car with this car numbur.";
        public string NotExistDriverForThisCar { get; } = "This car does not have a driver in the system.";
        public string CaseCreatedWithSucssesfully{ get; } = "Case created with sucssesfully.";
    }
    public class CreateIncidentActionBL
    {
        private readonly IOrganizationService service;
        public CreateIncidentActionBL(IOrganizationService service)
        {
            this.service = service;
        }

        public OutputArgumemts ExecuteCreateCase(InputArgumemts inputArgumemts)
        {
            OutputArgumemts output = new OutputArgumemts();
            Descriptions descriptions= new Descriptions();
            try
            {
                Guid car = FindCarByNumbur(inputArgumemts.CarNumber);
                if (car == Guid.Empty)
                {
                    output.Discription = descriptions.NotExistCar;
                    return output;
                }

                Entity driver = FindDriver(inputArgumemts.IdNumber, car);
                if (driver.Id == Guid.Empty)
                {
                    output.Discription = descriptions.NotExistDriverForThisCar;
                    return output;
                }


                Guid company = FindCompany(inputArgumemts.BusinessNumber, driver);

                OptionSetValue caseType = FindOptionValueCodeByNameOfCase(inputArgumemts.CaseType);

                Guid caseCreated = CreateCase(car, driver.Id, company, caseType, inputArgumemts.DiscriptionCase);

                output.Success = true;
                output.Discription = descriptions.CaseCreatedWithSucssesfully;
                output.IncidentRef = new EntityReference("incident", caseCreated);

                return output;

            }
            catch (Exception ex)
            {
                output.Discription = ex.Message;
                return output;

            }
        }
        private Guid CreateCase(Guid car, Guid driver, Guid company, OptionSetValue caseType,string discription)
        {
            Entity newCase = new Entity("incident");
            newCase.Attributes.Add("title", discription.Substring(0, 12)+"...");
            newCase.Attributes.Add("new_car_of_case", new EntityReference("new_cars", car));
            newCase.Attributes.Add("new_driver_of_case", new EntityReference("new_drivers", driver));
            newCase.Attributes.Add("customerid", new EntityReference("account", company));
            newCase.Attributes.Add("new_type_case", caseType);
            newCase.Attributes.Add("description", discription);

            return service.Create(newCase);
        }
        private OptionSetValue FindOptionValueCodeByNameOfCase(int caseType)
        {
            switch (caseType)
            {
                case 1:                 //fault
                    return new OptionSetValue(1);
                case 2:                 //accident
                    return new OptionSetValue(2);
                case 3:                 //test
                    return new OptionSetValue(3);
                case 4:                 //care
                    return new OptionSetValue(4);
                default:                //other
                    return new OptionSetValue(5);
            }

        }
        private Guid FindCompany(string businessNumber, Entity driver)
        {
            //find company by driver
            //else check the business number and send query to service
            Guid company = Guid.Empty;
            if (driver.Contains("new_company_of_driver") && driver["new_company_of_driver"] is EntityReference)
            {
                company = ((EntityReference)driver["new_company_of_driver"]).Id;
            }
            else
            {
                if (driver.Contains("account1.new_company_id") && ((AliasedValue)driver["account1.new_company_id"]).Value != null)
                {
                    businessNumber = ((AliasedValue)driver["account1.new_company_id"]).Value.ToString();
                }
                QueryExpression query = new QueryExpression("account");
                query.Criteria.AddCondition(new ConditionExpression("new_company_id", ConditionOperator.Equal, businessNumber));
                query.ColumnSet = new ColumnSet(false);

                EntityCollection results = service.RetrieveMultiple(query);
                if (results != null && results.Entities.Count > 0)
                {
                    company = results.Entities.FirstOrDefault().Id;
                }
            }

            return company;
        }
        public Entity FindDriver(string driverId, Guid car)
        {
            QueryExpression query = new QueryExpression("new_drivers");

            //GET ALL DRIVER WITH THIS ID AND THIS CAR.
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(new ConditionExpression("new_id_driver", ConditionOperator.Equal, driverId));
            filter.AddCondition(new ConditionExpression("new_car_of_driver", ConditionOperator.Equal, car));

            query.Criteria.AddFilter(filter);

            query.ColumnSet = new ColumnSet("new_id_driver", "new_car_of_driver", "new_company_of_driver");

            // Expand to retrieve the linked entity
            query.LinkEntities.Add(new LinkEntity("new_drivers", "account", "new_company_of_driver", "accountid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("new_company_id"); // Add the column to retrieve from the linked entity

            EntityCollection releventDrivers = service.RetrieveMultiple(query);

            Entity driver = null;
            if (releventDrivers != null && releventDrivers.Entities.Count > 0)
            {
                driver = releventDrivers.Entities.FirstOrDefault();
            }
            return driver;
        }
        private Guid FindCarByNumbur(string carNumbur)
        {
            // A logical requirement:
            //1.find car by number car
            QueryExpression query = new QueryExpression("new_cars");
            query.Criteria.AddCondition(new ConditionExpression("new_car_numbur", ConditionOperator.Equal, carNumbur));
            query.ColumnSet = new ColumnSet(false);
            query.TopCount = 1;
            EntityCollection res = service.RetrieveMultiple(query);
            if (res.Entities.Count > 0)
            {
               return  res.Entities.First().Id;
            }
            return Guid.Empty;
        }




    }
}
