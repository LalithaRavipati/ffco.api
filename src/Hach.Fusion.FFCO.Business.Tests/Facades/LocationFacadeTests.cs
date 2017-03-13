using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Routing;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class LocationFacadeTests
    {
        private Location _forUpdate;

        [SetUp]
        public void Setup()
        {

            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                ,_mockContext.Object.Users.Single(x => x.UserName == "tnt01and02user").Id.ToString());

            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var validator = new LocationValidator();
            _facade = new LocationFacade(_mockContext.Object, validator);

            _forUpdate = new Location
                {
                    Id = Guid.Parse("fb990272-34cf-4a55-bc39-16d899bacaca"),
                    Name = "Location For Update",
                    LocationTypeId = _mockContext.Object.LocationTypes.Single(t => t.I18NKeyName == "Plant").Id,
                    //Geography = DbGeography.PointFromText("POINT(-121.11 44.11)", 4326),
                    SortOrder = null
                };
        }

        private Mock<DataContext> _mockContext;
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
        public async Task When_Create_WithParent_Should_Succeed()
        {
            var dto = new LocationBaseDto
            {
                Name = "New Location",
                LocationTypeId = _mockContext.Object.LocationTypes.Single(x=> x.I18NKeyName =="Sampling Site").Id,
                ParentId = _mockContext.Object.Locations.Single(x=> x.Name =="SamplingSite_Chemical").Id
            };

            var commandResult = await _facade.Create(dto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

            var resultDto = _mockContext.Object.Locations.Single(x=> x.Id==commandResult.GeneratedId);

            Assert.That(resultDto.Name, Is.EqualTo(dto.Name));
            Assert.That(resultDto.ParentId.HasValue, Is.EqualTo(dto.ParentId.HasValue));
        }

        [Test]
        public async Task When_Delete_NoChildren_Should_Succeed()
        {
            var updateableId = _mockContext.Object.Locations.Single(x => x.Name == "Location Updateable").Id;
            var commandResult = await _facade.Delete(updateableId);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));
            Assert.That(commandResult.ErrorCodes, Is.Null);

            var queryResult = _mockContext.Object.Locations.SingleOrDefault(x=> x.Id == updateableId);

            Assert.That(queryResult, Is.Null);
        }

        [Test]
        public async Task When_Get_Location_Succeeds()
        {
            var plant01Id = _mockContext.Object.Locations.Single(x => x.Name == "Plant_01").Id;
            var queryResult = await _facade.Get(plant01Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            Assert.That(queryResult.Dto.Id == plant01Id);
        }

        [Test]
        public async Task When_Get_Locations_Succeeds()
        {
            var plant01Id = _mockContext.Object.Locations.Single(x => x.Name == "Plant_01").Id;
            var plant02Id = _mockContext.Object.Locations.Single(x => x.Name == "Plant_02").Id;
            var plant03Id = _mockContext.Object.Locations.Single(x => x.Name == "Plant_03").Id;
            var updateableId = _mockContext.Object.Locations.Single(x => x.Name == "Location Updateable").Id;

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == plant01Id), Is.True);
            Assert.That(results.Any(x => x.Id == plant01Id), Is.True);
            Assert.That(results.Any(x => x.Id == plant01Id), Is.True);
            Assert.That(results.Any(x => x.Id == updateableId), Is.True);
        }

        [Test]
        public async Task When_Update_NoLoggedInUser()
        {
            Thread.CurrentPrincipal = null;


            var seed = _mockContext.Object.Locations.Single(x => x.Name == "Location Updateable");
            var delta = new Delta<LocationBaseDto>();
            delta.TrySetPropertyValue("Name", "New Name");

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Update_SetAndRemoveSortOrder()
        {
            var seed = _mockContext.Object.Locations.Single(x => x.Name == "Location Updateable");
            var delta = new Delta<LocationBaseDto>();
            delta.TrySetPropertyValue("SortOrder", 1);

            var commandResult = await _facade.Update(seed.Id, delta);
            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(seed.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.SortOrder, Is.EqualTo(1));

            delta.TrySetPropertyValue("SortOrder", null);
            commandResult = await _facade.Update(seed.Id, delta);
            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            queryResult = await _facade.Get(seed.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.SortOrder, Is.Null);
        }

        [Test]
        public async Task When_Update_With_ValidData()
        {
            var seed = _mockContext.Object.Locations.Single(x => x.Name == "Location Updateable");

            var delta = new Delta<LocationBaseDto>();
            var sortOrderUpdateval = _forUpdate.SortOrder == null
                ? 1
                : _forUpdate.SortOrder++;
            delta.TrySetPropertyValue("Name", _forUpdate.Name);
            delta.TrySetPropertyValue("SortOrder", sortOrderUpdateval);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var resultDto = _mockContext.Object.Locations.Single(x => x.Id ==seed.Id);

            Assert.That(resultDto.Name, Is.EqualTo(_forUpdate.Name));
            Assert.That(resultDto.SortOrder, Is.EqualTo(sortOrderUpdateval));
        }

        [Test]
        public async Task When_Create_NullPrincipal_Should_Fail()
        {
            Thread.CurrentPrincipal = null;

            var dto = new LocationBaseDto
            {
                Name = "New Location",
                LocationTypeId = _mockContext.Object.LocationTypes.Single(x => x.I18NKeyName == "Distribution").Id
            };

            var commandResult = await _facade.Create(dto);
            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Create_AlreadyExists_Should_Fail()
        {
            var plant01 = _mockContext.Object.Locations.Single(x => x.Name == "Plant_01");
            var dto = new LocationBaseDto
            {
                Name = plant01.Name,
                LocationTypeId = plant01.LocationTypeId
            };

            var commandResult = await _facade.Create(dto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(EntityErrorCode.EntityAlreadyExists.Code));
            Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(EntityErrorCode.EntityAlreadyExists.Description));
        }

        [Test]
        public async Task When_Create_NoLocationTypeId_Should_Fail()
        {
            var dto = new LocationBaseDto
            {
                Name = "New Location"
            };

            var commandResult = await _facade.Create(dto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.PropertyRequired("LocationTypeId").Code));
            Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.PropertyRequired("LocationTypeId").Description));
        }

        [Test]
        public async Task When_Create_LocationTypeIdNoExist_Should_Fail()
        {
            var dto = new LocationBaseDto
            {
                Name = "New Location",
                LocationTypeId = Guid.NewGuid()
            };

            var commandResult = await _facade.Create(dto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("LocationTypeId").Code));
            Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("LocationTypeId").Description));
        }

        [Test]
        public async Task When_Create_ParentIdNoExist_Should_Fail()
        {
            var distributionType = _mockContext.Object.LocationTypes.Single(x => x.I18NKeyName == "Distribution");
            var dto = new LocationBaseDto
            {
                Name = "New Location",
                LocationTypeId = distributionType.Id,
                ParentId = Guid.NewGuid()
            };

            var commandResult = await _facade.Create(dto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.GeneratedId, Is.EqualTo(Guid.Empty));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId").Code));
            Assert.That(commandResult.ErrorCodes[0].Description, Is.EqualTo(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId").Description));
        }
    }
}