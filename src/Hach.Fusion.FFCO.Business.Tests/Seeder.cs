using System.Linq;
using Hach.Fusion.Data.Entities;
using System.Collections.Generic;
using System;
using Hach.Fusion.Data.Database;
using Moq;
using Hach.Fusion.Core.Test.EntityFramework;

namespace Hach.Fusion.FFCO.Business.Tests
{
    public static class Seeder
    {
        /// <summary>
        /// Initializes the <see cref="Mock{DataContext}"/> with a base set of data and entity relationships
        /// </summary>
        /// <param name="mockDataContext"><see cref="Mock{DataContext}"/> to be initialized</param>
        public static void InitializeMockDataContext(Mock<DataContext> mockDataContext)
        {
            // Set up the Mock returns
            mockDataContext.Setup(x => x.Dashboards).Returns(new InMemoryDbSet<Dashboard>(GetDashboards()));
            mockDataContext.Setup(x => x.Tenants).Returns(new InMemoryDbSet<Tenant>(GetTenants()));
            mockDataContext.Setup(x => x.Users).Returns(new InMemoryDbSet<User>(GetUsers()));
            mockDataContext.Setup(x => x.DashboardOptions).Returns(new InMemoryDbSet<DashboardOption>(GetDashboardOptions()));
            mockDataContext.Setup(x => x.ChemicalFormTypes).Returns(new InMemoryDbSet<ChemicalFormType>(GetChemicalFormTypes()));
            mockDataContext.Setup(x => x.InAppMessages).Returns(new InMemoryDbSet<InAppMessage>(GetInAppMessages()));
            mockDataContext.Setup(x => x.MessageTypes).Returns(new InMemoryDbSet<MessageType>(GetMessageTypes()));
            mockDataContext.Setup(x => x.LimitTypes).Returns(new InMemoryDbSet<LimitType>(GetLimitTypes()));
            mockDataContext.Setup(x => x.UnitTypes).Returns(new InMemoryDbSet<UnitType>(GetUnitTypes()));
            mockDataContext.Setup(x => x.UnitTypeGroups).Returns(new InMemoryDbSet<UnitTypeGroup>(GetUnitTypeGroups()));
            mockDataContext.Setup(x => x.Parameters).Returns(new InMemoryDbSet<Parameter>(GetParameters()));
            mockDataContext.Setup(x => x.ParameterTypes).Returns(new InMemoryDbSet<ParameterType>(GetParameterTypes()));
            mockDataContext.Setup(x => x.ParameterValidRanges).Returns(new InMemoryDbSet<ParameterValidRange>(GetParameterValidRanges()));
            mockDataContext.Setup(x => x.Locations).Returns(new InMemoryDbSet<Location>(GetLocations()));
            mockDataContext.Setup(x => x.LocationTypes).Returns(new InMemoryDbSet<LocationType>(GetLocationTypes()));
            mockDataContext.Setup(x => x.ProductOfferingTenantLocations).Returns(new InMemoryDbSet<ProductOfferingTenantLocation>(GetPOTLs()));
            mockDataContext.Setup(x => x.ProductOfferings).Returns(new InMemoryDbSet<ProductOffering>(GetProductOfferings()));
            mockDataContext.Setup(x => x.LocationLogEntries).Returns(new InMemoryDbSet<LocationLogEntry>(GetLocationLogEntries()));

            mockDataContext.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            // Setup relationships
            var dataContext = mockDataContext.Object;

            var devTenant01 = dataContext.Tenants.Single(x => x.Name == "Dev Tenant 01");
            var devTenant02 = dataContext.Tenants.Single(x => x.Name == "Dev Tenant 02");
            var fusionTenant = dataContext.Tenants.Single(x => x.Name == "Hach Fusion");

            devTenant01.Users.Add(dataContext.Users.Single(u => u.UserName == "tnt01user"));
            devTenant01.Users.Add(dataContext.Users.Single(u => u.UserName == "tnt01and02user"));
            devTenant02.Users.Add(dataContext.Users.Single(u => u.UserName == "tnt01and02user"));
            devTenant02.Users.Add(dataContext.Users.Single(u => u.UserName == "tnt02user"));
            fusionTenant.Users.Add(dataContext.Users.Single(u => u.UserName == "adhach"));

            dataContext.Users.Single(u => u.UserName == "tnt01user").Tenants.Add(devTenant01);

            dataContext.Dashboards.Single(d => d.Name == "tnt01user_Dashboard_1").Tenant = devTenant01;
            dataContext.Dashboards.Single(d => d.Name == "Test_tnt01user_ToUpdate").Tenant = devTenant01;
            dataContext.Dashboards.Single(d => d.Name == "tnt01user_Dashboard_2").Tenant = devTenant01;
            dataContext.Dashboards.Single(d => d.Name == "tnt02user_Dashboard_3").Tenant = devTenant02;
            dataContext.Dashboards.Single(d => d.Name == "tnt01and02user_Dashboard_4").Tenant = devTenant01;
            dataContext.Dashboards.Single(d => d.Name == "tnt01and02user_Dashboard_5").Tenant = devTenant02;
            dataContext.Dashboards.Single(d => d.Name == "Test_tnt01user_ToDelete").Tenant = devTenant01;

            dataContext.DashboardOptions.Single(opt => opt.Options == "DevTenant01_Options").Tenant = devTenant01;
            dataContext.DashboardOptions.Single(opt => opt.Options == "DevTenant02_Options").Tenant = devTenant02;

            foreach (var potl in dataContext.ProductOfferingTenantLocations)
            {
                dataContext.Locations.Single(x => x.Id == potl.LocationId).ProductOfferingTenantLocations.Add(potl);
                potl.Tenant = dataContext.Tenants.Single(x => x.Id == potl.TenantId);
            }

            foreach (var loc in dataContext.LocationTypes)
            {
                var children = dataContext.LocationTypes.Where(x => x.ParentId == loc.Id).ToList();
                children.ForEach(x => loc.LocationTypes.Add(x));
            }
            foreach (var locLog in dataContext.LocationLogEntries)
            {
                locLog.Location = dataContext.Locations.SingleOrDefault(x => x.Id == locLog.LocationId);
            }
        }

        public static List<Tenant> GetTenants()
        {
            return new List<Tenant>()
            {
                new Tenant
                {
                    Id = Guid.Parse("e56a4d05-a3b4-e511-82c1-000c29b494b6"),
                    Name = "Hach Fusion"
                },
                new Tenant
                {
                    Id = Guid.Parse("e66a4d05-a3b4-e511-82c1-000c29b494b6"),
                    Name = "IOS Developers"
                },
                new Tenant
                {
                    Id = Guid.Parse("494c1c3d-1026-4336-bd0b-23355785fab1"),
                    Name = "Dev Tenant 01"
                },
                new Tenant
                {
                    Id = Guid.Parse("8945b062-7333-456d-8cb4-e12911f89915"),
                    Name = "Dev Tenant 02"
                }
            };
        }

        public static List<User> GetUsers()
        {
            return new List<User>()
            {
                new User
                {
                    Id = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    UserName = "adhach",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                },

                new User
                {
                    Id = Guid.Parse("d24c45f1-3641-43af-8991-4c8049a587fc"),
                    UserName = "ffhach",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                },

                new User
                {
                    Id = Guid.Parse("57739dd8-0b58-49d5-9a6b-aef8b912e07e"),
                    UserName = "fahach",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                },

                new User
                {
                    Id = Guid.Parse("501286bf-ce04-417a-ae69-f9c5c0160c83"),
                    UserName = "tnt01user",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                },

                new User
                {
                    Id = Guid.Parse("5f3ed610-a28e-4ce7-9e9c-88f37c8a9f7c"),
                    UserName = "tnt02user",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                },

                new User
                {
                    Id = Guid.Parse("80fc46cc-456e-4450-a98c-838cc7fd643a"),
                    UserName = "tnt01and02user",
                    CreatedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    CreatedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00"),
                    ModifiedById = Guid.Parse("85c04bda-4416-44bf-8654-868ba9f5dd3a"),
                    ModifiedOn = DateTime.Parse("9/19/2016 10:27:14 PM +00:00")
                }
            };
        }

        public static List<DashboardOption> GetDashboardOptions()
        {
            var tenantList = GetTenants();
            return new List<DashboardOption>() {

                new DashboardOption
                {
                    Id = Guid.Parse("00061198-A605-4200-A16B-71E311DCE771"),
                    TenantId = tenantList.Single(x => x.Name == "Dev Tenant 01").Id,
                    Options = "DevTenant01_Options"
                },

                new DashboardOption
                {
                    Id = Guid.Parse("1EC0EB11-7A4D-4B7A-B39F-115C8641380A"),
                    TenantId = tenantList.Single(x=> x.Name == "Dev Tenant 02").Id,
                    Options = "DevTenant02_Options"
                }
            };
        }

        public static List<Dashboard> GetDashboards()
        {
            var userList = GetUsers();
            var tenantList = GetTenants();
            var dashboardOptionsList = GetDashboardOptions();

            return new List<Dashboard>()
            {
                 new Dashboard
                {
                    Id = Guid.Parse("0BA83E70-5CC9-4066-A15D-7FDE3F67E9CB"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01user").Id,
                    Name = "tnt01user_Dashboard_1",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 01").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant01_Options").Id,
                    Layout = "tnt01user_Dashboard_1",
                    IsPrivate = false
                },

                new Dashboard
                {
                    Id = Guid.Parse("B0F42ED5-A045-4CF9-A66D-3C7B2878A320"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01user").Id,
                    Name = "tnt01user_Dashboard_2",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 01").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant01_Options").Id,
                    Layout = "tnt01user_Dashboard_2",
                    IsPrivate = false
                },

                new Dashboard
                {
                    Id = Guid.Parse("6F30D93F-CD5E-4BDF-AFF8-4CF8F58200B8"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt02user").Id,
                    Name = "tnt02user_Dashboard_3",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 02").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant02_Options").Id,
                    Layout = "tnt02user_Dashboard_3",
                    IsPrivate = false
                },

                new Dashboard
                {
                    Id = Guid.Parse("AE10105C-4863-4E26-9878-4AC7CDE56836"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01and02user").Id,
                    Name = "tnt01and02user_Dashboard_4",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 01").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant01_Options").Id,
                    Layout = "tnt01and02user_Dashboard_4",
                    IsPrivate = false
                },

                new Dashboard
                {
                    Id = Guid.Parse("CDDDFFB1-C4F4-47C3-AFA3-F873EFD759F1"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01and02user").Id,
                    Name = "tnt01and02user_Dashboard_5",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 02").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant02_Options").Id,
                    Layout = "tnt01and02user_Dashboard_5",
                    IsPrivate = false
                },

                new Dashboard
                {
                    Id = Guid.Parse("9238C138-F77A-4AF4-8033-AFD30CBF7C0D"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01user").Id,
                    Name = "Test_tnt01user_ToDelete",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 01").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant01_Options").Id,
                    Layout = "Test_tnt01user_ToDelete",
                    IsPrivate = false
                },
                new Dashboard
                {
                    Id = Guid.Parse("0635527F-B47D-4ADC-B1D7-E2FDD0138CC2"),
                    OwnerUserId = userList.Single(u=> u.UserName =="tnt01user").Id,
                    Name = "Test_tnt01user_ToUpdate",
                    TenantId = tenantList.Single(t => t.Name == "Dev Tenant 01").Id,
                    DashboardOptionId = dashboardOptionsList.Single(opt => opt.Options == "DevTenant01_Options").Id,
                    Layout = "Test_tnt01user_ToUpdate",
                    IsPrivate = false
                }
            };
        }

        public static List<ChemicalFormType> GetChemicalFormTypes()
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

        public static List<InAppMessage> GetInAppMessages()
        {
            var usersList = GetUsers();
            var messageTypes = GetMessageTypes();

            return new List<InAppMessage>
            {
                new InAppMessage
                {
                    Id = new Guid("4CC29478-EC01-4EF5-B076-637508E241AF"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 11",
                    IsRead = false,
                    DateRead = null,
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt01user").Id,
                    DateReceived = null,
                    DateTimeSent = DateTime.Now.AddSeconds(-42),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD")
                },


                new InAppMessage
                {
                    Id = new Guid("4CC29478-EC01-4EF5-B076-6375081231AF"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 11.5",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-5),
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt01user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-10),
                    DateTimeSent = DateTime.Now.AddSeconds(-42),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD")
                },

                new InAppMessage
                {
                    Id = new Guid("4CC21118-EC01-4EF5-B076-637508E241AF"),
                    Subject = "OnBoarding - New User",
                    Body = "You have been on boarded. High five!",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-5),
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt01user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-10),
                    DateTimeSent = DateTime.Now.AddSeconds(-42),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD")
                },

                new InAppMessage
                {
                    Id = new Guid("54329478-EC01-4EF5-B076-637508E241AF"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 2.5",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-10),
                    IsTrash = true,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt01user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-48),
                    DateTimeSent = DateTime.Now.AddSeconds(-50),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD")
                },

                new InAppMessage
                {
                    Id = new Guid("4CC29478-EC01-4EF5-B076-637764E241AF"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 3.5",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-10),
                    IsTrash = true,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt02user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-48),
                    DateTimeSent = DateTime.Now.AddSeconds(-50),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD"),
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddMinutes(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddMinutes(-1)
                },

                new InAppMessage
                {
                    Id = new Guid("4CC29478-EC01-4EF5-B076-637508E2123A"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 3.5",
                    IsRead = false,
                    DateRead = null,
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt02user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-48),
                    DateTimeSent = DateTime.Now.AddSeconds(-50),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD"),
                },

                new InAppMessage
                {
                    Id = new Guid("4CC29478-EC01-4EF5-B076-637BBBE241AF"),
                    Subject = "pH Limit Violation",
                    Body = "pH measured at 11.5",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-5),
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt01user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-10),
                    DateTimeSent = DateTime.Now.AddSeconds(-42),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD"),
                },

                new InAppMessage
                {
                    Id = new Guid("4CB41418-EC01-4EF5-B076-637508E241AF"),
                    Subject = "OnBoarding - New User",
                    Body = $"You have been Onboarded {usersList.Single(u => u.UserName=="tnt02user").UserName}. High five!",
                    IsRead = true,
                    DateRead = DateTime.Now.AddSeconds(-5),
                    IsTrash = false,
                    MessageTypeId =  messageTypes.Single( t => t.Name =="LimitViolation").Id,
                    UserId = usersList.Single( u => u.UserName =="tnt02user").Id,
                    DateReceived = DateTime.Now.AddSeconds(-10),
                    DateTimeSent = DateTime.Now.AddSeconds(-42),
                    SenderId = new Guid("7601BA47-8CAB-47DA-9BC5-65E759AC91CD"),
                }
            };
        }

        public static List<MessageType> GetMessageTypes()
        {
            var usersList = GetUsers();

            return new List<MessageType> {
                new MessageType
                {
                    Id = new Guid("94372e8d-e75f-40d7-9d50-b0474ea52274"),
                    Name = "LimitViolation",
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddDays(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddDays(-1)
                },

                new MessageType
                {
                    Id = new Guid("566f0e0e-d362-4b4a-a410-1bb5cf523c16"),
                    Name = "OnBoarding",
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddDays(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddDays(-1)
                },

                new MessageType
                {
                    Id = new Guid("b396aa33-e91f-4946-9fb4-305747df30f1"),
                    Name = "RequestTenantAccess",
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddDays(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddDays(-1)
                },

                new MessageType
                {
                    Id = new Guid("de9ff28d-1932-4068-be40-851cfc965c14"),
                    Name = "TenantAccessGranted",
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddDays(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddDays(-1)
                },

                new MessageType
                {
                    Id = new Guid("EA4EEF63-CF06-459C-81C0-88ED83895EFD"),
                    Name = "DenyTenantAccess",
                    CreatedById = usersList.Single( u => u.UserName =="adhach").Id,
                    CreatedOn = DateTime.Now.AddDays(-1),
                    ModifiedById = usersList.Single( u => u.UserName =="adhach").Id,
                    ModifiedOn = DateTime.Now.AddDays(-1)
                }
            };
        }

        public static List<LimitType> GetLimitTypes()
        {

            return new List<LimitType> {
                new LimitType
                {
                    Id = Guid.Parse("4ffac136-f02b-4a4b-b04c-e88601705a0a"),
                    I18NKeyName = "Under",
                    Severity = 2,
                    Polarity = 0
                },

                new LimitType
                {
                    Id = Guid.Parse("a447b70d-a7c0-420d-8570-ab5a4780e3c6"),
                    I18NKeyName = "Over",
                    Severity = 2,
                    Polarity = 1
                },

                new LimitType
                {
                    Id = Guid.Parse("b338510c-9116-47f7-b19b-0daeb35cc801"),
                    I18NKeyName = "Near Under",
                    Severity = 1,
                    Polarity = 0
                },

                new LimitType
                {
                    Id = Guid.Parse("bf25df8a-e257-435c-86a9-4686a305885d"),
                    I18NKeyName = "Near Over",
                    Severity = 1,
                    Polarity = 1
                },

                new LimitType
                {
                    Id = Guid.Parse("B8EB8D5E-D3F4-42D7-A352-65002DEFB7FA"),
                    I18NKeyName = "ToDelete",
                    Severity = 2,
                    Polarity = 2
                }
            };
        }

        public static List<Parameter> GetParameters()
        {
            var parameterTypesList = GetParameterTypes();
            var unitTypesList = GetUnitTypes();
            var unitTypeGroups = GetUnitTypeGroups();

            return new List<Parameter> {
                new Parameter
                {
                    Id = Guid.Parse("2f2e360b-52d6-4819-9aec-e8af1314f8e8"),
                    I18NKeyName = "pH",
                    ParameterTypeId = parameterTypesList.Single(x => x.I18NKeyName == "Sensed").Id,
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "pH").Id,
                    BaseUnitTypeId = unitTypesList.Single( x=> x.I18NKeyName == "pH").Id,
                    BaseChemicalFormTypeId = null
                },

                new Parameter
                {
                    Id = Guid.Parse("463993ce-e08b-471f-abb5-f0ebeff42c68"),
                    I18NKeyName = "Flow",
                    ParameterTypeId = parameterTypesList.Single(x => x.I18NKeyName == "Sensed").Id,
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "VolumeTime").Id,
                    BaseUnitTypeId = unitTypesList.Single( x=> x.I18NKeyName == "Gallons / Minute").Id,
                    BaseChemicalFormTypeId = null
                }
            };
        }

        public static List<UnitType> GetUnitTypes()
        {
            var unitTypeGroups = GetUnitTypeGroups();

            return new List<UnitType> {
                new UnitType
                {
                    Id = Guid.Parse("856eb670-33bd-4440-b5dd-041c6412ecc1"),
                    I18NKeyAbbreviation = "gpg",
                    I18NKeyName = "Grains / Gallon",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("ccdfd6dd-8253-43f0-b8d2-0731d511125c"),
                    I18NKeyAbbreviation = "mm",
                    I18NKeyName = "Millimeters",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Length").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("7aa60069-12e5-4e2f-8649-0be021786e3a"),
                    I18NKeyAbbreviation = "ppm",
                    I18NKeyName = "Parts / Million",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("2f26769f-aa31-4eba-a9da-159d89e65051"),
                    I18NKeyAbbreviation = "mg/L",
                    I18NKeyName = "Milligrams / Liter",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("93c3aa6a-73a9-45e6-9c98-1a3a22f5cdfa"),
                    I18NKeyAbbreviation = "mS/cm",
                    I18NKeyName = "Millisiemens / Centimeter",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("86d24fa1-f7d6-46a2-a10a-1c4adbc31edb"),
                    I18NKeyAbbreviation = "°C",
                    I18NKeyName = "Centigrade",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Temp").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("af893f5b-f054-438b-a6ed-3cb415835b69"),
                    I18NKeyAbbreviation = "mgd",
                    I18NKeyName = "Million Gallons / Day",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "VolumeTime").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("5c9393ff-de3b-49c3-ad9c-4747e1e10702"),
                    I18NKeyAbbreviation = "in",
                    I18NKeyName = "Inches",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Length").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b049faf9-0ed2-4d67-8814-4de6a681bb02"),
                    I18NKeyAbbreviation = "mBar",
                    I18NKeyName = "Millibar",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b53a0405-c518-4698-8267-664b6f1211c4"),
                    I18NKeyAbbreviation = "µg/L",
                    I18NKeyName = "Micrograms / Liter",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("2ab51fed-21d2-4623-a63b-6ccecf4bbf52"),
                    I18NKeyAbbreviation = "% Sat",
                    I18NKeyName = "Percent Saturation",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b9030954-478f-424a-a990-84e1520ee6a6"),
                    I18NKeyAbbreviation = "°dH",
                    I18NKeyName = "German Degrees",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("7de0fafa-735e-4ff8-bcc6-86441977330b"),
                    I18NKeyAbbreviation = "inHg",
                    I18NKeyName = "Inches of Mercury",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("7cba0ab3-9e4f-44ef-8301-871b417e8fc8"),
                    I18NKeyAbbreviation = "ppb",
                    I18NKeyName = "Parts / Billion",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("536a6244-1c74-4abb-9574-92df19099bb9"),
                    I18NKeyAbbreviation = "hPa",
                    I18NKeyName = "Hectopascal",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b16b0755-e766-43f8-b806-a39265432109"),
                    I18NKeyAbbreviation = "°F",
                    I18NKeyName = "Fahrenheit",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Temp").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("0c9210e4-80e2-4d5d-9955-af7cc152e09f"),
                    I18NKeyAbbreviation = "°e",
                    I18NKeyName = "English Degrees",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("96572e2e-9060-4ba5-83ed-b53df47bb5c5"),
                    I18NKeyAbbreviation = "°fH",
                    I18NKeyName = "French Degrees",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b4406f52-833a-40b7-b71a-bd1f14ba56ae"),
                    I18NKeyAbbreviation = "µS/cm",
                    I18NKeyName = "Microsiemens / Centimeter",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("97fb564a-e9ba-488b-b392-be2dc6f5d199"),
                    I18NKeyAbbreviation = "mV",
                    I18NKeyName = "Millivolts",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("e58fc917-995d-494a-9eeb-cdbe518539e8"),
                    I18NKeyAbbreviation = "%",
                    I18NKeyName = "Percent",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("486b8123-ae59-4556-8abd-e4d5a0f88bd5"),
                    I18NKeyAbbreviation = "pH",
                    I18NKeyName = "pH",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("c47970d9-1eb5-44e0-828b-f48e6d674c26"),
                    I18NKeyAbbreviation = "gpm",
                    I18NKeyName = "Gallons / Minute",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "VolumeTime").Id
                },

                new UnitType
                {
                    Id = Guid.Parse("b5e1270c-7dcc-41b7-9f4c-fac6df415c63"),
                    I18NKeyAbbreviation = "mmHg",
                    I18NKeyName = "Millimeters Of Mercury",
                    UnitTypeGroupId = unitTypeGroups.Single(x => x.I18NKeyName == "Volume").Id
                }
            };
        }

        public static List<ParameterType> GetParameterTypes()
        {
            return new List<ParameterType>{
                new ParameterType
                {
                    Id = Guid.Parse("befffce4-6de6-4b00-8337-27af2ca48d25"),
                    I18NKeyName = "Sensed"
                },

                new ParameterType
                {
                    Id = Guid.Parse("2b071913-3928-49eb-a7a1-697dfc0668ff"),
                    I18NKeyName = "Chemical"
                }
            };
        }

        public static List<UnitTypeGroup> GetUnitTypeGroups()
        {
            return new List<UnitTypeGroup> {
                new UnitTypeGroup
                {
                    Id = Guid.Parse("11311567-778e-472d-a7b5-1c8cf9ae4c27"),
                    I18NKeyName = "Volume"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("2b215935-67eb-45b2-b5e9-58faaabc9032"),
                    I18NKeyName = "VolumeTime"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("3df8b9f3-9e49-4503-830f-6e769431282e"),
                    I18NKeyName = "AreaTime"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("034d1070-fb25-43d8-a7ee-788dd951a5e6"),
                    I18NKeyName = "Length"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("a816e876-dbed-4228-836e-85ea15d4ae7d"),
                    I18NKeyName = "Mass"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("336c8dd8-0e22-4757-942c-91d0f48a2166"),
                    I18NKeyName = "MassTime"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("2bf5516b-6e77-40a5-9af2-ce0d03048fbb"),
                    I18NKeyName = "Area"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("231a9c86-33b3-476f-9ab0-d5f8852c98ec"),
                    I18NKeyName = "LengthTime"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("203b1f93-4ace-4962-8b4b-e8828bf5a1b2"),
                    I18NKeyName = "Time"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("e28975a3-c453-4417-86d2-ec350d1a82f5"),
                    I18NKeyName = "pH"
                },

                new UnitTypeGroup
                {
                    Id = Guid.Parse("fc4db0da-d148-4acc-9422-fa0d14924971"),
                    I18NKeyName = "Temp"
                }
            };
        }

        public static List<ParameterValidRange> GetParameterValidRanges()
        {
            var parametersList = GetParameters();

            return new List<ParameterValidRange> {
                new ParameterValidRange
                {
                    Id = Guid.Parse("DD1BB35B-1585-4D6D-B21E-13F06B6A25BF"),
                    ParameterId = parametersList.Single(p=> p.I18NKeyName=="pH").Id,
                    MinValue = 0,
                    MaxValue = 14
                },

                new ParameterValidRange
                {
                    Id = Guid.Parse("4871F673-5308-4DF9-BE96-2CC34AD856E5"),
                    ParameterId = parametersList.Single(p=> p.I18NKeyName=="Flow").Id,
                    MinValue = 0,
                    MaxValue = 1000
                },

                new ParameterValidRange
                {
                    Id = Guid.Parse("D138C2F1-9EEB-4F8C-829A-04D8B9598049"),
                    ParameterId = parametersList.Single(p=> p.I18NKeyName=="Flow").Id,
                    MinValue = 0
                },

                new ParameterValidRange
                {
                    Id = Guid.Parse("150CC8CF-49AC-4BC5-A348-9D9FB29BA5DB"),
                    ParameterId = parametersList.Single(p=> p.I18NKeyName=="Flow").Id,
                    MaxValue = 1000
                }
            };
        }

        public static List<Location> GetLocations()
        {
            var locationTypesList = GetLocationTypes();

            return new List<Location>
            {
                new Location
                {
                    Id = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"),
                    Name = "Operation_01",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Operation").Id,
                    ParentId = null,
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("C5A3A070-1394-4734-B114-24AE9073BCF2"),
                    Name = "Operation_02",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Operation").Id,
                    ParentId = null,
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("6508A6ED-DE7C-4E26-849B-72F44AF4931A"),
                    Name = "Operation_03",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Operation").Id,
                    ParentId = null,
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"),
                    Name = "Process_Preliminary",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Process").Id,
                    ParentId = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"), // Operation_01.Id,
                    SortOrder = 1

                },

                new Location
                {
                    Id = Guid.Parse("e9562dff-5204-4aff-a413-d1212327dea8"),
                    Name = "Process_Influent",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Process").Id,
                    ParentId = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"), // Operation_01.Id,
                    SortOrder = null

                },

                new Location
                {
                    Id = Guid.Parse("eef791a6-01f0-4274-9bf1-98c27fbeb2b2"),
                    Name = "Process_PrimaryTreatment",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Process").Id,
                    ParentId = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"), // Operation_01.Id,
                    SortOrder = null

                },

                new Location
                {
                    Id = Guid.Parse("b317b8ab-d1b1-4c09-9b36-014fc024879f"),
                    Name = "Process_SecondaryTreatment",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Process").Id,
                    ParentId = Guid.Parse("B73C185D-667F-4636-A245-AB7B8EAA9BDA"), // Operation_01.Id,
                    SortOrder = null

                },

                new Location
                {
                    Id = Guid.Parse("6dfdc82f-ff38-468c-84cc-1855200cfbfe"),
                    Name = "SamplingSite_Grit",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"), // Process_Preliminary.Id,
                    SortOrder = 2
                },

                new Location
                {
                    Id = Guid.Parse("49935b82-f8df-4251-9144-d499f5ad5c79"),
                    Name = "SamplingSite_Screenings",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"), // Process_Preliminary.Id
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("cd6bd0f3-ac99-4a8e-94cc-324aebeea696"),
                    Name = "SamplingSite_Chemical",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("5E510C45-7BBE-47C6-80D1-F86D79F418E6"), // Process_Preliminary.Id
                    SortOrder = null
                },


                new Location
                {
                    Id = Guid.Parse("dcd6c19f-ee8b-4a08-ae1c-02214570c8f4"),
                    Name = "SamplingSite_Influent_InfluentCombined",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId =  Guid.Parse("e9562dff-5204-4aff-a413-d1212327dea8"),// Process_Influent.Id,
                    SortOrder = null
                },


                new Location
                {
                    Id = Guid.Parse("1d54b1a3-803b-46be-a870-772fb8af4e06"),
                    Name = "SamplingSite_Influent_HauledWasted",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("e9562dff-5204-4aff-a413-d1212327dea8"),// Process_Influent.Id
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("d1106178-2f92-447f-8853-d41c707e4cf4"),
                    Name = "SamplingSite_Influent_Recycled",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("e9562dff-5204-4aff-a413-d1212327dea8"),// Process_Influent.Id,
                    SortOrder = null
                },
                new Location
                {
                    Id = Guid.Parse("96D7F2A4-760F-4217-B175-1D43EA9DB79C"),
                    Name = "Loveland Distribution",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Distribution").Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {

                    Id = Guid.Parse("7AE0D3ED-D58E-436D-93FF-CE9C59F8702C"),
                    Name = "29th Street",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("96D7F2A4-760F-4217-B175-1D43EA9DB79C"), // Location_1_ParentWithDescendants.Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("35773DC0-0333-47C4-9793-16F96C9C89F7"),
                    Name = "27th Street",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("7AE0D3ED-D58E-436D-93FF-CE9C59F8702C"), // Location_1_1_ChildWithChildren.Id
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("44652DAD-B130-4FEF-A34F-10DD318FB205"),
                    Name = "28th Street",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    ParentId = Guid.Parse("96D7F2A4-760F-4217-B175-1D43EA9DB79C"), // Location_1_ParentWithDescendants.Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("6585DEEC-2EA3-4CCC-82E7-C94EFB49D979"),
                    Name = "FC Distribution",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Distribution").Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },


                new Location
                {
                    Id = Guid.Parse("19DF95AE-1443-4258-A23E-636EB8BC3678"),
                    Name = "28th Street",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Sampling Site").Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("3673FAA9-81E8-46F3-BEDB-1B0B2C011AB8"),
                    Name = "FC Distribution",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Distribution").Id,
                    //Geography = DbGeography.PointFromText("POINT(-121. 44.)", 4326),
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("e2cc3b11-a951-43a5-83ad-539b09584614"),
                    Name = "Location To Delete",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Operation").Id,
                    SortOrder = null
                },

                new Location
                {
                    Id = Guid.Parse("2b3faf82-988a-4dcf-9b4b-2cba3a3c8a58"),
                    Name = "Location Updateable",
                    LocationTypeId = locationTypesList.Single(t => t.I18NKeyName == "Operation").Id,
                    SortOrder = null,
                }

            };
        }

        public static List<LocationType> GetLocationTypes()
        {

            return new List<LocationType>
            {
                new LocationType
                {
                    Id = Guid.Parse("66b8a397-96ae-4c6b-b1f1-9f28a823a8e6"),
                    I18NKeyName = "Operation"
                },

                new LocationType
                {
                    Id = Guid.Parse("c40b1d39-004c-4684-a62f-18fcecd2a49c"),
                    I18NKeyName = "Process"
                },

                new LocationType
                {
                    Id = Guid.Parse("E43D28F7-E0D7-439B-AF11-7A56E384621D"),
                    I18NKeyName = "Sampling Site"
                },

                new LocationType
                {
                    Id = Guid.Parse("1ecb1b32-599d-4a7b-93b5-f2aa302d7635"),
                    I18NKeyName = "Distribution"
                },

                new LocationType
                {
                    Id = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC"),
                    I18NKeyName = "FortCollinsOperation"
                },

                new LocationType
                {
                    Id = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE"),
                    I18NKeyName = "FortCollinsSystemA",
                    ParentId = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC") //FortCollinsOperation.Id
                },

                new LocationType
                {
                    Id = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90"),
                    I18NKeyName = "FortCollinsSystemB",
                    ParentId = Guid.Parse("03CEA526-C2D4-4DAF-B5E9-DC21427A45EC") //FortCollinsOperation.Id
                },

                new LocationType
                {
                    Id = Guid.Parse("F3FE8D0F-B87D-4F40-B234-D15A4BB8E71D"),
                    I18NKeyName = "FortCollinsCollectorA1",
                    ParentId = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE") // FortCollinsSystemA.Id
                },

                new LocationType
                {
                    Id = Guid.Parse("88F848F9-E5D4-4F53-B8E3-BA3E4419468F"),
                    I18NKeyName = "FortCollinsCollectorA2",
                    ParentId = Guid.Parse("5A9000D2-1D69-4B1C-86DB-C915677286AE") // FortCollinsSystemA.Id
                },

                new LocationType
                {
                    Id = Guid.Parse("7720B622-D561-4D61-9FA5-F7B9B1BB8ED7"),
                    I18NKeyName = "FortCollinsCollectorB1",
                    ParentId = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90") // FortCollinsSystemB.Id
                },

                new LocationType
                {
                    Id = Guid.Parse("E5EB487C-D427-47EF-8651-9E56F658EA50"),
                    I18NKeyName = "FortCollinsCollectorB2",
                    ParentId = Guid.Parse("202B4B72-C811-41BC-B66F-D91B84E84A90") // FortCollinsSystemB.Id
                }
            };
        }

        public static List<ProductOfferingTenantLocation> GetPOTLs()
        {
            var productOfferingsList = GetProductOfferings();
            var locationsList = GetLocations();
            var tenantsList = GetTenants();

            return new List<ProductOfferingTenantLocation>
            {
                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 01").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Operation_01").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 01").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Operation_02").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 02").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Operation_03").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 02").Id,
                    LocationId = locationsList.Single(x=> x.Name =="SamplingSite_Influent_InfluentCombined").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Hach Fusion").Id,
                    LocationId = locationsList.Single(x=> x.Name =="SamplingSite_Influent_InfluentCombined").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Hach Fusion").Id,
                    LocationId = locationsList.Single(x=> x.Name =="SamplingSite_Influent_Recycled").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 02").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Process_Preliminary").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 02").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Process_Influent").Id
                },

                new ProductOfferingTenantLocation
                {
                    ProductOfferingId = productOfferingsList.Single(x=> x.Name == "Fusion Foundation").Id,
                    TenantId = tenantsList.Single(x=> x.Name =="Dev Tenant 01").Id,
                    LocationId = locationsList.Single(x=> x.Name =="Location Updateable").Id
                }
            };
        }

        public static List<ProductOffering> GetProductOfferings()
        {
            return new List<ProductOffering> {
                new ProductOffering
                {
                    Id = Guid.Parse("e36a4d05-a3b4-e511-82c1-000c29b494b6"),
                    Name = "Fusion Foundation"
                },

                new ProductOffering
                {
                    Id = Guid.Parse("e46a4d05-a3b4-e511-82c1-000c29b494b6"),
                    Name = "PPA Fusion"
                },

                new ProductOffering
                {
                    Id = Guid.Parse("e4c3c1ef-6e6e-4725-a6e2-3224507a0739"),
                    Name = "Mobile Sensor Management"
                }
            };
        }

        public static List<LocationLogEntry> GetLocationLogEntries()
        {
            var locationsList = GetLocations();

            return new List<LocationLogEntry> { 
            //public static LocationLogEntry Operation1Log1 =>
            new LocationLogEntry
            {
                Id = Guid.Parse("44E0C497-3A2C-4A89-99BD-F7B14C1A9187"),
                LocationId = locationsList.Single(x=> x.Name =="Operation_01").Id,
                LogEntry = "Operation 1, Log 1",
                TimeStamp = DateTimeOffset.Parse("10/01/2016 8:00:00")
            },

            //public static LocationLogEntry Operation1Log2 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("C7267A62-A2F1-4C2A-8F36-2BEABD9B0F66"),
                    LocationId = locationsList.Single(x=> x.Name =="Operation_01").Id,
                    LogEntry = "Operation 1, Log 2",
                    TimeStamp = DateTimeOffset.Parse("10/01/2016 8:05:00")
                },

            //public static LocationLogEntry Operation2Log1 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("83FFABC0-D51B-4B82-8DCC-80D1F258DB2F"),
                    LocationId = locationsList.Single(x=> x.Name =="Operation_02").Id,
                    LogEntry = "Operation 2, Log 1",
                    TimeStamp = DateTimeOffset.Parse("10/01/2016 8:10:00")
                },

            //public static LocationLogEntry Operation2Log2_NoLocation =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("53B3C397-54F0-4915-B02E-6301A0864A49"),
                    LocationId = Guid.Empty,
                    LogEntry = "Operation 2, Log 2",
                    TimeStamp = DateTimeOffset.Parse("10/01/2016 8:15:00")
                },

            //public static LocationLogEntry Operation2Log3_LocationNoExist =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("BBA19BAB-0E0F-4FC5-B5CF-4929F6870FF4"),
                    LocationId = Guid.NewGuid(),
                    LogEntry = "Operation 2, Log 3",
                    TimeStamp = DateTimeOffset.Parse("10/01/2016 8:20:00")
                },

            //public static LocationLogEntry Operation3Log1 =>
                new LocationLogEntry
                {
                    Id = Guid.Parse("EA7E094D-B5F7-4E59-B642-7FDC08DF58FC"),
                    LocationId = locationsList.Single(x=> x.Name =="Process_PrimaryTreatment").Id,
                    LogEntry = "Operation 3, Log 1",
                    TimeStamp = DateTimeOffset.Parse("10/01/2016 8:25:00")
                }
            };
        }
    }
}
