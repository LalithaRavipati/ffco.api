using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Moq;
using NUnit.Framework;
using System;
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
    public class ParameterValidRangeFacadeTests
    {
        private Mock<DataContext> _mockContext;
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
            _mockContext = new Mock<DataContext>();
            Seeder.InitializeMockDataContext(_mockContext);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                , _mockContext.Object.Users.Single(x => x.UserName == "adhach").Id.ToString());

            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            _facade = new ParameterValidRangeFacade(_mockContext.Object);

            RetrieveSeededData();
        }

        private void RetrieveSeededData()
        {
            // Needed so that audit time fields aren't reset
            _parameterValidRangePh = _mockContext.Object.ParameterValidRanges.First(x => x.Id == new Guid("DD1BB35B-1585-4D6D-B21E-13F06B6A25BF") );
            _parameterValidRangeFlowMinAndMax = _mockContext.Object.ParameterValidRanges.First(x => x.Id == new Guid("4871F673-5308-4DF9-BE96-2CC34AD856E5"));
            _parameterValidRangeFlowMinOnly = _mockContext.Object.ParameterValidRanges.First(x => x.Id == new Guid("D138C2F1-9EEB-4F8C-829A-04D8B9598049"));
            _parameterValidRangeFlowMaxOnly = _mockContext.Object.ParameterValidRanges.First(x => x.Id == new Guid("150CC8CF-49AC-4BC5-A348-9D9FB29BA5DB"));
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;
            Assert.That(results.Count, Is.EqualTo(4));

            Compare(_parameterValidRangePh, results.First(x => x.Id == _parameterValidRangePh.Id));
            Compare(_parameterValidRangeFlowMinAndMax, results.First(x => x.Id == _parameterValidRangeFlowMinAndMax.Id));
            Compare(_parameterValidRangeFlowMinOnly, results.First(x => x.Id == _parameterValidRangeFlowMinOnly.Id));
            Compare(_parameterValidRangeFlowMaxOnly, results.First(x => x.Id == _parameterValidRangeFlowMaxOnly.Id));
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
