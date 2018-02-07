using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Modifi.Mods {

    public struct ModDownloadDetails {
        public string Filename;
        public string Checksum;

        public string Encrypt() {

            MemoryStream stream = new MemoryStream();
            BinaryWriter b = new BinaryWriter(stream);

            b.Write(Filename);
            b.Write(Checksum);
            
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}
