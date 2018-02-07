using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Modifi.Utilities {
    public abstract class ModUtilities {

        public static string GetFileChecksum(string filePath) {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not load file for checksum, not found.");

            using (var md5 = MD5.Create()) {
                using (var fileStream = File.OpenRead(filePath)) {
                    byte[] hash = md5.ComputeHash(fileStream);
                    string hashString = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
                    return hashString;                    
                }
            }
        }
        public static bool ChecksumMatches(string filePath, string checksum) {
            if (!File.Exists(filePath)) return false;

            string hashString = GetFileChecksum(filePath);
            return hashString == checksum;
        }
    }
}
