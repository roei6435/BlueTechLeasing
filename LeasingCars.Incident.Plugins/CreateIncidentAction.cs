using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LeasingCars.Incident.Plugins
{
    public class CreateIncidentAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("the plugin on running");
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                InputArgumemts input = new InputArgumemts();
                OutputArgumemts output = new OutputArgumemts();

                //get all arguments input from context to my local object.
                input.BusinessNumber = context.InputParameters.Contains("businessNumber") ? context.InputParameters["businessNumber"].ToString() : null;
                input.IdNumber = context.InputParameters.Contains("idNumber") ? context.InputParameters["idNumber"].ToString() : null;
                input.CarNumber = context.InputParameters.Contains("carNumber") ? context.InputParameters["carNumber"].ToString() : null;
                input.DiscriptionCase = context.InputParameters.Contains("discriptionCase") ? context.InputParameters["discriptionCase"].ToString() : null;
                input.CaseType = context.InputParameters.Contains("caseType") ?context.InputParameters["caseType"].ToString() : null;

                //call to BL layer and getting the output local object .
                CreateIncidentActionBL BL = new CreateIncidentActionBL(service);
                output = BL.ExecuteCreateCase(input);


                //assignment to output arguments of the context from the local object.
                context.OutputParameters["caseRef"] = output.IncidentRef;
                context.OutputParameters["successDiscription"]=output.Discription;
                context.OutputParameters["successStatus"] = output.Success;


            }
            catch (Exception err)
            {
                context.OutputParameters["caseRef"] = null;
                context.OutputParameters["successDiscription"] = err.Message;
                context.OutputParameters["successStatus"] = false;
            }

        }

    }
}
