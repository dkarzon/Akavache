using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace Akavache
{
    internal interface IWantsToRegisterStuff
    {
        void Register(IMutableDependencyResolver resolverToUse);
    }

    public static class DependencyResolverMixin
    {
        /// <summary>
        /// Initializes a ReactiveUI dependency resolver with classes that 
        /// Akavache uses internally.
        /// </summary>
        public static void InitializeAkavache(this IMutableDependencyResolver This)
        {
            var namespaces = new[] { 
                "Akavache",
                "Akavache.Mac",
                "Akavache.Deprecated",
                "Akavache.Mobile",
                "Akavache.Http",
                "Akavache.Sqlite3",
            };

            var fdr = typeof(DependencyResolverMixin);

            var assmName = new AssemblyName(
                fdr.AssemblyQualifiedName.Replace(fdr.FullName + ", ", ""));

            foreach (var ns in namespaces) 
            {
                var targetType = ns + ".Registrations";
                string fullName = targetType +", " + assmName.FullName.Replace(assmName.Name, ns);
                
#if WINDOWS_PHONE
                Assembly assemb = null;
                try
                {
                    assemb = Assembly.Load(ns);
                }
                catch { }
                if (assemb == null) continue;
                var registerTypeClass = assemb.GetType(targetType, false);
#else
                var registerTypeClass = Type.GetType(fullName, false);
#endif
                if (registerTypeClass == null) continue;

                var registerer = (IWantsToRegisterStuff)Activator.CreateInstance(registerTypeClass);
                registerer.Register(This);
            };
        }
    }
}
