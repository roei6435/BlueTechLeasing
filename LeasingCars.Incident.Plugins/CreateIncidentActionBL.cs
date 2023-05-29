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
        public string CaseType { get; set; }
    }
    public class OutputArgumemts
    {
        public bool Success { get; set; }
        public string Discription { get; set; }
        public EntityReference IncidentRef { get; set; }
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
            output.Success = false;
            output.IncidentRef = null;
            try
            {
                Guid car = FindCarByNumbur(inputArgumemts.CarNumber);
                if (car == Guid.Empty)
                {
                    output.Discription = "Not exist car with this car numbur.";
                    return output;
                }

                Guid driver = FindDriver(inputArgumemts.IdNumber, car);
                if (driver == Guid.Empty)
                {
                    output.Discription = "This car does not have a driver in the system.";
                    return output;
                }
                Guid company = FindCompany(inputArgumemts.BusinessNumber, driver);

                OptionSetValue caseType = FindOptionValueCodeByNameOfCase(inputArgumemts.CaseType);

                Guid caseCreated = CreateCase(car, driver, company, caseType, inputArgumemts.DiscriptionCase);

                output.Success = true;
                output.Discription = "Case created with sucssesfully.";
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
        private OptionSetValue FindOptionValueCodeByNameOfCase(string caseType)
        {
            switch (caseType)
            {
                case "תקלה":
                    return new OptionSetValue(1);
                case "תאונה":
                    return new OptionSetValue(2);
                case "טסט":
                    return new OptionSetValue(3);
                case "טיפול":
                    return new OptionSetValue(4);
                default:
                    return new OptionSetValue(5);
            }

        }
        private Guid FindCompany(string businessNumber, Guid driver)
        {
            // A logical requirement:
            //1.find company by bussinuess numbur.
            //2.if not be existing, find by driver.

            QueryExpression query = new QueryExpression("new_drivers");
            query.Criteria.AddCondition(new ConditionExpression("new_driversid", ConditionOperator.Equal, driver));
            query.ColumnSet = new ColumnSet("new_company_of_driver");

            LinkEntity accountLink = new LinkEntity("new_drivers", "account", "new_company_of_driver", "accountid", JoinOperator.LeftOuter);
            accountLink.LinkCriteria.AddCondition(new ConditionExpression("new_company_id", ConditionOperator.Equal, businessNumber));
            accountLink.Columns.AddColumn("accountid"); 

          
            query.LinkEntities.Add(accountLink);

            EntityCollection results = service.RetrieveMultiple(query);

            Guid company = Guid.Empty;
            if (results.Entities.Count > 0)
            {
                if (results.Entities[0].Attributes.Contains("account1.accountid"))   //אם מצאתי לפי ח.פ
                {
                     AliasedValue findByBussnissesNum= (AliasedValue)results.Entities[0].Attributes["account1.accountid"];
                     company= (Guid)findByBussnissesNum.Value;
                }
                else
                {
                    //אחרת נמצא לפי נהג
                    EntityReference findByDriver = results.Entities[0].GetAttributeValue<EntityReference>("new_company_of_driver");
                    company= findByDriver.Id;
                }
            }

            return company;
        }
        private Guid FindDriver(string driverId,Guid car)   
        {
               // A logical requirement:
               //1.by driver id
               //2.if not be existing, find by car.
            QueryExpression query = new QueryExpression("new_drivers");


            //GET ALL DRIVER WITH THIS ID AND THIS CAR.
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(new ConditionExpression("new_id_driver", ConditionOperator.Equal, driverId));
            filter.AddCondition(new ConditionExpression("new_car_of_driver", ConditionOperator.Equal, car));

            query.Criteria.AddFilter(filter);

            query.ColumnSet = new ColumnSet("new_id_driver", "new_car_of_driver");

            EntityCollection releventDrivers = service.RetrieveMultiple(query);

            Entity driver = null;
            if (releventDrivers.Entities.Count > 0)
            {  
               driver = 
               releventDrivers.Entities.FirstOrDefault
               (dr =>dr.Attributes["new_id_driver"].ToString() == driverId           //find by driver id.
               ||
               (dr.Attributes.Contains("new_car_of_driver") &&                       //if not be existing,
               ((EntityReference)dr.Attributes["new_car_of_driver"]).Id == car));    // find by car.  

            } 
            if(driver is null)
            {
                return Guid.Empty;
            }
            return driver.Id;
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
