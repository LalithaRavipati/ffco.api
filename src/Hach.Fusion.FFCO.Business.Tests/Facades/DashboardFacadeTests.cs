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
using Hach.Fusion.FFCO.Dtos.Dashboards;
using Hach.Fusion.FFCO.Entities.Seed;
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
        private readonly Guid _userId = Data.Users.tnt01user.Id;

        public DashboardFacadeTests()
        {
            MappingManager.Initialize();

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
        public async Task When_Get_DevTenant01_Dashboards_Succeeds()
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
        public async Task When_Get_DevTenant01andDevTenant02_Dashboards_Succeeds()
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
        public async Task When_Get_HachFusion_Dashboards_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.Adhach.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            // There are no dashboards in the Hach Fusion tenant so zero dashboards should be returned.
            Assert.That(results.Count(), Is.EqualTo(0));
        }

        #endregion Get Tests
    }
}
