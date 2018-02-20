using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Modifi.Mods {

    public struct ModDownloadDetails {
        /// <summary>
        /// The file the mod is saved under.
        /// Note this is relative to the base MODS directory, not the installation directory.
        /// </summary>
        public string Filename;
        
        public string Checksum;

        public string Encrypt() {

            MemoryStream stream = new MemoryStream();
            BinaryWriter b = new BinaryWriter(stream);

            b.Write(Filename);
            b.Write(Checksum);
            
            return Convert.ToBase64String(stream.ToArray());
        }

        public static ModDownloadDetails Decrypt(string entry) {
            byte[] actual = Convert.FromBase64String(entry);
            MemoryStream stream = new MemoryStream(actual);
            BinaryReader b = new BinaryReader(stream);

            ModDownloadDetails details = new ModDownloadDetails();
            details.Filename = b.ReadString();
            details.Checksum = b.ReadString();
            
            return details;
        }
    }
}
