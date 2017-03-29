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
using Hach.Fusion.Core.Test.EntityFramework;
using Hach.Fusion.Core.Dtos;
using Hach.Fusion.Data.Database.Interfaces;
using Hach.Fusion.Data.Entities;

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

            _mockContext.Setup(x => x.Locations).Returns(new InMemoryDbSet<Location>(LocationSeedData.GetData()));
            _mockContext.Setup(x => x.LocationTypes)
                .Returns(new InMemoryDbSet<LocationType>(LocationTypeSeedData.GetData()));
            _mockContext.Setup(x => x.LocationTypeGroups)
                .Returns(new InMemoryDbSet<LocationTypeGroup>(LocationTypeGroupSeedData.GetData()));
            _mockContext.Setup(x => x.Measurements)
                .Returns(new InMemoryDbSet<Measurement>(MeasurementSeedData.GetData()));
            _mockContext.Setup(x => x.LocationParameterLimits)
                .Returns(new InMemoryDbSet<LocationParameterLimit>(LocationParameterLimitSeedData.GetData()));
            _mockContext.Setup(x => x.LocationParameterNotes)
                .Returns(new InMemoryDbSet<LocationParameterNote>(LocationParameterNoteSeedData.GetData()));

            _mockContext.Setup(x => x.Tenants).Returns(new InMemoryDbSet<Tenant>(TenantSeedData.GetData()));
            _mockContext.Setup(x => x.Users).Returns(new InMemoryDbSet<User>(UserSeedData.GetData()));
            _mockContext.Setup(x => x.ProductOfferingTenantLocations)
                .Returns(new InMemoryDbSet<ProductOfferingTenantLocation>(ProductOffingTenantLocationSeedData.GetData()));
            _mockContext.Setup(x => x.LocationParameters)
                .Returns(new InMemoryDbSet<LocationParameter>(LocationParameterSeedData.GetData()));
            _mockContext.Setup(x => x.Measurements)
                .Returns(new InMemoryDbSet<Measurement>(MeasurementSeedData.GetData()));
            _mockContext.Setup(x => x.MeasurementTransactions)
                .Returns(new InMemoryDbSet<MeasurementTransaction>(MeasurementTransactionSeedData.GetData()));
            _mockContext.Setup(x => x.ProductOfferings)
                .Returns(new InMemoryDbSet<ProductOffering>(ProductOfferingSeedData.GetData()));

            SetupDataRelationships(_mockContext.Object);

            var userIdClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", UserSeedData.DevTenant01User.Id.ToString());
            var tenantClaim = new Claim("Tenants", "[{\"id\" : \"" + TenantSeedData.DevTenant01.Id + "\", \"name\": \" " + TenantSeedData.DevTenant01.Name + "\"}]");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userIdClaim, tenantClaim }));

            _facade = new OperationConfigurationsFacade(_mockContext.Object, _blobManager.Object, _queueManager.Object, _documentDbRepository.Object);

        }

        private static void SetupDataRelationships(DataContext ctx)
        {
            var user = ctx.Users.SingleOrDefault(x => x.Id == UserSeedData.DevTenant01User.Id);
            var tenant = ctx.Tenants.SingleOrDefault(x => x.Id == TenantSeedData.DevTenant01.Id);

            user.Tenants.Add(tenant);
            tenant.Users.Add(user);

            user = ctx.Users.SingleOrDefault(x => x.Id == UserSeedData.AnotherUser.Id);
            tenant = ctx.Tenants.SingleOrDefault(x => x.Id == TenantSeedData.AnotherTenant.Id);

            user.Tenants.Add(tenant);
            tenant.Users.Add(user);

            // Add LocationType 'Operation' To LocationTypeGroup 'Operation'
            ctx.LocationTypes.Single(x => x.Id == LocationTypeSeedData.Operation.Id).LocationTypeGroup
                = ctx.LocationTypeGroups.Single(x => x.Id == LocationTypeGroupSeedData.Operation.Id);

            // Give Location 'Operation_01' LocationType 'Operation'
            ctx.Locations.Single(x => x.Id == LocationSeedData.Operation01.Id).LocationType
                = ctx.LocationTypes.Single(x => x.Id == LocationTypeSeedData.Operation.Id);


            foreach (var locParam in ctx.LocationParameters)
            {
                var loc = ctx.Locations.Single(x => x.Id == locParam.LocationId);
                locParam.Location = loc;
            }

            foreach (var note in ctx.LocationParameterNotes)
            {
                var locParam = ctx.LocationParameters.Single(x => x.Id == note.LocationParameterId);
                note.LocationParameter = locParam;
            }
            foreach (var measurment in ctx.Measurements)
            {
                var locParam = ctx.LocationParameters.Single(x => x.Id == measurment.LocationParameterId);
                measurment.LocationParameter = locParam;
            }

            foreach (var potl in ctx.ProductOfferingTenantLocations)
            {
                potl.Tenant = ctx.Tenants.Single(x => x.Id == potl.TenantId);
                ctx.Tenants.Single(x => x.Id == potl.TenantId).ProductOfferingTenantLocations.Add(potl);
                potl.Location = ctx.Locations.Single(x => x.Id == potl.LocationId);
                ctx.Locations.Single(x => x.Id == potl.LocationId).ProductOfferingTenantLocations.Add(potl);
                potl.ProductOffering = ctx.ProductOfferings.Single(x => x.Id == potl.ProductOfferingId);
                ctx.ProductOfferings.Single(x => x.Id == potl.ProductOfferingId).ProductOfferingTenantLocations.Add(potl);
            }
            foreach (var loc in ctx.Locations)
            {
                if (loc.ParentId.HasValue)
                {
                    loc.Parent = ctx.Locations.Single(x => x.Id == loc.ParentId.Value);
                    loc.Parent.Locations.Add(loc);
                }

                loc.LocationType = ctx.LocationTypes.Single(x => x.Id == loc.LocationTypeId);
            }
            foreach (var loc in ctx.LocationTypes)
            {
                var children = ctx.LocationTypes.Where(x => x.ParentId == loc.Id).ToList();
                children.ForEach(x => loc.LocationTypes.Add(x));
            }
        }

        [Test]
        public async Task When_Download_Succeeds()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(result.ErrorCodes, Is.Empty);

            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task When_Download_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-304");
            Assert.That(errorCode, Is.Not.Null);
            Assert.That(errorCode.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Download_EmptyOperationId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = Guid.Empty;

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_InvalidOperationId_Fails()
        {
            var tenantId = _mockContext.Object.Tenants.Single(x => x.Name == "Dev Tenant 01").Id;
            var operationId = Guid.Parse("23F0B5B9-6F97-4150-A1F0-3E1B6EAEE29E");

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_EmptyTenantId_Fails()
        {
            var tenantId = Guid.Empty;
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }

        [Test]
        public async Task When_Download_InvalidTenantId_Fails()
        {
            var tenantId = Guid.Parse("5066B0D1-CAC3-4719-9D17-0C80FE0C16A5");
            var operationId = _mockContext.Object.Locations.Single(l => l.Name == "Operation_01").Id;

            var result = await _facade.Download(tenantId, operationId, "");
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            var errorCode = result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209");
            Assert.That(errorCode, Is.Not.Null);
        }


        [Test]
        public async Task When_Delete_Succeeds()
        {
            var operationId = LocationSeedData.Operation01.Id;

            var result = await _facade.Delete(operationId);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var deleted = _mockContext.Object.Locations.SingleOrDefault(x => x.Id == operationId);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task When_Delete_EmptyGuid_Fails()
        {
            var result = await _facade.Delete(Guid.Empty);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-201"), Is.Not.Null); // Property Required
        }

        [Test]
        public async Task When_Delete_NullId_Fails()
        {
            var result = await _facade.Delete(null);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-201"), Is.Not.Null); // Property Required
        }

        [Test]
        public async Task When_Delete_InvalidId_Fails()
        {
            var result = await _facade.Delete(new Guid("5747EFC2-6AFA-47D9-98A1-0F1AD1D9DC85"));
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-100"), Is.Not.Null); // Entity Could no be found
        }

        [Test]
        public async Task When_Delete_OperationCannotBeDeleted_Fails()
        {
            var result = await _facade.Delete(LocationSeedData.CannotDeleteOperation.Id);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-106"), Is.Not.Null); // Entity could not be deleted
        }

        [Test]
        public async Task When_Delete_UserNotInTenant_Fails()
        {
            var userIdClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", UserSeedData.AnotherUser.Id.ToString());
            var tenantClaim = new Claim("Tenants", "[{\"id\" : \"" + TenantSeedData.DevTenant01.Id + "\", \"name\": \" " + TenantSeedData.AnotherTenant.Name + "\"}]");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userIdClaim, tenantClaim }));


            var result = await _facade.Delete(LocationSeedData.Operation01.Id);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-209"), Is.Not.Null); // Foreign Key Does not exist
        }

        [Test]
        public async Task When_Delete_UserNotAuthenticated_Fails()
        {
            Thread.CurrentPrincipal = null;

            var result = await _facade.Delete(null);
            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-304"), Is.Not.Null); // Foreign Key Does not exist
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

            _blobManager.Verify(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _documentDbRepository.Verify(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()));
            _queueManager.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
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
            Assert.That(result.ErrorCodes.FirstOrDefault(x => x.Code == "FFERR-304"), Is.Not.Null);

            // Make sure no external storage is written to or called
            _blobManager.Verify(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _documentDbRepository.Verify(x => x.CreateItemAsync(It.IsAny<UploadTransaction>()), Times.Never);
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

    #region Seed Data

    internal static class LocationParameterNoteSeedData
    {
        public static IEnumerable<LocationParameterNote> GetData()
        {
            return new List<LocationParameterNote>()
            {
                Note1
            };
        }

        public static LocationParameterNote Note1 => new LocationParameterNote()
        {
            Id = new Guid("4D13B5FB-D883-4BC7-9A4F-07ABDDE658B2"),
            Note = "A test note",
            LocationParameterId = LocationParameterSeedData.LocationParameter.Id
        };
    }

    internal static class LocationParameterLimitSeedData
    {
        public static IEnumerable<LocationParameterLimit> GetData()
        {
            return new List<LocationParameterLimit>()
            {
                Limit1
            };
        }
        public static LocationParameterLimit Limit1 => new LocationParameterLimit()
        {
            Id = new Guid("8DCFFB4D-8B11-4D25-BD6A-0E11DA8BA5A6"),
            LocationParameterId = LocationParameterSeedData.LocationParameter.Id,
            Value = 1
        };
    }
    internal static class ProductOffingTenantLocationSeedData
    {
        public static IEnumerable<ProductOfferingTenantLocation> GetData()
        {
            return new List<ProductOfferingTenantLocation>
            {
                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.DevTenant01.Id,
                    LocationId = LocationSeedData.Operation01.Id
                },

                  new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.DevTenant01.Id,
                    LocationId = LocationSeedData.CannotDeleteProcess.Id
                },

                  new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.DevTenant01.Id,
                    LocationId = LocationSeedData.CannotDeleteOperation.Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.AnotherTenant.Id,
                    LocationId = LocationSeedData.SamplingSiteInfluentInfluentCombined.Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.AnotherTenant.Id,
                    LocationId = LocationSeedData.ProcessPreliminary.Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = ProductOfferingSeedData.Collect.Id,
                    TenantId = TenantSeedData.AnotherTenant.Id,
                    LocationId = LocationSeedData.ProcessInfluent.Id
                }
            };
        }
    }

    internal static class MeasurementTransactionSeedData
    {
        public static IEnumerable<MeasurementTransaction> GetData()
        {
            return new List<MeasurementTransaction>()
            {
                TestMeasurementTransaction
            };
        }

        public static MeasurementTransaction TestMeasurementTransaction => new MeasurementTransaction()
        {
            Id = new Guid("76138C92-A5E4-494E-892C-09EE3E575243"),
            LocationId = LocationSeedData.CannotDeleteProcess.Id
        };
    }


    internal static class MeasurementSeedData
    {
        public static IEnumerable<Measurement> GetData()
        {
            return new List<Measurement>()
            {
                TestMeasurement
            };
        }

        public static Measurement TestMeasurement => new Measurement()
        {
            Id = new Guid("EC5FA6F4-7D07-494F-A050-93E5B9382D3C"),
            LocationParameterId = LocationParameterSeedData.LocationParameter.Id
        };
    }

    internal static class LocationParameterSeedData
    {
        public static IEnumerable<LocationParameter> GetData()
        {
            return new List<LocationParameter>()
            {
                LocationParameter
            };
        }

        public static LocationParameter LocationParameter => new LocationParameter()
        {
            Id = new Guid("B7386376-1DBF-4850-9DEA-640B8D770596"),
            LocationId = LocationSeedData.CannotDeleteProcess.Id
        };
    }

    internal static class TenantSeedData
    {
        public static IEnumerable<Tenant> GetData()
        {
            return new List<Tenant>()
            {
                DevTenant01,
                AnotherTenant
            };
        }

        public static Tenant DevTenant01 => new Tenant()
        {
            Id = new Guid("464FBE6E-BED8-4086-AE54-3533DA93846F"),
            Name = "Dev Tenant 01"
        };
        public static Tenant AnotherTenant => new Tenant()
        {
            Id = new Guid("5AB4F13A-8F22-46CA-AEE9-51D8AD2F3D45"),
            Name = "Another Tenant"
        };
    }

    internal static class UserSeedData
    {
        public static IEnumerable<User> GetData()
        {
            return new List<User>()
            {
                DevTenant01User,
                AnotherUser
            };
        }
        public static User DevTenant01User => new User()
        {
            Id = new Guid("A567CB04-6EA9-4DBC-9487-54846A72B2E7")
        };

        public static User AnotherUser => new User()
        {
            Id = new Guid("BCF18955-918C-4042-991B-3BDC7A7D6ADB")
        };
    }

    internal static class LocationTypeSeedData
    {
        public static IEnumerable<LocationType> GetData()
        {
            return new List<LocationType>()
            {
                Operation,
                SamplingSite,
                Process,
                Distibution,
                FortCollinsOperation,
                FortCollinsSystemA,
                FortCollinsSystemB,
                FortCollinsCollectorA1,
                FortCollinsCollectorB2,
                FortCollinsCollectorA2,
                FortCollinsCollectorB1
            };
        }

        public static LocationType Operation => new LocationType
        {
            Id = Guid.Parse("66b8a397-96ae-4c6b-b1f1-9f28a823a8e6"),
            I18NKeyName = "Operation"
        };

        public static LocationType Process => new LocationType

        {
            Id = Guid.Parse("c40b1d39-004c-4684-a62f-18fcecd2a49c"),
            I18NKeyName = "Process"
        };

        public static LocationType SamplingSite => new LocationType

        {
            Id = Guid.Parse("E43D28F7-E0D7-439B-AF11-7A56E384621D"),
            I18NKeyName = "Sampling Site"
        };

        public static LocationType Distibution => new LocationType

        {
            Id = Guid.Parse("1ecb1b32-599d-4a7b-93b5-f2aa302d7635"),
            I18NKeyName = "Distribution"
        };

        public static LocationType FortCollinsOperation => new LocationType

        {
            Id = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC"),
            I18NKeyName = "FortCollinsOperation"
        };

        public static LocationType FortCollinsSystemA => new LocationType

        {
            Id = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE"),
            I18NKeyName = "FortCollinsSystemA",
            ParentId = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC") //FortCollinsOperation.Id
        };

        public static LocationType FortCollinsSystemB => new LocationType

        {
            Id = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90"),
            I18NKeyName = "FortCollinsSystemB",
            ParentId = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC") //FortCollinsOperation.Id
        };

        public static LocationType FortCollinsCollectorA1 => new LocationType

        {
            Id = Guid.Parse("F3FE8D0F-B87D-4F40-B234-D15A4BB8E71D"),
            I18NKeyName = "FortCollinsCollectorA1",
            ParentId = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE") // FortCollinsSystemA.Id
        };

        public static LocationType FortCollinsCollectorA2 => new LocationType

        {
            Id = Guid.Parse("88F848F9-E5D4-4F53-B8E3-BA3E4419468F"),
            I18NKeyName = "FortCollinsCollectorA2",
            ParentId = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE") // FortCollinsSystemA.Id
        };

        public static LocationType FortCollinsCollectorB1 => new LocationType

        {
            Id = Guid.Parse("7720B622-D561-4D61-9FA5-F7B9B1BB8ED7"),
            I18NKeyName = "FortCollinsCollectorB1",
            ParentId = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90") // FortCollinsSystemB.Id
        };

        public static LocationType FortCollinsCollectorB2 => new LocationType

        {
            Id = Guid.Parse("E5EB487C-D427-47EF-8651-9E56F658EA50"),
            I18NKeyName = "FortCollinsCollectorB2",
            ParentId = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90") // FortCollinsSystemB.Id
        };
    }

    internal static class LocationSeedData
    {
        public static IEnumerable<Location> GetData()
        {
            return new List<Location>()
            {
                CannotDeleteOperation,
                Operation01,
                ProcessPreliminary,
                ProcessInfluent,
                ProcessPrimaryTreatment,
                ProcessSecondaryTreatment,
                SamplingSiteGrit,
                SamplingSiteScreenings,
                SamplingSiteChemical,
                SamplingSiteInfluentInfluentCombined,
                SamplingSiteInfluentHauledWasted,
                SamplingSiteInfluentRecycled,
                CannotDeleteProcess,
                CannotDeleteCollector
            };
        }

        public static Location CannotDeleteCollector => new Location()
        {
            Id = new Guid("0B26D24D-72B6-4B1D-A1ED-5D6FE0E328F0"),
            Name = "Cannot Delete Collector",
            ParentId = CannotDeleteProcess.Id,
            LocationTypeId = LocationTypeSeedData.Distibution.Id

        };

        public static Location CannotDeleteOperation => new Location()
        {
            Id = new Guid("89B39DDC-5F87-4FF0-A1FB-3EB6A611B606"),
            Name = "Cannot Be Deleted",
            LocationTypeId = LocationTypeSeedData.Operation.Id
        };

        public static Location CannotDeleteProcess => new Location()
        {
            Id = new Guid("C7222396-08E4-4AB2-A9C9-D7AC94614BC0"),
            Name = "Process 01",
            LocationTypeId = LocationTypeSeedData.Process.Id,
            ParentId = CannotDeleteOperation.Id
        };

        public static Location Operation01 => new Location()
        {
            Id = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"),
            Name = "Operation_01",
            LocationTypeId = LocationTypeSeedData.Operation.Id,
            ParentId = null,
            SortOrder = null
        };

        public static Location ProcessPreliminary => new Location()
        {
            Id = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"),
            Name = "Process_Preliminary",
            LocationTypeId = LocationTypeSeedData.Process.Id,
            ParentId = Operation01.Id, // Operation_01.Id,
            SortOrder = 1

        };

        public static Location ProcessInfluent => new Location()
        {
            Id = Guid.Parse("e9562dff-5204-4aff-a413-d1212327dea8"),
            Name = "Process_Influent",
            LocationTypeId = LocationTypeSeedData.Process.Id,
            ParentId = Operation01.Id, // Operation_01.Id,
            SortOrder = null

        };

        public static Location ProcessPrimaryTreatment => new Location()
        {
            Id = Guid.Parse("eef791a6-01f0-4274-9bf1-98c27fbeb2b2"),
            Name = "Process_PrimaryTreatment",
            LocationTypeId = LocationTypeSeedData.Process.Id,
            ParentId = Operation01.Id, // Operation_01.Id,
            SortOrder = null

        };

        public static Location ProcessSecondaryTreatment => new Location()
        {
            Id = Guid.Parse("b317b8ab-d1b1-4c09-9b36-014fc024879f"),
            Name = "Process_SecondaryTreatment",
            LocationTypeId = LocationTypeSeedData.Process.Id,
            ParentId = Operation01.Id, // Operation_01.Id,
            SortOrder = null

        };

        public static Location SamplingSiteGrit => new Location()
        {
            Id = Guid.Parse("6dfdc82f-ff38-468c-84cc-1855200cfbfe"),
            Name = "SamplingSite_Grit",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessPreliminary.Id, // Process_Preliminary.Id,
            SortOrder = 2
        };

        public static Location SamplingSiteScreenings => new Location()
        {
            Id = Guid.Parse("49935b82-f8df-4251-9144-d499f5ad5c79"),
            Name = "SamplingSite_Screenings",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessPreliminary.Id, // Process_Preliminary.Id
            SortOrder = null
        };

        public static Location SamplingSiteChemical => new Location()
        {
            Id = Guid.Parse("cd6bd0f3-ac99-4a8e-94cc-324aebeea696"),
            Name = "SamplingSite_Chemical",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessPreliminary.Id, // Process_Preliminary.Id
            SortOrder = null
        };


        public static Location SamplingSiteInfluentInfluentCombined => new Location()
        {
            Id = Guid.Parse("dcd6c19f-ee8b-4a08-ae1c-02214570c8f4"),
            Name = "SamplingSite_Influent_InfluentCombined",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessInfluent.Id,// Process_Influent.Id,
            SortOrder = null
        };


        public static Location SamplingSiteInfluentHauledWasted => new Location()
        {
            Id = Guid.Parse("1d54b1a3-803b-46be-a870-772fb8af4e06"),
            Name = "SamplingSite_Influent_HauledWasted",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessInfluent.Id,// Process_Influent.Id
            SortOrder = null
        };

        public static Location SamplingSiteInfluentRecycled => new Location()
        {
            Id = Guid.Parse("d1106178-2f92-447f-8853-d41c707e4cf4"),
            Name = "SamplingSite_Influent_Recycled",
            LocationTypeId = LocationTypeSeedData.SamplingSite.Id,
            ParentId = ProcessInfluent.Id,// Process_Influent.Id,
            SortOrder = null
        };
    }

    internal static class ProductOfferingSeedData
    {
        public static IEnumerable<ProductOffering> GetData()
        {
            return new List<ProductOffering>()
            {
                Collect
            };
        }

        public static ProductOffering Collect => new ProductOffering()
        {
            Id = Guid.Parse("e36a4d05-a3b4-e511-82c1-000c29b494b6"),
            Name = "Collect"
        };
    }


    internal static class LocationTypeGroupSeedData
    {
        public static IEnumerable<LocationTypeGroup> GetData()
        {
            return new List<LocationTypeGroup>()
            {
                Operation
            };
        }

        public static LocationTypeGroup Operation => new LocationTypeGroup()
        {
            Id = Data.Constants.LocationTypeGroups.Operation.Id,
            I18NKeyName = Data.Constants.LocationTypeGroups.Operation.I18NKeyName
        };
    }

    #endregion

}
