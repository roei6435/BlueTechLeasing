using LeasingCars.Incident.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using static LeasingCars.Incident.Plugins.CreateIncidentAction;

namespace UnitTests
{
    [TestClass]
    public class ActionsBLTest
    {
        private readonly IOrganizationService _organizationService;

        public ActionsBLTest()
        {
            var connactionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var crmServiceClient = new CrmServiceClient(connactionString);
            _organizationService = crmServiceClient;
        }
        [TestMethod]
        public void TestExecuteFunction()           
        {

            var BL = new CreateIncidentActionBL(_organizationService);
            InputArgumemts inputArgumemts = new InputArgumemts();
            inputArgumemts.BusinessNumber = "559933";
            inputArgumemts.CarNumber = "418-7001-332";
            inputArgumemts.IdNumber = "250700311";
            inputArgumemts.CaseType = "טסט";
            inputArgumemts.DiscriptionCase = "הנהגת נוי בן דוד מצהירה על טסט הקרב עבור רכב הקיא שבליסינג אצל אאורה פיננסים";
            var result = BL.ExecuteCreateCase(inputArgumemts);
        }

        [TestMethod]
        //קריאה לאקשיין בבקשת WEB API 
        public void TestWithWebApiRequest()
        {
            OrganizationRequest request = new OrganizationRequest("new_Create_Case");
            request["businessNumber"] = "0000";
            request["idNumber"] = "00000";
            request["carNumber"] = "571500372";
            request["discriptionCase"] = "הנהג ליאור מורנו מצהיר על טיפול מתקרב ברכבו, סקודה אוקטביה, שבליסינג תפעולי עבור חברת קמביום.";
            request["caseType"] = "טיפול";

            // Call the custom action
            OrganizationResponse response = _organizationService.Execute(request);

            var caseId = response["caseRef"] is null ? Guid.Empty : ((EntityReference)response["caseRef"]).Id;
            bool success = (bool)response["successStatus"];
            string description = (string)response["successDiscription"];
            Console.WriteLine($"The action is over:\n " +
                $"action success:{success}\n" +
                $"description:{description}\n" +
                $"incident id in system: {caseId}\n");
        }





    }
}
