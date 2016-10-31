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
using AutoMapper;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;
using Hach.Fusion.FFCO.Core.Seed;
using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class ChemicalFormTypesFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<ChemicalFormTypeQueryDto>> _mockDtoOptions;
        private ChemicalFormTypesFacade _facade;
        private readonly IMapper _mapper;
        private readonly Guid _userId = Data.Users.tnt01user.Id;

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

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            _facade = new ChemicalFormTypesFacade(_context);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_GetAll_ChemicalFormTypes_Succeeds()
        {
            var queryResult = await _facade.Get(_mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.EqualTo(5));

            Assert.That(results.SingleOrDefault(x=> x.Id == Data.ChemicalFormTypes.Caffeine.Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x=> x.Id == Data.ChemicalFormTypes.Alum.Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x=> x.Id == Data.ChemicalFormTypes.Ethanol.Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x=> x.Id == Data.ChemicalFormTypes.GalliumArsenide.Id), Is.Not.Null);
            Assert.That(results.SingleOrDefault(x=> x.Id == Data.ChemicalFormTypes.Water.Id), Is.Not.Null);
        }

        [Test]
        public async Task When_GetOne_ChemicalFormType_Succeeds()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                Data.Users.tnt01and02user.Id.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {claim}));

            var queryResult = await _facade.Get(Data.ChemicalFormTypes.Caffeine.Id);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            Assert.That(queryResult.Dto, Is.Not.Null);
            Assert.That(queryResult.Dto.Id, Is.EqualTo(Data.ChemicalFormTypes.Caffeine.Id));
            Assert.That(queryResult.Dto.Form, Is.EqualTo(Data.ChemicalFormTypes.Caffeine.Form));
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
