using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
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
    public class LocationLogEntryFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private readonly Mock<ODataQueryOptions<LocationLogEntryQueryDto>> _mockDtoOptions;
        private LocationLogEntryFacade _facade;
        private Guid _userId;

        public LocationLogEntryFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<LocationLogEntryQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof (LocationLogEntryQueryDto), new ODataPath()),
                new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LocationLogEntryBaseDto>("LocationLogEntries");
            builder.EntityType<LocationLogEntryBaseDto>()
                .HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {

            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);
            var validator = new LocationLogEntryValidator();
            _facade = new LocationLogEntryFacade(_mockContext.Object, validator);

            _userId = _mockContext.Object.Users.Single(x=> x.UserName =="tnt01user").Id;

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                , _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
        }


        #region Get Tests

        [Test]
        public async Task When_GetAll_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.EqualTo(3));
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationLogEntries.Single(l=> l.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationLogEntries.Single(l=> l.Id == new Guid("C7267A62-A2F1-4C2A-8F36-2BEABD9B0F66")).Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationLogEntries.Single(l=> l.Id == new Guid("83FFABC0-D51B-4B82-8DCC-80D1F258DB2F")).Id), Is.True);
        }

        [Test]
        public async Task When_GetAll_BadToken()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var queryResult = await _facade.Get(_mockDtoOptions.Object);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        }

        [Test]
        public async Task When_GetOne_Succeeds()
        {
            var queryResult = await _facade.Get(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            Assert.That(queryResult.Dto, Is.Not.Null);
            Assert.That(queryResult.Dto.Id, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x => x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id));
            Assert.That(queryResult.Dto.LocationId, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId));
            Assert.That(queryResult.Dto.LogEntry, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LogEntry));
            Assert.That(queryResult.Dto.CreatedById, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).CreatedById));
            Assert.That(queryResult.Dto.ModifiedById, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).ModifiedById));
        }

        [Test]
        public async Task When_GetOne_NoExist()
        {
            var queryResult = await _facade.Get(Guid.NewGuid());

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        }

        [Test]
        public async Task When_GetOne_WrongTenant()
        {
            var queryResult = await _facade.Get(_mockContext.Object.LocationLogEntries.Single(l=> l.Id == new Guid("EA7E094D-B5F7-4E59-B642-7FDC08DF58FC")).Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        }

        [Test]
        public async Task When_GetOne_BadToken()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var queryResult = await _facade.Get(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        }

        #endregion Get Tests

        #region Create Tests

        [Test]
        public async Task When_Create_Succeeds()
        {
            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                LocationId = _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId,
                LogEntry = "Create Log Entry",
                TimeStamp = DateTimeOffset.UtcNow
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.Entity.LocationId, Is.EqualTo(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId));
            Assert.That(commandResult.Entity.LogEntry, Is.EqualTo("Create Log Entry"));
            Assert.That(commandResult.Entity.CreatedById, Is.EqualTo(_userId));
            Assert.That((DateTime.Now - commandResult.Entity.CreatedOn).Seconds, Is.LessThanOrEqualTo(10));
            Assert.That(commandResult.Entity.ModifiedById, Is.EqualTo(_userId));
            Assert.That((DateTime.Now - commandResult.Entity.ModifiedOn).Seconds, Is.LessThanOrEqualTo(10));
        }

        [Test]
        public async Task When_Create_BadLocation()
        {
            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                LocationId = Guid.NewGuid(),
                LogEntry = "Create Log Entry",
                TimeStamp = DateTimeOffset.UtcNow
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-209"));
        }

        [Test]
        public async Task When_Create_BadId()
        {
            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                Id = Guid.NewGuid(),
                LocationId = _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId,
                LogEntry = "Create Log Entry",
                TimeStamp = DateTimeOffset.UtcNow
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-211"));
        }

        [Test]
        public async Task When_Create_BadLogEntry()
        {
            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                LocationId = _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId,
                LogEntry = null,
                TimeStamp = DateTimeOffset.UtcNow
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        }

        [Test]
        public async Task When_Create_BadToken()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                LocationId = _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id,
                LogEntry = "Create Log Entry"
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        }

        [Test]
        public async Task When_Create_MissingTimestamp()
        {
            var locationLogEntryDto = new LocationLogEntryQueryDto()
            {
                LocationId = _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).LocationId,
                LogEntry = "Create Log Entry"
            };

            var commandResult = await _facade.Create(locationLogEntryDto);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task When_Delete_Succeeds()
        {
            var deleteResult = await _facade.Delete(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id);

            Assert.That(deleteResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = _mockContext.Object.LocationLogEntries.SingleOrDefault(x=> x.Id ==  new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187"));

            Assert.That(queryResult, Is.Null);
        }

        [Test]
        public async Task When_Delete_BadToken()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var deleteResult = await _facade.Delete(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id);

            Assert.That(deleteResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(deleteResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(deleteResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));

        }

        #endregion

        #region Update Tests

        [Test]
        public async Task When_Update_Succeeds()
        {
            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(_mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.LogEntry, Is.EqualTo("Update Log Entry"));
            Assert.That(queryResult.Dto.ModifiedById, Is.EqualTo(_userId));
            Assert.That((DateTime.Now - queryResult.Dto.ModifiedOn).Seconds, Is.LessThanOrEqualTo(10));
        }

        [Test]
        public async Task When_Update_NotFound()
        {
            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

            var commandResult = await _facade.Update(
                Guid.NewGuid(), delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        }

        [Test]
        public async Task When_Update_BadDelta()
        {
            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));

            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-102"));
        }

        [Test]
        public async Task When_Update_CantUpdateId()
        {
            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("Id", Guid.NewGuid());

            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-203"));
        }

        [Test]
        public async Task When_Update_BadLocationId()
        {
            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("LocationId", Guid.NewGuid());

            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-209"));
        }

        [Test]
        public async Task When_Update_NullTimestamp()
        {
            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("TimeStamp", null);

            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        }

        [Test]
        public async Task When_Update_BadToken()
        {
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            var delta = new Delta<LocationLogEntryBaseDto>(typeof(LocationLogEntryBaseDto));
            delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

            var commandResult = await _facade.Update(
                _mockContext.Object.LocationLogEntries.Single(x=> x.Id == new Guid("44E0C497-3A2C-4A89-99BD-F7B14C1A9187")).Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        }

        #endregion
    }
}