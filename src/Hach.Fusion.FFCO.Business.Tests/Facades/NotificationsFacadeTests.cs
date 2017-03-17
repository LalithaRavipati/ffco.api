using System.Linq;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Notifications;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.Data.Dtos;

using Moq;
using NUnit.Framework;

namespace Hach.Fusion.FFCO.Business.Tests.Facades
{
    [TestFixture]
    public class NotificationsFacadeTests
    {
        private NotificationsFacade _facade;
        private NotificationValidator _validator;
        private Mock<INotificationSender> _notificationSender;

        [SetUp]
        public void Setup()
        {
            _notificationSender = new Mock<INotificationSender>();
            _notificationSender.Setup(x => x.SendAll(It.IsAny<string>()))
                .Returns(Task.Factory.StartNew(() => { }));
            _notificationSender.Setup(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Factory.StartNew(() => { }));

            _validator = new NotificationValidator();
            _facade = new NotificationsFacade(_notificationSender.Object, _validator);
        }

        [Test]
        public void When_SendBroadcastNotification_Succeeds()
        {
            var dto = new GenericNotificationDto()
            {
                BroadcastAll = true,
                Message = "Test Message"
            };

            var result = _facade.SendNotification(dto).Result;

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            _notificationSender.Verify(x => x.SendAll(It.IsAny<string>()), Times.Once);
            _notificationSender.Verify(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void When_SendGroupNotification_Succeeds()
        {
            var dto = new GenericNotificationDto
            {
                BroadcastAll = false,
                GroupName = "Test Group",
                Message = "Test Message"
            };

            var result = _facade.SendNotification(dto).Result;

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.Ok));
            _notificationSender.Verify(x => x.SendAll(It.IsAny<string>()), Times.Never);
            _notificationSender.Verify(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void When_SendBroadcastNotification_EmptyMessage_Fails()
        {
            var dto = new GenericNotificationDto
            {
                BroadcastAll = true,
                Message = ""
            };

            var result = _facade.SendNotification(dto).Result;

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            Assert.That(result.ErrorCodes.Count, Is.EqualTo(1));
            var validationError = result.ErrorCodes.FirstOrDefault() as ValidationErrorCode;
            Assert.That(validationError, Is.Not.Null);
            Assert.That(validationError.Code, Is.EqualTo("FFERR-201"));
            Assert.That(validationError.Property, Is.EqualTo(nameof(dto.Message)));
            _notificationSender.Verify(x => x.SendAll(It.IsAny<string>()), Times.Never);
            _notificationSender.Verify(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void When_SendGroupNotification_EmptyGroupName_Fails()
        {
            var dto = new GenericNotificationDto
            {
                BroadcastAll = false,
                GroupName = "",
                Message = "Test Message"
            };

            var result = _facade.SendNotification(dto).Result;

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            _notificationSender.Verify(x => x.SendAll(It.IsAny<string>()), Times.Never);
            _notificationSender.Verify(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void When_SendGroupNotification_EmptyMessage_Fails()
        {
            var dto = new GenericNotificationDto
            {
                BroadcastAll = false,
                GroupName = "Test Group",
                Message = ""
            };

            var result = _facade.SendNotification(dto).Result;

            Assert.That(result.StatusCode, Is.EqualTo(FacadeStatusCode.BadRequest));
            _notificationSender.Verify(x => x.SendAll(It.IsAny<string>()), Times.Never);
            _notificationSender.Verify(x => x.SendGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
