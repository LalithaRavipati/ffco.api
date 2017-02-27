using AutoMapper;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Moq;
using NUnit.Framework;
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

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class LimitTypeFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<LimitTypeQueryDto>> _mockDtoOptions;
        private LimitTypeFacade _facade;
        private readonly IMapper _mapper;
        private readonly Guid _userId = Data.Users.tnt01user.Id;

        public LimitTypeFacadeTests()
        {
            MappingManager.Initialize();
            _mapper = MappingManager.AutoMapper;

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<LimitTypeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(LimitTypeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LimitTypeQueryDto>("LimitTypes");
            builder.EntityType<LimitTypeQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new LimitTypeValidator();
            _facade = new LimitTypeFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.EqualTo(5));
            Assert.That(results.Any(x => x.Id == Data.LimitTypes.Under.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LimitTypes.Over.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LimitTypes.NearUnder.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LimitTypes.NearOver.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LimitTypes.ToDelete.Id), Is.True);
        }

        [Test]
        public async Task When_GetAll_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.Results, Is.Null);
        }

        [Test]
        public async Task When_Get_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.LimitTypes.Under;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.Dto, Is.Null);
        }

        [Test]
        public async Task When_Get_InvalidId_Fails()
        {
            var queryResult = await _facade.Get(Guid.Empty);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        [Test]
        public async Task When_Get_BadId_Fails()
        {
            var queryResult = await _facade.Get(Guid.NewGuid());

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        #endregion Get Tests

        #region Create Tests

        [Test]
        public async Task When_Create_Succeeds()
        {
            var toCreate = _mapper.Map<LimitType, LimitTypeQueryDto>(Data.LimitTypes.Under);
            toCreate.Id = Guid.Empty;
            toCreate.I18NKeyName = "New LimitType";
            var callTime = DateTime.UtcNow;

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

            var queryResult = await _facade.Get(commandResult.GeneratedId);

            var dto = queryResult.Dto;
            Assert.That(dto.I18NKeyName, Is.EqualTo(toCreate.I18NKeyName));
            Assert.That(dto.Severity, Is.EqualTo(toCreate.Severity));
            Assert.That(dto.Polarity, Is.EqualTo(toCreate.Polarity));
            Assert.That(dto.CreatedById, Is.EqualTo(_userId));
            Assert.That(dto.ModifiedById, Is.EqualTo(_userId));
            Assert.That(dto.CreatedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
            Assert.That(dto.ModifiedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public async Task When_Create_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var commandResult = await _facade.Create(null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Create_Null_Fails()
        {
            var commandResult = await _facade.Create(null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-102"), Is.True);
        }

        [Test]
        public async Task When_Create_IdNotEmpty_Fails()
        {
            var toCreate = _mapper.Map<LimitType, LimitTypeQueryDto>(Data.LimitTypes.Under);
            toCreate.Id = Guid.NewGuid();
            toCreate.I18NKeyName = "New LimitType";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-211"), Is.True);
        }

        [Test]
        public async Task When_Create_EmptyKeyName_Fails()
        {
            var toCreate = _mapper.Map<LimitType, LimitTypeQueryDto>(Data.LimitTypes.Under);
            toCreate.Id = Guid.Empty;
            toCreate.I18NKeyName = "";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(2));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-207"), Is.True);
        }

        [Test]
        public async Task When_Create_BadKeyName_Fails()
        {
            var toCreate = _mapper.Map<LimitType, LimitTypeQueryDto>(Data.LimitTypes.Under);
            toCreate.Id = Guid.Empty;
            toCreate.I18NKeyName = "123";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-207"), Is.True);
        }

        #endregion Create Tests

        #region Delete Tests

        [Test]
        public async Task When_Delete_Succeeds()
        {
            var commandResult = await _facade.Delete(Data.LimitTypes.ToDelete.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));
            Assert.That(commandResult.ErrorCodes, Is.Null);
        }

        [Test]
        public async Task When_Delete_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var commandResult = await _facade.Delete(Data.LimitTypes.ToDelete.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Delete_NotFound_Fails()
        {
            var commandResult = await _facade.Delete(Guid.Empty);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        #endregion Delete Tests

        #region Update Tests

        [Test]
        public async Task When_Update_Succeeds()
        {
            var seed = Data.LimitTypes.Under;
            const int newSeverity = 3;
            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("Severity", newSeverity);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(seed.Id);

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.Severity, Is.EqualTo(newSeverity));
        }

        [Test]
        public async Task When_Update_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.LimitTypes.Under;
            const int newSeverity = 3;
            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("Severity", newSeverity);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_ChangeId_Fails()
        {
            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("Id", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.LimitTypes.Under.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-203"), Is.True);
        }

        [Test]
        public async Task When_Update_NullDelta_Fails()
        {
            var commandResult = await _facade.Update(Data.LimitTypes.Under.Id, null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-102"), Is.True);
        }

        [Test]
        public async Task When_Update_BadId_Fails()
        {
            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("Severity", 3);

            var commandResult = await _facade.Update(Guid.Empty, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-100"), Is.True);
        }

        [Test]
        public async Task When_Update_EmptyKeyName_Fails()
        {
            var seed = Data.LimitTypes.Under;

            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("I18NKeyName", "");

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(2));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-207"), Is.True);
        }

        [Test]
        public async Task When_Update_BadKeyName_Fails()
        {
            var seed = Data.LimitTypes.Under;

            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("I18NKeyName", "123");

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-207"), Is.True);
        }

        [Test]
        public async Task When_Update_Duplicate_Fails()
        {
            var seed = Data.LimitTypes.Under;

            var delta = new Delta<LimitTypeQueryDto>();
            delta.TrySetPropertyValue("I18NKeyName", Data.LimitTypes.Over.I18NKeyName);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-204"), Is.True);
        }

        #endregion Update Tests
    }
}
