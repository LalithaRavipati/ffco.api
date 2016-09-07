using System.Configuration;
using Autofac;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;

namespace Hach.Fusion.FFCO.Api.DependencyInjection
{
    /// <summary>
    /// Service module that provides for dependency injection.
    /// </summary>
    public class ServiceModule : Module
    {
        /// <summary>
        /// Registers types for dependency injection.
        /// </summary>
        /// <param name="builder">Container used to hold dependency injection registration information.</param>
        protected override void Load(ContainerBuilder builder)
        {
            // Contexts

            // OData Helper
            builder.RegisterType<ODataHelper>().As<IODataHelper>().InstancePerLifetimeScope();

            // Claims Transformation
            builder.RegisterType<ClaimsTransformationMiddleware>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<ClaimsTransformer>().AsSelf().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}