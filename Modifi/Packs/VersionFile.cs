using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RobotGryphon.Modifi.Packs {
    public class VersionFile : IDisposable {

        protected LiteDB.LiteDatabase DatabaseFile;

        public VersionFile(string hash) {
            string versionFile = Path.Combine(Settings.ModifiDirectory, hash + ".db");
            this.DatabaseFile = new LiteDB.LiteDatabase(versionFile);
        }

        public void Dispose() {
            this.DatabaseFile.Dispose();
        }

        public bool CollectionExists(string collectionName) {
            return DatabaseFile.CollectionExists(collectionName);
        }

        public LiteDB.LiteCollection<T> FetchCollection<T>(string collectionName) {
            return DatabaseFile.GetCollection<T>(collectionName);
        }
    }
}
