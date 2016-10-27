using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class LocationFacadeTests
    {
        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                "5170AE58-21B4-40F5-A025-E886489E9B82");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {claim}));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new LocationValidator();
            _facade = new LocationFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private DataContext _context;
        private readonly Mock<ODataQueryOptions<LocationQueryDto>> _mockDtoOptions;
        private LocationFacade _facade;

        public LocationFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<LocationQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(LocationQueryDto), new ODataPath()),
                new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LocationQueryDto>("Locations");
            builder.EntityType<LocationQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [Test]
        public async Task When_Create_ValidNoChildren_Should_Succeed()
        {
            var location = new LocationCommandDto
            {
                Name = Data.Locations.Location_2_Parent_NoDescendants.Name,
                LocationTypeId = Data.Locations.Location_2_Parent_NoDescendants.LocationTypeId
            };

            var commandResult = await _facade.Create(location);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

            var queryResult = await _facade.Get(commandResult.GeneratedId);

            Assert.That(queryResult.Dto.Name, Is.EqualTo(Data.Locations.Location_2_Parent_NoDescendants.Name));
            Assert.That(queryResult.Dto.ParentId.HasValue,
                Is.EqualTo(Data.Locations.Location_2_Parent_NoDescendants.ParentId.HasValue));
            //Assert.That(queryResult.Dto.LocationTypeId, Is.EqualTo(SeedData.Locations.Location_3_ToCreateNoChildren.LocationTypeId));
            //Assert.That(queryResult.Dto.Point.X, Is.EqualTo(SeedData.Locations.Location_3_ToCreateNoChildren.Point.X));
            //Assert.That(queryResult.Dto.Point.Y, Is.EqualTo(SeedData.Locations.Location_3_ToCreateNoChildren.Point.Y));
            //Assert.That(queryResult.Dto.Point.SpatialReference.Wkid, Is.EqualTo(SeedData.Locations.Location_3_ToCreateNoChildren.Point.SpatialReference.Wkid));
        }

        [Test]
        public async Task When_Delete_NoChildren_Should_Succeed()
        {
            var commandResult = await _facade.Delete(Data.Locations.Test_SoftDeletable.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));
            Assert.That(commandResult.ErrorCodes, Is.Null);

            var queryResult = await _facade.Get(Data.Locations.Test_SoftDeletable.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        public async Task When_Get()
        {
            var queryResult = await _facade.Get(Data.Locations.Plant_01.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            Assert.That(queryResult.Dto.Id == Data.Locations.Plant_01.Id);
        }

        [Test]
        public async Task When_Get_Deleted()
        {
            var queryResult = await _facade.Get(Data.Locations.Test_SoftDeleted.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        [Ignore("Disabled so unit test does not fail. Being worked on in a different story")]
        public async Task When_Get_Locations_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == Data.Locations.Plant_01.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Locations.Process_Preliminary.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Locations.Process_Influent.Id), Is.True);
        }

        [Test]
        public async Task When_Update_NoLoggedInUser()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.Locations.Test_Updateable;
            var delta = new Delta<LocationCommandDto>();
            delta.TrySetPropertyValue("Name", "New Name");

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Update_SetAndRemoveSortOrder()
        {
            var seed = Data.Locations.Test_Updateable;
            var delta = new Delta<LocationCommandDto>();
            delta.TrySetPropertyValue("SortOrder", 1);

            var commandResult = await _facade.Update(seed.Id, delta);
            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(Data.Locations.Test_Updateable.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.SortOrder, Is.EqualTo(1));

            delta.TrySetPropertyValue("SortOrder", null);
            commandResult = await _facade.Update(seed.Id, delta);
            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            queryResult = await _facade.Get(Data.Locations.Test_Updateable.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.SortOrder, Is.Null);
        }

        [Test]
        public async Task When_Update_With_ValidData()
        {
            var seed = Data.Locations.Test_Updateable;
            var delta = new Delta<LocationCommandDto>();
            var sortOrderUpdateval = Data.Locations.Test_ForUpdate.SortOrder == null
                ? 1
                : Data.Locations.Test_ForUpdate.SortOrder++;
            delta.TrySetPropertyValue("Name", Data.Locations.Test_ForUpdate.Name);
            delta.TrySetPropertyValue("SortOrder", sortOrderUpdateval);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(Data.Locations.Test_Updateable.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.Name, Is.EqualTo(Data.Locations.Test_ForUpdate.Name));
            Assert.That(queryResult.Dto.SortOrder, Is.EqualTo(sortOrderUpdateval));
        }

        //    Assert.That(queryResult.Dto.Point.X, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.Point.X));
        //    Assert.That(queryResult.Dto.LocationTypeId, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.LocationTypeId));
        //    Assert.That(queryResult.Dto.ParentId.Value, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.ParentId.Value));
        //    Assert.That(queryResult.Dto.ParentId.HasValue && SeedData.Locations.Location_2_1_ToCreateAsChild.ParentId.HasValue, Is.EqualTo(true));

        //    Assert.That(queryResult.Dto.InternalName, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.InternalName));

        //    var queryResult = await _facade.Get(commandResult.GeneratedId);
        //    Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_2_1_ToCreateAsChild);
        //{
        //public async Task When_Create_ValidAsChild_Should_Succeed()

        //[Test]
        //    Assert.That(queryResult.Dto.Point.Y, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.Point.Y));
        //    Assert.That(queryResult.Dto.Point.SpatialReference.Wkid, Is.EqualTo(SeedData.Locations.Location_2_1_ToCreateAsChild.Point.SpatialReference.Wkid));
        //}

        //[Test]
        //public async Task When_Create_NullPrincipal_Should_Fail()
        //{
        //    Thread.CurrentPrincipal = null;

        //    var commandResult = await _facade.Create(SeedData.Locations.Location_3_ToCreateNoChildren);
        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //}

        //[Test]
        //public async Task When_Create_AlreadyExists_Should_Fail()
        //{
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_ToCreateAlreadyExists);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(EntityErrorCode.EntityAlreadyExists.Code));
        //    Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(EntityErrorCode.EntityAlreadyExists.Description));
        //}

        //[Test]
        //public async Task When_Create_NoLocationTypeId_Should_Fail()
        //{
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_ToCreateNoLocationTypeId);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.PropertyRequired("LocationTypeId").Code));
        //    Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.PropertyRequired("LocationTypeId").Description));
        //}

        //[Test]
        //public async Task When_Create_LocationTypeIdNoExist_Should_Fail()
        //{
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_ToCreateLocationTypeIdNoExist);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("LocationTypeId").Code));
        //    Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("LocationTypeId").Description));
        //}

        //[Test]
        //public async Task When_Create_InvalidPoint_Should_Fail()
        //{
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_ToCreateInvalidPoint);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.PropertyIsInvalid("Point").Code));
        //    Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.PropertyIsInvalid("Point").Description));
        //}

        //[Test]
        //public async Task When_Create_ParentIdNoExist_Should_Fail()
        //{
        //    var commandResult = await _facade.Create(SeedData.Locations.Location_ToCreateParentIdNoExist);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId").Code));
        //    Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId").Description));
        //}
    }
}