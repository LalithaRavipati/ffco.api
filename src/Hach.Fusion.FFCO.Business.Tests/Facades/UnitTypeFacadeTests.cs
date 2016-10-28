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
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class UnitTypeFacadeTests
    {
        private DataContext _context;
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
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "5170AE58-21B4-40F5-A025-E886489E9B82");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new UnitTypeValidator();
            _facade = new UnitTypeFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_Get_UnitTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Any(x => x.Id == Data.UnitTypes.Centigrade.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.UnitTypes.Fahrenheit.Id), Is.True);
            //Assert.That(results.Any(x => x.Id == Data.UnitTypes.Hectopascal.Id), Is.True);
        }

        #endregion Get Tests
    }
}
