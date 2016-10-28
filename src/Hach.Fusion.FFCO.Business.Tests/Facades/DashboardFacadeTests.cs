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
using AutoMapper;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Core.Dtos.Dashboards;
using Hach.Fusion.FFCO.Core.Entities;
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class DashboardFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<DashboardQueryDto>> _mockDtoOptions;
        private DashboardFacade _facade;
        private readonly IMapper _mapper;
        private readonly Guid _userId = Data.Users.tnt01user.Id;

        public DashboardFacadeTests()
        {
            MappingManager.Initialize();
            _mapper = MappingManager.AutoMapper;

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<DashboardQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(DashboardQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<DashboardQueryDto>("Dashboards");
            builder.EntityType<DashboardQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new DashboardValidator();
            _facade = new DashboardFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_DevTenant01_Dashboards_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // Only dashboards for DevTenant01 should be returned.
            Assert.That(results.Count(), Is.EqualTo(5));
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01user_Dashboard_1.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01user_Dashboard_2.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01and02user_Dashboard_4.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.Test_tnt01user_ToDelete.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.Test_tnt01user_ToUpdate.Id), Is.True);
        }

        [Test]
        public async Task When_GetAll_DevTenant01andDevTenant02_Dashboards_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01and02user.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // Dashboards for DevTenant01 and DevTenant02 should be returned.
            Assert.That(results.Count(), Is.EqualTo(7));
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01user_Dashboard_1.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01user_Dashboard_2.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01and02user_Dashboard_4.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.Test_tnt01user_ToDelete.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.Test_tnt01user_ToUpdate.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt02user_Dashboard_3.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Dashboards.tnt01and02user_Dashboard_5.Id), Is.True);
        }

        [Test]
        public async Task When_GetAll_HachFusion_Dashboards_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.Adhach.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // There are no dashboards in the Hach Fusion tenant so zero dashboards should be returned.
            Assert.That(results.Count(), Is.EqualTo(0));
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
        public async Task When_Get_Dashboard_Same_Tenant_Succeeds()
        {
            var seed = Data.Dashboards.tnt01user_Dashboard_1;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
            Assert.That(dto.DashboardOptionId, Is.EqualTo(seed.DashboardOptionId));
            Assert.That(dto.OwnerUserId, Is.EqualTo(seed.OwnerUserId));
            Assert.That(dto.Layout, Is.EqualTo(seed.Layout));
        }

        [Test]
        public async Task When_Get_Dashboard_Other_Tenant_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01and02user.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var seed = Data.Dashboards.tnt01user_Dashboard_1;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
            Assert.That(dto.DashboardOptionId, Is.EqualTo(seed.DashboardOptionId));
            Assert.That(dto.OwnerUserId, Is.EqualTo(seed.OwnerUserId));
            Assert.That(dto.Layout, Is.EqualTo(seed.Layout));
        }

        [Test]
        public async Task When_Get_Dashboard_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.Dashboards.tnt01user_Dashboard_1;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.Dto, Is.Null);
        }

        [Test]
        public async Task When_Get_Dashboard_Other_Tenant_Fails()
        {
            var seed = Data.Dashboards.tnt02user_Dashboard_3;

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
        public async Task When_Create_Succeeds()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";
            var callTime = DateTime.UtcNow;

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Created));
            Assert.That(commandResult.GeneratedId, Is.Not.EqualTo(Guid.Empty));

            var queryResult = await _facade.Get(commandResult.GeneratedId);

            var dto = queryResult.Dto;
            Assert.That(dto.TenantId, Is.EqualTo(toCreate.TenantId));
            Assert.That(dto.OwnerUserId, Is.EqualTo(_userId));
            Assert.That(dto.Name, Is.EqualTo(toCreate.Name));
            Assert.That(dto.DashboardOptionId, Is.EqualTo(toCreate.DashboardOptionId));
            Assert.That(dto.Layout, Is.EqualTo(toCreate.Layout));
            Assert.That(dto.CreatedById, Is.EqualTo(_userId));
            Assert.That(dto.ModifiedById, Is.EqualTo(_userId));
            Assert.That(dto.CreatedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
            Assert.That(dto.ModifiedOn, Is.EqualTo(callTime).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public async Task When_Create_OtherTenant_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt02user_Dashboard_3);
            toCreate.Id = Guid.Empty;
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(2));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True); // TenantId not found
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True); // DashboardOptionId not found
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
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.NewGuid();
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-211"), Is.True);
        }

        [Test]
        public async Task When_Create_BadName_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.Name = "123";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-207"), Is.True);
        }

        [Test]
        public async Task When_Create_EmptyTenantId_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.TenantId = Guid.Empty;
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Create_BadTenantId_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.TenantId = Guid.NewGuid();
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        [Test]
        public async Task When_Create_EmptyDashboardOptionId_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.DashboardOptionId = Guid.Empty;
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Create_BadDashboardOptionId_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;
            toCreate.DashboardOptionId = Guid.NewGuid();
            toCreate.Name = "New Dashboard";
            toCreate.Layout = "New Dashboard";

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        [Test]
        public async Task When_Create_Duplicate_Fails()
        {
            var toCreate = _mapper.Map<Dashboard, DashboardCommandDto>(Data.Dashboards.tnt01user_Dashboard_1);
            toCreate.Id = Guid.Empty;

            var commandResult = await _facade.Create(toCreate);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-101"), Is.True);
        }

        #endregion Create Tests

        #region Delete Tests

        [Test]
        public async Task When_Delete_Succeeds()
        {
            var commandResult = await _facade.Delete(Data.Dashboards.Test_tnt01user_ToDelete.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));
            Assert.That(commandResult.ErrorCodes, Is.Null);
        }

        [Test]
        public async Task When_Delete_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var commandResult = await _facade.Delete(Data.Dashboards.Test_tnt01user_ToDelete.Id);

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
            var commandResult = await _facade.Delete(Data.Dashboards.tnt02user_Dashboard_3.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        public async Task When_Delete_NotReportOwner_Fails()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01and02user.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var commandResult = await _facade.Delete(Data.Dashboards.Test_tnt01user_ToDelete.Id);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-106"), Is.True);
        }

        #endregion Delete Tests

        #region Update Tests

        [Test]
        public async Task When_Update_Succeeds()
        {
            var seed = Data.Dashboards.Test_tnt01user_ToUpdate;
            const string newLayout = "NewLayout";
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Layout", newLayout);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.Get(seed.Id);

            var dto = queryResult.Dto;
            Assert.That(dto.Layout, Is.EqualTo(newLayout));
            Assert.That(dto.TenantId, Is.EqualTo(seed.TenantId));
            Assert.That(dto.DashboardOptionId, Is.EqualTo(seed.DashboardOptionId));
        }

        [Test]
        public async Task When_Update_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var seed = Data.Dashboards.Test_tnt01user_ToUpdate;
            const string newLayout = "NewLayout";
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Layout", newLayout);

            var commandResult = await _facade.Update(seed.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_ChangeId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Id", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-203"), Is.True);
        }

        [Test]
        public async Task When_Update_NullDelta_Fails()
        {
            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, null);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-102"), Is.True);
        }

        [Test]
        public async Task When_Update_BadId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Layout", "New Layout");

            var commandResult = await _facade.Update(Guid.Empty, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-100"), Is.True);
        }

        [Test]
        public async Task When_Update_OtherTenant_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Layout", "New Layout");

            var commandResult = await _facade.Update(Data.Dashboards.tnt02user_Dashboard_3.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-100"), Is.True);
        }

        [Test]
        public async Task When_Update_EmptyTenantId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("TenantId", Guid.Empty);

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Update_BadTenantId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("TenantId", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        [Test]
        public async Task When_Update_EmptyDashboardOptionId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("DashboardOptionId", Guid.Empty);

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-201"), Is.True);
        }

        [Test]
        public async Task When_Update_BadDashboardOptionId_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("DashboardOptionId", Guid.NewGuid());

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-209"), Is.True);
        }

        [Test]
        public async Task When_Update_Duplicate_Fails()
        {
            var delta = new Delta<DashboardCommandDto>();
            delta.TrySetPropertyValue("Name", Data.Dashboards.Test_tnt01user_ToDelete.Name);

            var commandResult = await _facade.Update(Data.Dashboards.Test_tnt01user_ToUpdate.Id, delta);

            Assert.That(commandResult.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(commandResult.ErrorCodes.Count, Is.EqualTo(1));
            Assert.That(commandResult.ErrorCodes.Any(x => x.Code == "FFERR-101"), Is.True);
        }

        #endregion Update Tests
    }
}
