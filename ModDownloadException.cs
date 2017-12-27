using System;
using System.Runtime.Serialization;

namespace RobotGryphon.ModCLI {
    [Serializable]
    internal class ModDownloadException : Exception {
        private Mod.ModDownloadResult errorType;

        public ModDownloadException() {
        }

        public ModDownloadException(Mod.ModDownloadResult error) {
            this.errorType = error;
        }

        public ModDownloadException(string message) : base(message) {
        }

        public ModDownloadException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ModDownloadException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}