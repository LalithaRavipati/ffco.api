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
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class ParameterFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<ParameterDto>> _mockDtoOptions;
        private ParameterFacade _facade;

        public ParameterFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<ParameterDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(ParameterDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<ParameterDto>("Parameters");
            builder.EntityType<ParameterDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "5170AE58-21B4-40F5-A025-E886489E9B82");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            _facade = new ParameterFacade(_context);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_Get_Parameters_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Any(x => x.Id == Data.Parameters.Flow.Id), Is.True);
            Assert.That(results.Any(x => x.Id == Data.Parameters.pH.Id), Is.True);
        }

        [Test]
        public async Task When_Get_Parameter_Succeeds()
        {
            var seed = Data.Parameters.Flow;

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.I18NKeyName, Is.EqualTo(seed.I18NKeyName));
            Assert.That(dto.BaseChemicalFormTypeId, Is.EqualTo(seed.BaseChemicalFormTypeId));
            Assert.That(dto.BaseUnitTypeId, Is.EqualTo(seed.BaseUnitTypeId));
            Assert.That(dto.CreatedById, Is.EqualTo(seed.CreatedById));
            Assert.That(dto.ModifiedById, Is.EqualTo(seed.ModifiedById));
            Assert.That(dto.CreatedOn, Is.EqualTo(seed.CreatedOn).Within(TimeSpan.FromSeconds(1)));
            Assert.That(dto.ModifiedOn, Is.EqualTo(seed.ModifiedOn).Within(TimeSpan.FromSeconds(1)));
            Assert.That(dto.ParameterTypeId, Is.EqualTo(seed.ParameterTypeId));
        }

        [Test]
        public async Task When_Get_Parameter_InvalidId_Fails()
        {
            var queryResult = await _facade.Get(Guid.Empty);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        #endregion Get Tests
    }
}
