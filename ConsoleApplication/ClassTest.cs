
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleApplication1
{
    class ClassTest
    {

        IOrganizationService service;
        public OrganizationServiceContext svcContext = null;
        public ClassTest(IOrganizationService m_service)
        {
            service = m_service;
            svcContext = new OrganizationServiceContext(service);
        }

        public void GetLeadById(Guid leadId)
        {       
            Entity ent = service.Retrieve("lead", leadId, new ColumnSet(true));
        }


    }
}
