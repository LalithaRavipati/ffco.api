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
    public class InAppMessageFacadeTests
    {
        private DataContext _context;
        private readonly Mock<ODataQueryOptions<InAppMessageQueryDto>> _mockDtoOptions;
        private InAppMessageFacade _facade;
        private ClaimsPrincipal _tnt01UserClaimPrinciple;
        private ClaimsPrincipal _tnt01and02UserClaimPrinciple;
        private ClaimsPrincipal _tnt02UserClaimPrinciple;

        public InAppMessageFacadeTests()
        {
            MappingManager.Initialize();

            var builder = BuildODataModel();

            _mockDtoOptions = new Mock<ODataQueryOptions<InAppMessageQueryDto>>(
                new ODataQueryContext(builder.GetEdmModel(), typeof(InAppMessageQueryDto), new ODataPath()), new HttpRequestMessage());

            _mockDtoOptions.Setup(x => x.Validate(It.IsAny<ODataValidationSettings>())).Callback(() => { });

            Claim userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01user.Id.ToString());
            _tnt01UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));

            userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt01and02user.Id.ToString());
            _tnt01and02UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));

            userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Data.Users.tnt02user.Id.ToString());
            _tnt02UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));
        }

        private static ODataModelBuilder BuildODataModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<InAppMessageQueryDto>("InAppMessages");
            builder.EntityType<InAppMessageQueryDto>().HasKey(x => x.Id);

            return builder;
        }

        [SetUp]
        public void Setup()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var connectionString = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;
            _context = new DataContext(connectionString);
            var validator = new InAppMessageValidator();
            _facade = new InAppMessageFacade(_context, validator);

            Seeder.SeedWithTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task When_Get_InAppMessagesTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(Data.Users.tnt01user.Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt02User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(Data.Users.tnt02user.Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt02User_Fails()
        {
            Thread.CurrentPrincipal = _tnt02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(Data.Users.tnt01user.Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt01And02User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01and02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(Data.Users.tnt01user.Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));

            queryResult = await _facade.GetByUserId(Data.Users.tnt02user.Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        #endregion Get Tests

        #region Update Tests

        [Test]
        public async Task When_Update_InAppMessageReadTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageCommandDto>();

            toUpdateDto.TrySetPropertyValue("IsRead", true);
            var Id = Data.InAppMessages.tnt01UserMessageLimitViolation1.Id;
            var updateResult = await _facade.Update(Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(Data.Users.tnt01user.Id, _mockDtoOptions.Object);
            // Check that the IsRead flag is true and the DateRead time is within the last 5 seconds.
        }

        [Test]
        public async Task When_Update_InAppMessageTrashTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageCommandDto>();

            toUpdateDto.TrySetPropertyValue("IsRead", true);
            var Id = Data.InAppMessages.tnt01UserMessageLimitViolation1.Id;
            var updateResult = await _facade.Update(Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(Data.Users.tnt01user.Id, _mockDtoOptions.Object);
            // Check that the IsRead flag is true and the DateRead time is within the last 5 seconds.
        }

        #endregion
    }
}
