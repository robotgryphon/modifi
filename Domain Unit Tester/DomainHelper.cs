using Modifi.Domains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Modifi.Tests.DomainTester {

    class DomainHelper {

        public static IEnumerable<string> GetSearchPaths() {
            return new List<string>();
        }

        /// <summary>
        /// Loads a domain and adds it to a pack's loaded domain list.        /// </summary>
        /// <param name="pack"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static IDomain LoadDomain(IEnumerable<string> searchPaths, string domain) {

            // Perform domain search
            bool domainFound = false;
            string domainPath = null;

            foreach (string path in searchPaths) {
                string pathCheck = Path.Combine(path, domain + ".dll");

                Console.WriteLine("Trying to load domain handler from {0}...", pathCheck);
                if (File.Exists(pathCheck)) {
                    domainPath = pathCheck;
                    domainFound = true;
                    break;
                }
            }

            if (!domainFound)
                throw new DllNotFoundException("Cannot find the domain DLL.");

            Assembly cfAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(domainPath);
            Type controller = cfAssembly.ExportedTypes.First(t => t.GetInterfaces().Contains(typeof(IDomain)));

            if (controller == null) {
                throw new Exception("Cannot find the domain handler inside domain assembly.");
            }

            IDomain domainInstance = Activator.CreateInstance(controller) as IDomain;
            return domainInstance;
        }
    }
}