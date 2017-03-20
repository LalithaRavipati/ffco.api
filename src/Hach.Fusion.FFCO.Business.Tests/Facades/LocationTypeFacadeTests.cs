using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
using Moq;
using NUnit.Framework;
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
    public class LocationTypeFacadeTests
    {
        private Mock<DataContext> _mockContext;
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
            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                , _mockContext.Object.Users.Single(x => x.UserName == "adhach").Id.ToString());

            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var validator = new LocationTypeValidator();
            _facade = new LocationTypeFacade(_mockContext.Object, validator);

        }

        #region Get Tests

        [Test]
        public async Task When_Get_LocationTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="Operation").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="Process").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="Sampling Site").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="Distribution").Id), Is.True);

            var loc = results.FirstOrDefault(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="FortCollinsOperation").Id);
            Assert.That(loc, Is.Not.Null);
            Assert.That(loc.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName=="FortCollinsSystemA").Id), Is.True);
            Assert.That(loc.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsSystemB").Id), Is.True);

            var sys = loc.LocationTypes.First(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsSystemA").Id);
            Assert.That(sys.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsCollectorA1").Id), Is.True);
            Assert.That(sys.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsCollectorA2").Id), Is.True);

            sys = loc.LocationTypes.First(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsSystemB").Id);
            Assert.That(sys.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsCollectorB1").Id), Is.True);
            Assert.That(sys.LocationTypes.Any(x => x.Id == _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "FortCollinsCollectorB2").Id), Is.True);
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
            var dto = _mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "Process");
            var queryResult = await _facade.Get(dto.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            Assert.That(queryResult.Dto.Id == dto.Id);
        }

        [Test]
        public async Task When_Get_LocationType_UnauthenticatedUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var queryResult = await _facade.Get(_mockContext.Object.LocationTypes.Single(t=> t.I18NKeyName== "Process").Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
        }

        #endregion Get Tests
    }
}
