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
    public class UnitTypeFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private readonly Mock<ODataQueryOptions<UnitTypeQueryDto>> _mockDtoOptions;
        private UnitTypeFacade _facade;

        public UnitTypeFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<UnitTypeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(UnitTypeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LocationQueryDto>("Locations");
            builder.EntityType<LocationQueryDto>().HasKey(x => x.Id);

            builder.EntitySet<UnitTypeQueryDto>("UnitTypes");
            builder.EntityType<UnitTypeQueryDto>().HasKey(x => x.Id);


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

            var validator = new UnitTypeValidator();
            _facade = new UnitTypeFacade(_mockContext.Object, validator);
        }

        #region Get Tests

        [Test]
        public async Task When_Get_UnitTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == _mockContext.Object.UnitTypes.Single(ut => ut.I18NKeyName == "Centigrade").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.UnitTypes.Single(ut => ut.I18NKeyName == "Fahrenheit").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.UnitTypes.Single(ut => ut.I18NKeyName == "Hectopascal").Id), Is.True);
        }

        #endregion Get Tests
    }
}
