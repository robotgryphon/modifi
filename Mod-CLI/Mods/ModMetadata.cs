using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Mods {
    public struct ModMetadata {

        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A unique ID for this file. May be generated.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// A list of supported versions.
        /// </summary>
        public string[] Versions { get; set; }

        /// <summary>
        /// A path to the file on its server.
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// Where the mod is hosted. Typically this will be "curseforge".
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        public string ModId { get; set; }

        /// <summary>
        /// An MD5 hash of the file. Used for verifying the mod downloaded correctly.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// The type of release the mod is.
        /// </summary>
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty("url")]
        public string DownloadURL { get; set; }

        [Newtonsoft.Json.JsonProperty("id")]
        public string FileId { get; set; }

        public string Description;
    }
}
