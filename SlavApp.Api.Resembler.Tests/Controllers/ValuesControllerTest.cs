using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlavApp.Api.Resembler;
using SlavApp.Api.Resembler.Controllers;
using SlavApp.Api.Resembler.Providers;
using Moq;
using SlavApp.Api.Resembler.Services;
using System.Threading.Tasks;
using System.Web;
using SlavApp.Api.Resembler.Models;

namespace SlavApp.Api.Resembler.Tests.Controllers
{
    [TestClass]
    public class ValuesControllerTest
    {
        private static Mock<IVersionProvider> _versionProvider;
        private static Mock<IResemblerUsageService> _usageService;

        private static VersionController controller;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            _versionProvider = new Mock<IVersionProvider>();
            _usageService = new Mock<IResemblerUsageService>();
            controller = new VersionController(_versionProvider.Object, _usageService.Object);
        }

        [TestMethod]
        public async Task Get()
        {
            // Arrange
            const string Hash = "hash";
            const string V = "0.0.0";
            controller.Request = new HttpRequestMessage();

            _versionProvider.Setup(x => x.GetVersion()).Returns(Task.FromResult(new VersionModel() { V = V }));
            
            // Act
            string result = await controller.Get(Hash);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(V, result);

            _usageService.Verify(x => x.RegisterUsage(It.IsAny<string>(), It.Is<string>(h => h == Hash)), Times.Once);
        }
    }
}
