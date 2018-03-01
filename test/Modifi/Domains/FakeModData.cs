using Modifi.Mods;

namespace Modifi.Tests.Domains {

    internal class FakeModMetadata : ModMetadata {

        public string Name;

        public string Description;

        public string ModIdentifier;

        public string MinecraftVersion;

        public FakeModMetadata() {
            this.Description = "";
            this.Name = "Unnamed Fake Mod";
            this.MinecraftVersion = "1.12.2";
        }

        public FakeModMetadata(string id): this() {
            this.ModIdentifier = id;
        }

        public FakeModMetadata(string id, string description): this() {
            this.ModIdentifier = id;
            this.Description = description;
        }

        public override string GetDescription() {
            return Description;
        }

        public override string GetMinecraftVersion() {
            return MinecraftVersion ?? "1.12.2";
        }

        public override string GetModIdentifier() {
            return ModIdentifier;
        }

        public override string GetName() {
            return Name;
        }

        public override bool HasDescription() {
            throw new System.NotImplementedException();
        }
    }

    internal class FakeModVersion : ModVersion {

        public string Version;

        public ModReleaseType ReleaseType;

        public string Name;

        public FakeModVersion(string version) {
            this.Name = string.Format("[Mod Version {0}]", version);
            this.ReleaseType = ModReleaseType.Release;
            this.Version = version;
        }

        public FakeModVersion(string version, ModReleaseType releaseType) : this(version) {
            this.ReleaseType = releaseType;
        }

        public override string GetModVersion() {
            return Version;
        }

        public override ModReleaseType GetReleaseType() {
            return ReleaseType;
        }

        public override string GetVersionName() {
            return Name;
        }
    }
}