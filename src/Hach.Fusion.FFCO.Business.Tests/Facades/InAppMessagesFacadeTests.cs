﻿using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Validators;
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
    public class InAppMessageFacadeTests
    {
        private Mock<DataContext> _mockContext;
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
            _mockContext = new Mock<DataContext>();
            var validator = new InAppMessageValidator();
            _facade = new InAppMessageFacade(_mockContext.Object, validator);

            Seeder.InitializeMockDataContext(_mockContext);
            
            Claim userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockContext.Object.Users.Single(u => u.UserName=="tnt01user").Id.ToString());
            _tnt01UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));

            userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockContext.Object.Users.Single(u => u.UserName == "tnt01and02user").Id.ToString());
            _tnt01and02UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));

            userClaim = new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockContext.Object.Users.Single(u => u.UserName == "tnt02user").Id.ToString());
            _tnt02UserClaimPrinciple = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { userClaim }));
        }
        

        #region Get Tests

        [Test]
        public async Task When_Get_InAppMessagesTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u=> u.UserName =="tnt01user").Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt02User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt02user").Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt02User_Fails()
        {
            Thread.CurrentPrincipal = _tnt02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.NotFound));
        }

        [Test]
        public async Task When_Get_InAppMessagesTnt01And02User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01and02UserClaimPrinciple;
            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            var results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));

            queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt02user").Id, _mockDtoOptions.Object);
            Assert.That(queryResult.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));

            results = queryResult.Results;

            Assert.That(results.Count(), Is.GreaterThan(0));
        }

        #endregion Get Tests

        #region Update Tests

        [Test]
        public async Task When_Update_InAppMessageMarkReadTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageBaseDto>();

            toUpdateDto.TrySetPropertyValue("IsRead", true);
            var msg = _mockContext.Object.InAppMessages.Single( x => x.Id == new Guid("4CC29478-EC01-4EF5-B076-637508E241AF"));

            DateTime updateDate = DateTime.UtcNow;
            var updateResult = await _facade.Update(msg.Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            var result = queryResult.Results.SingleOrDefault(x => x.Id == msg.Id);

            Assert.That(result.DateRead, Is.AtLeast(updateDate));
            Assert.That(result.IsRead, Is.EqualTo(true));
            Assert.That(result.IsTrash, Is.EqualTo(msg.IsTrash));
        }

        public async Task When_Update_InAppMessageMarkReadandTrashTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageBaseDto>();

            toUpdateDto.TrySetPropertyValue("IsRead", true);
            toUpdateDto.TrySetPropertyValue("IsTrash", true);
            var msg = _mockContext.Object.InAppMessages.Single( x => x.Id == new Guid("4CC29478-EC01-4EF5-B076-637508E241AF"));

            DateTime updateDate = DateTime.UtcNow;
            var updateResult = await _facade.Update(msg.Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            var result = queryResult.Results.SingleOrDefault(x => x.Id == msg.Id);

            Assert.That(result.DateRead, Is.AtLeast(updateDate));
            Assert.That(result.IsRead, Is.EqualTo(true));
            Assert.That(result.IsTrash, Is.EqualTo(true));
        }

        [Test]
        public async Task When_Update_InAppMessageMarkTrashTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageBaseDto>();

            toUpdateDto.TrySetPropertyValue("IsTrash", true);
            var msg = _mockContext.Object.InAppMessages.Single(x => x.Id == new Guid("4CC29478-EC01-4EF5-B076-637508E241AF"));

            DateTime updateDate = DateTime.UtcNow;
            var updateResult = await _facade.Update(msg.Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            var result = queryResult.Results.SingleOrDefault(x => x.Id == msg.Id);

            Assert.That(result.DateRead, Is.EqualTo(msg.DateRead));
            Assert.That(result.IsRead, Is.EqualTo(msg.IsRead));
            Assert.That(result.IsTrash, Is.EqualTo(true));
        }

        public async Task When_Update_InAppMessageMarkUnReadTnt01User_Succeeds()
        {
            Thread.CurrentPrincipal = _tnt01UserClaimPrinciple;

            var toUpdateDto = new Delta<InAppMessageBaseDto>();

            toUpdateDto.TrySetPropertyValue("IsRead", false);
            var msg = _mockContext.Object.InAppMessages.Single(x => x.Id == new Guid("4CC29478-EC01-4EF5-B076-6375081231AF"));

            DateTime updateDate = DateTime.UtcNow;
            var updateResult = await _facade.Update(msg.Id, toUpdateDto);

            Assert.That(updateResult.StatusCode, Is.EqualTo(FacadeStatusCode.NoContent));

            var queryResult = await _facade.GetByUserId(_mockContext.Object.Users.Single(u => u.UserName == "tnt01user").Id, _mockDtoOptions.Object);
            var result = queryResult.Results.SingleOrDefault(x => x.Id == msg.Id);

            Assert.That(result.DateRead, Is.Null);
            Assert.That(result.IsRead, Is.EqualTo(false));
            Assert.That(result.IsTrash, Is.EqualTo(msg.IsTrash));
        }

        #endregion
    }
}
