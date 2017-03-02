using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Routing;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class ParameterTypeFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private readonly Mock<ODataQueryOptions<ParameterTypeQueryDto>> _mockDtoOptions;
        private ParameterTypeFacade _facade;

        public ParameterTypeFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<ParameterTypeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(ParameterTypeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<ParameterTypeQueryDto>("ParameterTypes");
            builder.EntityType<ParameterTypeQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<DataContext>();

            Seeder.InitializeMockDataContext(_mockContext);

            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                , _mockContext.Object.Users.Single(x => x.UserName == "adhach").Id.ToString());

            _facade = new ParameterTypeFacade(_mockContext.Object);
        }

        #region Get Tests

        [Test]
        public async Task When_Get_ParameterTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.Any(x => x.Id == _mockContext.Object.ParameterTypes.Single(t => t.I18NKeyName=="Chemical").Id), Is.True);
            Assert.That(results.Any(x => x.Id == _mockContext.Object.ParameterTypes.Single(t => t.I18NKeyName == "Sensed").Id), Is.True);
        }

        [Test]
        public async Task When_Get_ParameterType_Succeeds()
        {
            var seed = _mockContext.Object.ParameterTypes.Single(t => t.I18NKeyName == "Chemical");

            var queryResult = await _facade.Get(seed.Id);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var dto = queryResult.Dto;
            Assert.That(dto.Id, Is.EqualTo(seed.Id));
            Assert.That(dto.I18NKeyName, Is.EqualTo(seed.I18NKeyName));
        }

        [Test]
        public async Task When_Get_ParameterType_InvalidId_Fails()
        {
            var queryResult = await _facade.Get(Guid.Empty);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        #endregion Get Tests
    }
}
