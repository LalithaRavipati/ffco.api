using System.Linq;
using Hach.Fusion.Data.Entities;
using System.Collections.Generic;
using System;

namespace Hach.Fusion.FFCO.Business.Tests
{
    public static class Seeder
    {
        public static List<Tenant> GetCommonTenantSeedData()
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

        public static List<User> GetCommonUserSeedData()
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

        public static List<DashboardOption> GetCommonDashboardOptions()
        {
            return new List<DashboardOption>() {
                new DashboardOption
                {
                    Id = Guid.Parse("7DDD52F4-A21D-4B99-A87F-A837DA940D40"),
                    Options = "HachFusion_Options"
                },

                new DashboardOption
                {
                    Id = Guid.Parse("00061198-A605-4200-A16B-71E311DCE771"),
                    Options = "DevTenant01_Options"
                },

                new DashboardOption
                {
                    Id = Guid.Parse("1EC0EB11-7A4D-4B7A-B39F-115C8641380A"),
                    Options = "DevTenant02_Options"
                }
            };
        }
    }
}
