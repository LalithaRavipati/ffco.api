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
using Hach.Fusion.FFCO.Core.Dtos.LocationType;
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class LocationTypeFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<LocationTypeQueryDto>> _mockDtoOptions;
        private LocationTypeFacade _facade;

        public LocationTypeFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<LocationTypeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(LocationTypeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LocationQueryDto>("Locations");
            builder.EntityType<LocationQueryDto>().HasKey(x => x.Id);

            builder.EntitySet<LocationTypeQueryDto>("LocationTypes");
            builder.EntityType<LocationTypeQueryDto>().HasKey(x => x.Id);


            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "5170AE58-21B4-40F5-A025-E886489E9B82");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new LocationTypeValidator();
            _facade = new LocationTypeFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_Get_LocationTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == Data.LocationTypes.Plant.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LocationTypes.Process.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LocationTypes.SamplingSite.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.LocationTypes.Distribution.Id), Is.True);

            var loc = results.FirstOrDefault(x => x.Id == Data.LocationTypes.FortCollinsPlant.Id);
            Assert.That(loc, Is.Not.Null);
            Assert.That(loc.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsSystemA.Id), Is.True);
            Assert.That(loc.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsSystemB.Id), Is.True);

            var sys = loc.LocationTypes.First(x => x.Id == Data.LocationTypes.FortCollinsSystemA.Id);
            Assert.That(sys.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsCollectorA1.Id), Is.True);
            Assert.That(sys.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsCollectorA2.Id), Is.True);

            sys = loc.LocationTypes.First(x => x.Id == Data.LocationTypes.FortCollinsSystemB.Id);
            Assert.That(sys.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsCollectorB1.Id), Is.True);
            Assert.That(sys.LocationTypes.Any(x => x.Id == Data.LocationTypes.FortCollinsCollectorB2.Id), Is.True);
        }

        #endregion Get Tests
    }
}
