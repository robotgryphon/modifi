using System;
using System.IO;
using System.Runtime.Loader;
using System.Reflection;
using System.Linq;
using Modifi;
using Modifi.Packs;

namespace Modifi.Domains {
    public class DomainHelper {

        /// <summary>
        /// Loads a domain and adds it to a pack's loaded domain list.        /// </summary>
        /// <param name="pack"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static IDomain LoadDomain(Pack pack, string domain) {

            if (pack.Domains.ContainsKey(domain)) return pack.Domains[domain];

            // Perform domain search
            bool domainFound = false;
            string domainPath = null;

            if (Modifi.DomainSearchPaths == null)
                Modifi.LoadSearchPaths();

            foreach (string path in Modifi.DomainSearchPaths) {
                string pathCheck = Path.Combine(path, domain + ".dll");

                Modifi.DefaultLogger.Debug("Trying to load domain handler from {0}...", pathCheck);
                if(File.Exists(pathCheck)) {
                    domainPath = pathCheck;
                    domainFound = true;
                    break;
                }
            }

            if (!domainFound)
                throw new DllNotFoundException("Cannot find the domain DLL.");

            Assembly cfAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(domainPath);
            Type controller = cfAssembly.ExportedTypes.First(t => t.GetInterfaces().Contains(typeof(IDomain)));

            if(controller == null) {
                throw new Exception("Cannot find the domain handler inside domain assembly.");
            }

            try {
                IDomain domainInstance = Activator.CreateInstance(controller) as IDomain;
                pack.Domains.Add(domain, domainInstance);
                return domainInstance;
            }

            catch(Exception e) {
                Modifi.DefaultLogger.Error("Error loading domain handler from {0}:", domainPath);
                Modifi.DefaultLogger.Error(e.Message);
                return null;
            }
            
        }
    }
}
