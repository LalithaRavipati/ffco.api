using AutoMapper;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Core.Testing;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;
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
    public class ChemicalFormTypesFacadeTests
    {
        private Mock<DataContext> _mockContext;
        private readonly Mock<ODataQueryOptions<ChemicalFormTypeQueryDto>> _mockDtoOptions;
        private ChemicalFormTypesFacade _facade;
        private readonly IMapper _mapper;
        private readonly Guid _userId = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a");


        private List<ChemicalFormType> _chemicalFormTypeSeedData;

        public ChemicalFormTypesFacadeTests()
        {
            MappingManager.Initialize();
            _mapper = MappingManager.AutoMapper;

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<ChemicalFormTypeQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(ChemicalFormTypeQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<ChemicalFormTypeQueryDto>("ChemicalFormTypes");
            builder.EntityType<ChemicalFormTypeQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            _mockContext = SetupMockDbContext();

            _facade = new ChemicalFormTypesFacade(_mockContext.Object);
        }

        private Mock<DataContext> SetupMockDbContext()
        {
            _chemicalFormTypeSeedData = GetChemicalFormTypeSeedData();

            var result = new Mock<DataContext>();

            result.Setup(x => x.ChemicalFormTypes).Returns(new InMemoryDbSet<ChemicalFormType>(_chemicalFormTypeSeedData));

            result.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            return result;
        }

        [TearDown]
        public void TearDown()
        {
            _mockContext.Object.Dispose();
        }

        private static List<ChemicalFormType> GetChemicalFormTypeSeedData()
        {
            return new List<ChemicalFormType>
            {
                new ChemicalFormType
                {
                    Id = Guid.Parse("029654E6-996F-4D13-B9F7-5DAE0D5EE632"),
                    I18NKeyName = "Ethanol",
                    Form = "C2H6O"
                },
                new ChemicalFormType
                {
                      Id = Guid.Parse("1705A4BF-CE9B-455E-A325-E32971265153"),
                      I18NKeyName = "Caffeine",
                      Form = "C8H10N4O2"
                },
                new ChemicalFormType
                {
                      Id = Guid.Parse("781E04A9-FDCF-4968-89D6-FE92023B28BF"),
                      I18NKeyName = "Alum",
                      Form = "12H2O"},
                new ChemicalFormType
                {
                      Id = Guid.Parse("350F30B7-E48D-4A24-8A6E-8ACAECA619F2"),
                      I18NKeyName = "Water",
                      Form = "H2O"
                },
                new ChemicalFormType
                {
                      Id = Guid.Parse("B32B2B70-F0D1-42F7-9FF9-582DF538756C"),
                      I18NKeyName = "GalliumArsenide",
                      Form = "GaAs"
                }
            };
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_ChemicalFormTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.EqualTo(5));

            Assert.That(results.SingleOrDefault(x => x.Id == _chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "Caffeine").Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x => x.Id == _chemicalFormTypeSeedData.Single(c => c.I18NKeyName == "Alum").Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x => x.Id == _chemicalFormTypeSeedData.Single(c => c.I18NKeyName == "Ethanol").Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x => x.Id == _chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "GalliumArsenide").Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x => x.Id == _chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "Water").Id), Is.Not.Null);
        }

        [Test]
        public async Task When_GetOne_ChemicalFormType_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var queryResult = await _facade.Get(_chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "Caffeine").Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            Assert.That(queryResult.Dto, Is.Not.Null);
            Assert.That(queryResult.Dto.Id, Is.EqualTo(_chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "Caffeine").Id));
            Assert.That(queryResult.Dto.Form, Is.EqualTo(_chemicalFormTypeSeedData.Single(c => c.I18NKeyName== "Caffeine").Form));
        }

        [Test]
        public async Task When_GetAll_NoLoggedInUser_Fails()
        {
            Thread.CurrentPrincipal = null;

            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Unauthorized));
            Assert.That(queryResult.Results, Is.Null);
        }


        [Test]
        public async Task When_Get_InvalidId_Fails()
        {
            var queryResult = await _facade.Get(Guid.Empty);

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        [Test]
        public async Task When_Get_BadId_Fails()
        {
            var queryResult = await _facade.Get(Guid.NewGuid());

            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
            Assert.That(queryResult.Dto, Is.Null);
        }

        #endregion Get Tests

    }
}
