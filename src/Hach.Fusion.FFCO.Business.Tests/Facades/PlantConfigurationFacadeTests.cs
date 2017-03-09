using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Azure.Queue;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Hach.Fusion.Data.Database.Interfaces;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class PlantConfigurationFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private PlantConfigurationsFacade _facade;
        private Mock<IBlobManager> _blobManager;
        private Mock<IQueueManager> _queueManager;
        private Mock<IDocumentDbRepository<UploadTransaction>> _documentDbRepository;

        public PlantConfigurationFacadeTests()
        {
            MappingManager.Initialize();
        }

        [SetUp]
        public void Setup()
        {

            _queueManager = new Mock<IQueueManager>();
            _queueManager
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Delay(0));

            _blobManager = new Mock<IBlobManager>();
            _documentDbRepository = new Mock<IDocumentDbRepository<UploadTransaction>>();

            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockContext.Object.Users.Single(x=> x.UserName == "tnt01user").Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            _facade = new PlantConfigurationsFacade(_mockContext.Object, _blobManager.Object, _queueManager.Object, _documentDbRepository.Object);

        }

        [Test]
        public async Task When_Download_Succeeds()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name =="Dev Tenant 01").Id;
            var plantId = _mockContext.Object.Locations.Single(l => l.Name =="Plant_01").Id;

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(result.ErrorCodes, Is.Empty);

            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_Download_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name =="Dev Tenant 01").Id;
            var plantId = _mockContext.Object.Locations.Single(l => l.Name == "Plant_01").Id;

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-304");
            Assert.That(errorCode, Is.Not.Null);
            Assert.That(errorCode.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Download_EmptyPlantId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name =="Dev Tenant 01").Id;
            var plantId = Guid.Empty;

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_InvalidPlantId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name =="Dev Tenant 01").Id;
            var plantId = Guid.Parse("23F0B5B9-6F97-4150-A1F0-3E1B6EAEE29E");

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_EmptyTenantId_Fails()
        {
            var tenantId = Guid.Empty;
            var plantId = _mockContext.Object.Locations.Single(l => l.Name == "Plant_01").Id;

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_InvalidTenantId_Fails()
        {
            var tenantId = Guid.Parse("5066B0D1-CAC3-4719-9D17-0C80FE0C16A5");
            var plantId = _mockContext.Object.Locations.Single(l => l.Name == "Plant_01").Id;

            var result = await _facade.Download(tenantId, plantId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }
    }
}
