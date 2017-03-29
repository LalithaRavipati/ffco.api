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
using Hach.Fusion.Core.Dtos;
using Hach.Fusion.Data.Database.Interfaces;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class OperationConfigurationFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private OperationConfigurationsFacade _facade;
        private Mock<IBlobManager> _blobManager;
        private Mock<IQueueManager> _queueManager;
        private Mock<IDocumentDbRepository<UploadTransaction>> _documentDbRepository;

        public OperationConfigurationFacadeTests()
        {
            MappingManager.Initialize();
        }

        [SetUp]
        public void Setup()
        {
            _queueManager = new Mock<IQueueManager>();
            _queueManager
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Delay(0)).Verifiable();

            _blobManager = new Mock<IBlobManager>();
            _blobManager.Setup(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new BlobStoreResult())).Verifiable();

            _documentDbRepository = new Mock<IDocumentDbRepository<UploadTransaction>>();

            _documentDbRepository.Setup(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()))
                .Returns(Task.FromResult(new Microsoft.Azure.Documents.Document())).Verifiable();

            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);

            var userIdClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockContext.Object.Users.Single(x => x.UserName == "tnt01user").Id.ToString());
            var tenantClaim = new Claim("Tenants", "[{\"id\" : \"494c1c3d-1026-4336-bd0b-23355785fab1\", \"name\": \"Dev Tenant 01\"}]");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userIdClaim, tenantClaim }));

            _facade = new OperationConfigurationsFacade(_mockContext.Object, _blobManager.Object, _queueManager.Object, _documentDbRepository.Object);
        }

        [Test]
        public async Task When_GetConfig_Succeeds()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(result.ErrorCodes, Is.Empty);

            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_GetTemplate_Succeeds()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;

            var result = await _facade.Get(tenantId, null, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(result.ErrorCodes, Is.Empty);

            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_Get_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-304");
            Assert.That(errorCode, Is.Not.Null);
            Assert.That(errorCode.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task When_Get_EmptyOperationId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = Guid.Empty;

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task When_Get_InvalidOperationId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = Guid.Parse("23F0B5B9-6F97-4150-A1F0-3E1B6EAEE29E");

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Get_EmptyTenantId_Fails()
        {
            var tenantId = Guid.Empty;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Get_InvalidTenantId_Fails()
        {
            var tenantId = Guid.Parse("5066B0D1-CAC3-4719-9D17-0C80FE0C16A5");
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Get(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Import_Succeeds()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            string authHeader = "StubbedAuthHeader";
            var fileMetaData = new FileUploadMetadataDto()
            {
                OriginalFileName = "MyFile.xlsx",
                SavedFileName = "/SomeFile/Somewhere",
                TransactionType = "OperationConfig"
            };

            var result = await _facade.Upload(fileMetaData, authHeader, tenantId);

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(result.ErrorCodes.Count, Is.EqualTo(0));

            _blobManager.Verify(x=> x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>()));
            _documentDbRepository.Verify(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()));
            _queueManager.Verify(x=> x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public async Task When_Import__UserClaimNull_Fails()
        {
            Thread.CurrentPrincipal = null;

            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            string authHeader = "StubbedAuthHeader";
            var fileMetaData = new FileUploadMetadataDto()
            {
                OriginalFileName = "MyFile.xlsx",
                SavedFileName = "/SomeFile/Somewhere",
                TransactionType = "OperationConfig"
            };

            var result = await _facade.Upload(fileMetaData, authHeader, tenantId);

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x=> x.Code == "FFERR-304"), Is.Not.Null);

            // Make sure no external storage is written to or called
            _blobManager.Verify(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),Times.Never);
            _documentDbRepository.Verify(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()),Times.Never);
            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task When_Import__UserNotInRequestedTenant_Fails()
        {
            var tenantId = new Guid("D4820370-E6E1-4FC4-B3B1-F978E21C0D67");
            string authHeader = "StubbedAuthHeader";
            var fileMetaData = new FileUploadMetadataDto()
            {
                OriginalFileName = "MyFile.xlsx",
                SavedFileName = "/SomeFile/Somewhere",
                TransactionType = "OperationConfig"
            };

            var result = await _facade.Upload(fileMetaData, authHeader, tenantId);

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209"), Is.Not.Null);

            // Make sure no external storage is written to or called
            _blobManager.Verify(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _documentDbRepository.Verify(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()), Times.Never);
            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
