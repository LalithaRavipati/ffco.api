using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.FFCO.Business.Facades;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hach.Fusion.Core.Test.EntityFramework;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Database.Interfaces;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class FileFacadeTests
    {
        private Mock<DataContext> _mockDataContext;
        private Mock<IDocumentDbRepository<UploadTransaction>> _mockDocumentDb;
        private Mock<IBlobManager> _mockBlobManager;

        private FileFacade _facade;

        // For success
        private readonly Guid _documentId = new Guid("EF8344F9-E5FA-4256-AD78-B6DC763E5D86");
        private Guid _userId = new Guid("85c04bda-4416-44bf-8654-868ba9f5dd3a");
        private readonly Guid _tenantForUser = new Guid("840D9FC6-8B6D-401E-8A4C-BC4B8560E6E1");
        private readonly string _blobStorageBlobName = "BodyPart_0738841b-b676-4abc-9f4f-0cbedf006abb";
        private readonly string _blobContent = "ABCDE";
        private readonly string _blobOriginalFilename = "TestFileName.xlsx";

        // For failure (metadata but no blob data)
        private readonly Guid _documentNoExistBlobGuidId = new Guid("BA5B3AD9-DC2D-4DF2-A70C-29863A3BA55A");
        private readonly string _blobStorageNoExistBlobName = "NoExist";

        // For failure (user is not a member of the blob's tenant)
        private Guid _userNotInBlobTenantId = new Guid("299335EA-2216-4BB8-939B-FD85022403E4");
        private Guid _tenantNotInBlobTenantId = new Guid("3E76CE09-D31F-4CD3-A66B-1EB19F31EFD7");

        [SetUp]
        public void Setup()
        {
            _mockDataContext = new Mock<DataContext>();
            _mockDocumentDb = new Mock<IDocumentDbRepository<UploadTransaction>>();
            _mockBlobManager = new Mock<IBlobManager>();

            _facade = new FileFacade(_mockDataContext.Object, _mockDocumentDb.Object, _mockBlobManager.Object);

            InitializeMockDataContext(_mockDataContext);

            InitializeMockDocumentDb(_mockDocumentDb);
            InitializeMockBlobManager(_mockBlobManager);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                _userId.ToString());

            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
        }

        public void InitializeMockDataContext(Mock<DataContext> mockDataContext)
        {
            mockDataContext.Setup(x => x.Users).Returns(new InMemoryDbSet<User>(GetUsers()));
        }

        public List<User> GetUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = _userId,
                    UserName = "adhach",
                    CreatedById = _userId,
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = _userId,
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    Tenants = new List<Tenant>
                        { new Tenant {Id = _tenantForUser } }
                },

                new User
                {
                    Id = _userNotInBlobTenantId,
                    UserName = "notInBlobTenant",
                    CreatedById = _userId,
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = _userId,
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    Tenants = new List<Tenant>
                        { new Tenant {Id = _tenantNotInBlobTenantId } }
                },
            };
        }

        public void InitializeMockDocumentDb(Mock<IDocumentDbRepository<UploadTransaction>> mockDataContext)
        {
            mockDataContext.Setup(x => 
                x.GetItemAsync(It.Is<string>(g => g == _documentId.ToString())))
                .ReturnsAsync(new UploadTransaction
                {
                   Id =  new Guid("5b86471d-dd00-2402-202d-f684f5cafbdb"),
                   OriginalFileName = _blobOriginalFilename,
                   TenantIds = new List<Guid> { _tenantForUser },
                   UserId = _userId,
                   UtcTimestamp = new DateTime(2017, 03, 17),
                   BlobStorageContainerName = "blobdata",
                   BlobStorageBlobName = _blobStorageBlobName
                });

            mockDataContext.Setup(x =>
                x.GetItemAsync(It.Is<string>(g => g == _documentNoExistBlobGuidId.ToString())))
                .ReturnsAsync(new UploadTransaction
                {
                    Id = new Guid("1B84EA6A-C990-49F2-9BF0-10B11174EF79"),
                    OriginalFileName = _blobOriginalFilename,
                    TenantIds = new List<Guid> { _tenantForUser },
                    UserId = _userId,
                    UtcTimestamp = new DateTime(2017, 03, 17),
                    BlobStorageContainerName = "blobdata",
                    BlobStorageBlobName = _blobStorageNoExistBlobName
                });

        }

        public void InitializeMockBlobManager(Mock<IBlobManager> mockBlobManager)
        {
            mockBlobManager.Setup(x =>
                x.DownloadBlobAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(),
                                    It.Is<string>(b => b == _blobStorageBlobName)))
                .ReturnsAsync((MemoryStream ms, string blobStorageConnectionString, string blobStorageContainerName,
                               string blobStorageBlobName) => new BlobDownloadResult
                {
                    BlobName = blobStorageBlobName
                })
                .Callback((MemoryStream ms, string blobStorageConnectionString, string blobStorageContainerName,
                           string blobStorageBlobName) => ms.Write(Encoding.ASCII.GetBytes(_blobContent), 0, 5 ));
        }

        [Test]
        public async Task When_Get_Succeeds()
        {
            var result = await _facade.Get(_documentId);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo(_blobContent));

            Assert.That(result.Content.Headers.ContentDisposition.FileName, Is.EqualTo(_blobOriginalFilename));
            Assert.That(result.Content.Headers.ContentDisposition.Size, Is.EqualTo(_blobContent.Length));
            Assert.That(result.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
        }

        [Test]
        public async Task When_Get_Fails_NoPrincipal()
        {
            Thread.CurrentPrincipal = null;

            var result = await _facade.Get(_documentId);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content.Contains("FFERR-304"));
        }

        [Test]
        public async Task When_Get_Fails_NoUserClaim()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var result = await _facade.Get(_documentId);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content.Contains("FFERR-304"));
        }

        [Test]
        public async Task When_Get_Fails_NoMetadata()
        {
            var result = await _facade.Get(Guid.NewGuid());

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content.Contains("FFERR-100"));
        }

        [Test]
        public async Task When_Get_Fails_NoBlob()
        {
            var result = await _facade.Get(_documentNoExistBlobGuidId);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content.Contains("FFERR-100"));
        }

        [Test]
        public async Task When_Get_Fails_UserNotInBlobTenant()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                _userNotInBlobTenantId.ToString());

            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var result = await _facade.Get(_documentId);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var content = await result.Content.ReadAsStringAsync();
            Assert.That(content.Contains("FFERR-100"));
        }
    }
}