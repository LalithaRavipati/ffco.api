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
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", 
                Data.Users.tnt01and02user.Id.ToString());
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

        [Test]
        public async Task When_Get_LocationTypes_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        [Test]
        public async Task When_Get_LocationType_Succeeds()
        {
            var dto = Data.LocationTypes.Process;
            var queryResult = await _facade.Get(dto.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.Id == dto.Id);
        }

        [Test]
        public async Task When_Get_LocationType_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var queryResult = await _facade.Get(Data.LocationTypes.Process.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        #endregion Get Tests
    }
}
