[assembly: WebActivator.PreApplicationStartMethod(typeof(INET.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(INET.Web.App_Start.NinjectWebCommon), "Stop")]

namespace INET.Web.App_Start
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;    
    using INET.Services;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using System.Linq;
    using INET.Core;
    using INET.Data;
    using System.Security.Principal;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            try
            {
                DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
                DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
                bootstrapper.Initialize(CreateKernel);
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (Exception ee in e.LoaderExceptions) {
                    throw ee;
                }
            }
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            kernel.Bind<INETContext>().ToSelf().InRequestScope();
            kernel.Bind<IPrincipal>().ToMethod(context => HttpContext.Current.User).InRequestScope();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            foreach (Type type in GetAllSubclassesOf(typeof(BusinessService)))
                kernel.Bind(type).ToSelf().InRequestScope();
            
        }

        public static IEnumerable<Type> GetAllSubclassesOf(Type baseType)
        {
            return Assembly.GetAssembly(baseType).GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
        }
    }
}
