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
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;


using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class LocationLogEntryFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<LocationLogEntryQueryDto>> _mockDtoOptions;
        private LocationLogEntryFacade _facade;
        //private Guid _userId = Data.Users.tnt01user.Id;

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
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {claim}));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new LocationLogEntryValidator();
            _facade = new LocationLogEntryFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        //#region Get Tests

        //[Test]
        //public async Task When_GetAll_Succeeds()
        //{
        //    var queryResult = await _facade.Get(_mockDtoOptions.Object);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

        //    var results = queryResult.Results;

        //    Assert.That(results.Count(), Is.EqualTo(3));
        //    Assert.That(results.Any(x => x.Id == Data.LocationLogEntries.Plant1Log1.Id), Is.True);
        //    Assert.That(results.Any(x => x.Id == Data.LocationLogEntries.Plant1Log2.Id), Is.True);
        //    Assert.That(results.Any(x => x.Id == Data.LocationLogEntries.Plant2Log1.Id), Is.True);
        //}

        //[Test]
        //public async Task When_GetAll_BadToken()
        //{
        //    Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

        //    var queryResult = await _facade.Get(_mockDtoOptions.Object);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //    Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        //}

        //[Test]
        //public async Task When_GetOne_Succeeds()
        //{
        //    var queryResult = await _facade.Get(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

        //    Assert.That(queryResult.Dto, Is.Not.Null);
        //    Assert.That(queryResult.Dto.Id, Is.EqualTo(Data.LocationLogEntries.Plant1Log1.Id));
        //    Assert.That(queryResult.Dto.LocationId, Is.EqualTo(Data.LocationLogEntries.Plant1Log1.LocationId));
        //    Assert.That(queryResult.Dto.LogEntry, Is.EqualTo(Data.LocationLogEntries.Plant1Log1.LogEntry));
        //    Assert.That(queryResult.Dto.CreatedById, Is.EqualTo(Data.LocationLogEntries.Plant1Log1.CreatedById));
        //    Assert.That(queryResult.Dto.ModifiedById, Is.EqualTo(Data.LocationLogEntries.Plant1Log1.ModifiedById));
        //}

        //[Test]
        //public async Task When_GetOne_NoExist()
        //{
        //    var queryResult = await _facade.Get(Guid.NewGuid());

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        //    Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        //}

        //[Test]
        //public async Task When_GetOne_WrongTenant()
        //{
        //    var queryResult = await _facade.Get(Data.LocationLogEntries.Plant3Log1.Id);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        //    Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        //}

        //[Test]
        //public async Task When_GetOne_BadToken()
        //{
        //    Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

        //    var queryResult = await _facade.Get(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //    Assert.That(queryResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(queryResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        //}

        //#endregion Get Tests

        //#region Create Tests

        //[Test]
        //public async Task When_Create_Succeeds()
        //{
        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        LocationId = Data.Locations.Plant_01.Id,
        //        LogEntry = "Create Log Entry",
        //        TimeStamp = DateTimeOffset.UtcNow
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
        //    Assert.That(commandResult.Entity.LocationId, Is.EqualTo(Data.Locations.Plant_01.Id));
        //    Assert.That(commandResult.Entity.LogEntry, Is.EqualTo("Create Log Entry"));
        //    Assert.That(commandResult.Entity.CreatedById, Is.EqualTo(_userId));
        //    Assert.That((DateTime.Now - commandResult.Entity.CreatedOn).Seconds, Is.LessThanOrEqualTo(10));
        //    Assert.That(commandResult.Entity.ModifiedById, Is.EqualTo(_userId));
        //    Assert.That((DateTime.Now - commandResult.Entity.ModifiedOn).Seconds, Is.LessThanOrEqualTo(10));
        //}

        //[Test]
        //public async Task When_Create_BadLocation()
        //{
        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        LocationId = Guid.NewGuid(),
        //        LogEntry = "Create Log Entry",
        //        TimeStamp = DateTimeOffset.UtcNow
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-209"));
        //}

        //[Test]
        //public async Task When_Create_BadId()
        //{
        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        LocationId = Data.Locations.Plant_01.Id,
        //        LogEntry = "Create Log Entry",
        //        TimeStamp = DateTimeOffset.UtcNow
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-211"));
        //}

        //[Test]
        //public async Task When_Create_BadLogEntry()
        //{
        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        LocationId = Data.Locations.Plant_01.Id,
        //        LogEntry = null,
        //        TimeStamp = DateTimeOffset.UtcNow
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        //}

        //[Test]
        //public async Task When_Create_BadToken()
        //{
        //    Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        LocationId = Data.Locations.Plant_01.Id,
        //        LogEntry = "Create Log Entry"
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        //}

        //[Test]
        //public async Task When_Create_MissingTimestamp()
        //{
        //    var locationLogEntryDto = new LocationLogEntryCommandDto()
        //    {
        //        LocationId = Data.Locations.Plant_01.Id,
        //        LogEntry = "Create Log Entry"
        //    };

        //    var commandResult = await _facade.Create(locationLogEntryDto);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        //}

        //#endregion

        //#region Delete Tests

        //[Test]
        //public async Task When_Delete_Succeeds()
        //{
        //    var deleteResult = await _facade.Delete(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(deleteResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

        //    var queryResult = await _facade.Get(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        //}

        //[Test]
        //public async Task When_Delete_BadToken()
        //{
        //    Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

        //    var deleteResult = await _facade.Delete(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(deleteResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //    Assert.That(deleteResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(deleteResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));

        //}

        //#endregion

        //#region Update Tests

        //[Test]
        //public async Task When_Update_Succeeds()
        //{
        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

        //    var queryResult = await _facade.Get(Data.LocationLogEntries.Plant1Log1.Id);

        //    Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
        //    Assert.That(queryResult.Dto.LogEntry, Is.EqualTo("Update Log Entry"));
        //    Assert.That(queryResult.Dto.ModifiedById, Is.EqualTo(_userId));
        //    Assert.That((DateTime.Now - queryResult.Dto.ModifiedOn).Seconds, Is.LessThanOrEqualTo(10));
        //}

        //[Test]
        //public async Task When_Update_NotFound()
        //{
        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

        //    var commandResult = await _facade.Update(
        //        Guid.NewGuid(), delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-100"));
        //}

        //[Test]
        //public async Task When_Update_BadDelta()
        //{
        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, null);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));

        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-102"));
        //}

        //[Test]
        //public async Task When_Update_CantUpdateId()
        //{
        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("Id", Guid.NewGuid());

        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-203"));
        //}

        //[Test]
        //public async Task When_Update_BadLocationId()
        //{
        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("LocationId", Guid.NewGuid());

        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-209"));
        //}

        //[Test]
        //public async Task When_Update_NullTimestamp()
        //{
        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("TimeStamp", null);

        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-201"));
        //}

        //[Test]
        //public async Task When_Update_BadToken()
        //{
        //    Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

        //    var delta = new Delta<LocationLogEntryCommandDto>(typeof(LocationLogEntryCommandDto));
        //    delta.TrySetPropertyValue("LogEntry", "Update Log Entry");

        //    var commandResult = await _facade.Update(
        //        Data.LocationLogEntries.Plant1Log1.Id, delta);

        //    Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        //    Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
        //    Assert.That(commandResult.ErrorCodes[0].Code, Is.EqualTo("FFERR-304"));
        //}

        //#endregion
    }
}