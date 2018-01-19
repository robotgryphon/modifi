using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RobotGryphon.Modifi.Utilities {
    public abstract class FileUtilities {

        public static bool ChecksumMatches(string filePath, string checksum) {
            if (!File.Exists(filePath)) return false;

            using (var md5 = MD5.Create()) {
                using (var fileStream = File.OpenRead(filePath)) {
                    byte[] hash = md5.ComputeHash(fileStream);
                    string hashString = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();

                    if (hashString == checksum) return true;
                }
            }

            return false;
        }
    }
}
