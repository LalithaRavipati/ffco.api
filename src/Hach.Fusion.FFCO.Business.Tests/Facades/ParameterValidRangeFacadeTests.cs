using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.FFCO.Business.Facades;
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
    public class ParameterValidRangeFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<ParameterValidRangeQueryDto>> _mockDtoOptions;
        private ParameterValidRangeFacade _facade;

        private ParameterValidRange _parameterValidRangePh;
        private ParameterValidRange _parameterValidRangeFlowMinAndMax;
        private ParameterValidRange _parameterValidRangeFlowMinOnly;
        private ParameterValidRange _parameterValidRangeFlowMaxOnly;

        public ParameterValidRangeFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<ParameterValidRangeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(ParameterValidRangeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<ParameterValidRangeQueryDto>("ParameterValidRanges");
            builder.EntityType<ParameterValidRangeQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "5170AE58-21B4-40F5-A025-E886489E9B82");
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            _facade = new ParameterValidRangeFacade(_context);

            Seeder.SeedWithTestData(_context);
            RetrieveSeededData();
        }

        private void RetrieveSeededData()
        {
            // Needed so that audit time fields aren't reset
            _parameterValidRangePh = _context.ParameterValidRanges.First(x => x.Id == Data.ParameterValidRanges.pH.Id);
            _parameterValidRangeFlowMinAndMax = _context.ParameterValidRanges.First(x => x.Id == Data.ParameterValidRanges.FlowMinAndMax.Id);
            _parameterValidRangeFlowMinOnly = _context.ParameterValidRanges.First(x => x.Id == Data.ParameterValidRanges.FlowMinOnly.Id);
            _parameterValidRangeFlowMaxOnly = _context.ParameterValidRanges.First(x => x.Id == Data.ParameterValidRanges.FlowMaxOnly.Id);
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
            Assert.That(results.Count, Is.EqualTo(4));

            Compare(_parameterValidRangePh, results.First(x => x.Id == Data.ParameterValidRanges.pH.Id));
            Compare(_parameterValidRangeFlowMinAndMax, results.First(x => x.Id == Data.ParameterValidRanges.FlowMinAndMax.Id));
            Compare(_parameterValidRangeFlowMinOnly, results.First(x => x.Id == Data.ParameterValidRanges.FlowMinOnly.Id));
            Compare(_parameterValidRangeFlowMaxOnly, results.First(x => x.Id == Data.ParameterValidRanges.FlowMaxOnly.Id));
        }

        [Test]
        public async Task When_GetOne_Succeeds()
        {
            var queryResult = await _facade.Get(_parameterValidRangePh.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            
            Compare(_parameterValidRangePh, dto);
        }

        [Test]
        public async Task When_GetOne_EntityNotFound_Fails()
        {
            var queryResult = await _facade.Get(Guid.NewGuid());

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        #endregion Get Tests

        #region Compare

        private static void Compare(ParameterValidRange entity, ParameterValidRangeQueryDto dto)
        {
            Assert.That(dto.Id, Is.EqualTo(entity.Id));
            Assert.That(dto.ParameterId, Is.EqualTo(entity.ParameterId));
            Assert.That(dto.MinValue.HasValue, Is.EqualTo(entity.MinValue.HasValue));
            if (dto.MinValue.HasValue && entity.MinValue.HasValue)
                Assert.That(dto.MinValue.Value, Is.EqualTo(entity.MinValue.Value));

            Assert.That(dto.MaxValue.HasValue, Is.EqualTo(entity.MaxValue.HasValue));
            if (dto.MaxValue.HasValue && entity.MaxValue.HasValue)
                Assert.That(dto.MaxValue.Value, Is.EqualTo(entity.MaxValue.Value));

            Assert.That(dto.CreatedById, Is.EqualTo(entity.CreatedById));
            Assert.That(dto.ModifiedById, Is.EqualTo(entity.ModifiedById));
            Assert.That(dto.CreatedOn, Is.EqualTo(entity.CreatedOn));
            Assert.That(dto.ModifiedOn, Is.EqualTo(entity.ModifiedOn));
        }
        #endregion Compare
    }
}
