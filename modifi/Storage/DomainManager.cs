using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Modifi.Domains {

    public class DomainManager {

        protected List<string> SearchPaths;

        protected Dictionary<string, IDomain> LoadedDomains;

        public DomainManager(string name, bool addCurrentDirectory = false) {
            this.SearchPaths = new List<string>();
            this.LoadedDomains = new Dictionary<string, IDomain>();

            if(addCurrentDirectory) this.SearchPaths.Add(Environment.CurrentDirectory);
        }

        public bool AddPath(string path) {
            string realPath = new DirectoryInfo(path).FullName;
            if(!Directory.Exists(realPath)) 
                return false;

            if(SearchPaths.Contains(realPath))
                return false;

            SearchPaths.Add(realPath);
            return true;
        }

        public bool RemovePath(string path) {
            string realPath =new DirectoryInfo(path).FullName;
            return SearchPaths.Remove(realPath);
        }

        public IEnumerable<string> GetSearchPaths() {
            return SearchPaths;
        }

        /// <summary>
        /// Gets or loads a domain from the specified assembly.
        /// This searches through the added paths and tries to find an assembly
        /// by the given domain name.
        /// </summary>
        /// <param name="name">The domain to fetch or load.</param>
        /// <returns>The domain if it could be found/loaded, otherwise null.</returns>
        public IDomain GetDomain(string name) {
            string properName = name.ToLowerInvariant();
            if(this.LoadedDomains.ContainsKey(properName))
                return this.LoadedDomains[properName];

            // Load domain, then return it
            Task<IDomain> loadedDomain = LoadDomain(properName);
            if(loadedDomain.IsFaulted) 
                return null;

            return loadedDomain.Result;
        }

        /// <summary>
        /// Makes an attempt to load a domain into the domain manager.
        /// This handles the exceptions and wraps them inside a Task- check the return
        /// Task for faulting before using the return value.
        /// </summary>
        /// <param name="domain">The name of the domain to load.</param>
        /// <returns>A Task with either an IDomain or null, if the domain could not be loaded.</returns>
        public Task<IDomain> LoadDomain(string domain) {

            if (LoadedDomains.ContainsKey(domain)) 
                return Task.FromResult(LoadedDomains[domain]);

            // Perform domain search
            bool domainFound = false;
            string domainPath = null;

            if (SearchPaths == null || SearchPaths.Count == 0)
                return Task.FromException<IDomain>(new Exception("No search paths are defined; cannot load any domains!"));

            foreach (string path in SearchPaths) {
                string pathCheck = Path.Combine(path, domain + ".dll");
                if(File.Exists(pathCheck)) {
                    domainPath = pathCheck;
                    domainFound = true;
                    break;
                }
            }

            if (!domainFound)
                return Task.FromResult<IDomain>(null);

            Assembly assemblyLoaded = Assembly.LoadFrom(domainPath);
            Type controller = assemblyLoaded.ExportedTypes.First(t => t.GetInterfaces().Contains(typeof(IDomain)));

            if(controller == null) {
                return Task.FromException<IDomain>(new Exception("Cannot find the domain handler inside domain assembly."));
            }

            try {
                IDomain domainInstance = Activator.CreateInstance(controller) as IDomain;
                LoadedDomains.Add(domain, domainInstance);
                return Task.FromResult(domainInstance);
            }

            catch(Exception e) {
                return Task.FromException<IDomain>(e);
            }
            
        }
    }
}