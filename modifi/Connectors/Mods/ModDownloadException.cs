using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Mods {
    public class ModDownloadException : Exception {

        public ModDownloadException(string message) : base(message) { }
    }
}
