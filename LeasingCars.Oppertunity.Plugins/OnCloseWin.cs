using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeasingCars.Oppertunity.Plugins
{


    public class OnCloseWin : IPlugin
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
                //check that this is a winning opportunity
                if (context.PrimaryEntityName == "opportunity" && context.MessageName == "Win")
                {
                    Entity opportunity = (Entity)context.InputParameters["OpportunityClose"];
                    Guid opportunityId = ((EntityReference)opportunity.Attributes["opportunityid"]).Id;
                    opportunity = GetOpportunityEntityWithFeilds(opportunityId, service);
                    CreateDriverInCar(opportunity, service);
                }
            }
            catch (Exception err)
            {
                tracingService.Trace($"Message:{err.Message} trace:{err.StackTrace}");
            }

        }
        private Entity GetOpportunityEntityWithFeilds(Guid opportunityId, IOrganizationService service)
        {

            ColumnSet columns = new ColumnSet("new_first_name_driver", "new_last_name_driver", "new_id_driver", "new_mail_driver",
                "new_phone_driver", "new_car_of_opportunity", "parentaccountid");
            Entity opportunity = service.Retrieve("opportunity", opportunityId, columns);
            return opportunity;
        }
        private void CreateDriverInCar(Entity opportunity, IOrganizationService service)
        {

            Entity newDriverInCar = new Entity("new_drivers");
            newDriverInCar.Attributes.Add("new_id_driver", opportunity.Attributes["new_id_driver"]);
            newDriverInCar.Attributes.Add("new_first_name", opportunity.Attributes["new_first_name_driver"]);
            newDriverInCar.Attributes.Add("new_last_name", opportunity.Attributes["new_last_name_driver"]);
            newDriverInCar.Attributes.Add("new_full_name", opportunity.Attributes["new_first_name_driver"] + " " + opportunity.Attributes["new_last_name_driver"]);
            newDriverInCar.Attributes.Add("new_mail", opportunity.Attributes["new_mail_driver"]);
            newDriverInCar.Attributes.Add("new_phone", opportunity.Attributes["new_phone_driver"]);
            newDriverInCar.Attributes.Add("new_opportunity_of_driver", new EntityReference("opportunity",opportunity.Id));
            newDriverInCar.Attributes.Add("new_car_of_driver", opportunity.Attributes["new_car_of_opportunity"]);
            newDriverInCar.Attributes.Add("new_company_of_driver", opportunity.Attributes["parentaccountid"]);
            service.Create(newDriverInCar);
        }
    }

}
