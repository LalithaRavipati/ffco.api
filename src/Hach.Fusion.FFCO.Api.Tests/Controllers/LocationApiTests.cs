using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.FFCO.Api.Controllers.v16_1;
using Hach.Fusion.FFCO.Business;
using Hach.Fusion.Data.Database;
using Hach.Fusion.FFCO.Business.Tests;
using Hach.Fusion.FFCO.Business.Validators;

using NUnit.Framework;

using Moq;
using LocationFacade = Hach.Fusion.FFCO.Business.Facades.LocationFacade;

namespace Hach.Fusion.FFCO.Api.Tests.Controllers
{
    [TestFixture]
    public class LocationApiTests
    {

        private LocationsController _controller;
        private readonly Mock<ODataQueryOptions<LocationQueryDto>> _mockDtoOptions;
        private DataContext _context;
        private LocationFacade _facade;
        private Guid _userId = Data.Users.tnt01user.Id;

        public LocationApiTests()
        {
            var builder = BuildODataModel();
            MappingManager.Initialize();

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://tempuri.com");

            _mockDtoOptions = new Mock<ODataQueryOptions<LocationQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(LocationQueryDto), new ODataPath()),
                request);

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<LocationQueryDto>("Locations");
            builder.EntityType<LocationQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        #region Setup and Teardown

        [SetUp]
        public void Setup()
        {
            var claim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _userId.ToString());
            Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { claim }));

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);

            _facade = new LocationFacade(_context, new LocationValidator());

            ODataHelper oDataHelper = new ODataHelper();

            _controller = new LocationsController(oDataHelper, _facade);
            _controller.Request = new HttpRequestMessage();
            _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            // Setting the URI of the request here is needed for the API POST method to work
            _controller.Request.RequestUri = new Uri("http://tempuri.com");

            /* 
             * TODO: akrone - Taking on some Techincal Debt doing this, but pulling the Seeder into it's own project would need to 
             *      be merged into other development work going on for sprint 60
             */
            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _facade = null;
            _controller.Dispose();
            _context.Dispose();

        }
        #endregion

        [Test]
        public async Task When_Get_Succeeds()
        {
            var response = await _controller.Get(_mockDtoOptions.Object).ConfigureAwait(false);
            var result = response.ExecuteAsync(new CancellationToken()).Result;

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
