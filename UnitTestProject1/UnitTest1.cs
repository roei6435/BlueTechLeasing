using ConsoleApp1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task DELETE()
        {
            Common.DeleteAllRecords("roe_manufacturer");//roe_model  //roe_manufacturer

        }
        [TestMethod]
        public async Task MANUFACURERS()
        {
            var res = await UpdateData.UpdateManufacturers();

        }
        [TestMethod]
        public async Task MODELS()
        {
            var res = await UpdateData.UpdateModels();
        }
        [TestMethod]
        public async Task CONTRIES()
        {
            var res = await UpdateData.UpdateCountries();
        }



    }
}
