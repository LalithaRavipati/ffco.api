using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using AutoMapper;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Dtos.Dashboards;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class DashboardOptionFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<DashboardOptionQueryDto>> _mockDtoOptions;
        private DashboardOptionFacade _facade;
        private readonly IMapper _mapper;
        private readonly IPrincipal _adHachClaims;
        private readonly IPrincipal _tnt01Claims;
        private readonly IPrincipal _tnt01And02Claims;

        public DashboardOptionFacadeTests()
        {
            MappingManager.Initialize();
            _mapper = MappingManager.AutoMapper;

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<DashboardOptionQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(DashboardOptionQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.Adhach.Id.ToString());
            _adHachClaims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01user.Id.ToString());
            _tnt01Claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01and02user.Id.ToString());
            _tnt01And02Claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<DashboardOptionQueryDto>("DashboardOptions");
            builder.EntityType<DashboardOptionQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            Thread.CurrentPrincipal = _tnt01Claims;

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new DashboardOptionValidator();
            _facade = new DashboardOptionFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_DevTenant01_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // Only dashboard options for DevTenant01 should be returned.
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.Any(x => x.Id == Data.DashboardOptions.DevTenant01_Options.Id), Is.True);
        }

        [Test]
        public async Task When_GetAll_DevTenant01andDevTenant02_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01And02Claims;

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // DashboardsOptions for DevTenant01 and DevTenant02 should be returned.
            Assert.That(results.Count(), Is.EqualTo(2));
            Assert.That(results.Any(x => x.Id == Data.DashboardOptions.DevTenant01_Options.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.DashboardOptions.DevTenant02_Options.Id), Is.True);
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
        public async Task When_Get_Same_Tenant_Succeeds()
        {
            var seed = Data.DashboardOptions.DevTenant01_Options;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
            Assert.That(dto.Options, Is.EqualTo(seed.Options));
        }

        [Test]
        public async Task When_Get_Other_Tenant_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01And02Claims;

            var seed = Data.DashboardOptions.DevTenant02_Options;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
            Assert.That(dto.Options, Is.EqualTo(seed.Options));
        }

        [Test]
        public async Task When_Get_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.DashboardOptions.DevTenant01_Options;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.Dto, Is.Null);
        }

        [Test]
        public async Task When_Get_Other_Tenant_Fails()
        {
            var seed = Data.DashboardOptions.DevTenant02_Options;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
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
        public async Task When_Create_DashboardOption_Succeeds()
        {
            Thread.CurrentPrincipal = _adHachClaims;
            var user = Data.Users.Adhach;

            var toCreate = _mapper.Map<DashboardOption, DashboardOptionCommandDto>(Data.DashboardOptions.HachFusion_ToCreate);
            toCreate.Id = Guid.Empty;
            var callTime = DateTime.UtcNow;

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

            var queryResult = await _facade.Get(commandResult.GeneratedId);

            var dto = queryResult.Dto;
            Assert.That(dto.TenantId, Is.EqualTo(toCreate.TenantId));
            Assert.That(dto.Options, Is.EqualTo(toCreate.Options));
            Assert.That(dto.CreatedById, Is.EqualTo(user.Id));
            Assert.That(dto.ModifiedById, Is.EqualTo(user.Id));
            Assert.That(dto.CreatedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
            Assert.That(dto.ModifiedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public async Task When_Create_OtherTenant_Fails()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.Adhach.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var toCreate = _mapper.Map<DashboardOption, DashboardOptionCommandDto>(Data.DashboardOptions.DevTenant02_Options);
            toCreate.Id = Guid.Empty;
            toCreate.Options = "New Options";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True); // TenantId not found
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
            Thread.CurrentPrincipal = _adHachClaims;
            var commandResult = await _facade.Create(null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-102"), Is.True);
        }

        [Test]
        public async Task When_Create_IdNotEmpty_Fails()
        {
            Thread.CurrentPrincipal = _adHachClaims;

            var toCreate = _mapper.Map<DashboardOption, DashboardOptionCommandDto>(Data.DashboardOptions.HachFusion_ToCreate);
            toCreate.Id = Guid.NewGuid();
            toCreate.Options = "New Options";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-211"), Is.True);
        }

        [Test]
        public async Task When_Create_EmptyTenantId_Fails()
        {
            Thread.CurrentPrincipal = _adHachClaims;

            var toCreate = _mapper.Map<DashboardOption, DashboardOptionCommandDto>(Data.DashboardOptions.HachFusion_ToCreate);
            toCreate.Id = Guid.Empty;
            toCreate.TenantId = Guid.Empty;
            toCreate.Options = "New Options";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Create_BadTenantId_Fails()
        {
            Thread.CurrentPrincipal = _adHachClaims;

            var toCreate = _mapper.Map<DashboardOption, DashboardOptionCommandDto>(Data.DashboardOptions.HachFusion_ToCreate);
            toCreate.Id = Guid.Empty;
            toCreate.TenantId = Guid.NewGuid();
            toCreate.Options = "New Options";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        #endregion Create Tests

        #region Delete Tests

        [Test,Ignore("Fix for referential integrity check")]
        public async Task When_Delete_Succeeds()
        {
            var commandResult = await _facade.Delete(Data.DashboardOptions.DevTenant01_Options.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));
            Assert.That(commandResult.ErrorCodes, Is.Null);
        }

        [Test]
        public async Task When_Delete_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var commandResult = await _facade.Delete(Data.DashboardOptions.DevTenant01_Options.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Delete_NotFound_Fails()
        {
            var commandResult = await _facade.Delete(Guid.Empty);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        public async Task When_Delete_OtherTenant_Fails()
        {
            var commandResult = await _facade.Delete(Data.DashboardOptions.DevTenant02_Options.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        #endregion Delete Tests

        #region Update Tests

        [Test]
        public async Task When_Update_Succeeds()
        {
            var seed = Data.DashboardOptions.DevTenant01_Options;
            const string newOptions = "NewOptions";
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("Options", newOptions);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(seed.Id);

            var dto = queryResult.Dto;
            Assert.That(dto.Options, Is.EqualTo(newOptions));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
        }

        [Test]
        public async Task When_Update_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.DashboardOptions.DevTenant01_Options;
            const string newOptions = "NewOptions";
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("Options", newOptions);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_ChangeId_Fails()
        {
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("Id", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.DashboardOptions.DevTenant01_Options.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-203"), Is.True);
        }

        [Test]
        public async Task When_Update_NullDelta_Fails()
        {
            var commandResult = await _facade.Update(Data.DashboardOptions.DevTenant01_Options.Id, null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-102"), Is.True);
        }

        [Test]
        public async Task When_Update_BadId_Fails()
        {
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("Options", "New Options");

            var commandResult = await _facade.Update(Guid.Empty, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-100"), Is.True);
        }

        [Test]
        public async Task When_Update_OtherTenant_Fails()
        {
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("Options", "New Options");

            var commandResult = await _facade.Update(Data.DashboardOptions.DevTenant02_Options.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-100"), Is.True);
        }

        [Test]
        public async Task When_Update_EmptyTenantId_Fails()
        {
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("TenantId", Guid.Empty);

            var commandResult = await _facade.Update(Data.DashboardOptions.DevTenant01_Options.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Update_BadTenantId_Fails()
        {
            var delta = new Delta<DashboardOptionCommandDto>();
            delta.TrySetPropertyValue("TenantId", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.DashboardOptions.DevTenant01_Options.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        #endregion Update Tests
    }
}
