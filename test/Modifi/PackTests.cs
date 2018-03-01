using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Modifi.Mods;
using Modifi.Storage;


using Xunit;

namespace Modifi.Tests {
    
    public class PackTestFixture : IDisposable {

        public string dataDirectory {
            get { return Path.Combine(Environment.CurrentDirectory, "test_data"); }
            set { }
        }

        public PackTestFixture() {
            Directory.CreateDirectory(dataDirectory);
        }

        public void Dispose() {
            Directory.Delete(dataDirectory, true);
        }
    }

    public class PackTests : IClassFixture<PackTestFixture> {
        
        public string dataDirectory;

        public PackTests(PackTestFixture fixture) {
            this.dataDirectory = fixture.dataDirectory;
        }

        protected Pack CreatePack() {
            Pack p = new Pack();
            p.Name ="Pack";
            p.MinecraftVersion = "1.12.2";
            p.Mods = new System.Collections.Generic.Dictionary<string, string>() {
                { "test:test-mod", "1.0.0" }
            };

            return p;
        }

        [Fact]
        public void TestInvalidPackLoad () {
            // Pack.json does not exist
            Assert.Throws<FileNotFoundException>(() => {
                Pack.Load("pack.json");
            });
        }

        [Fact]
        public async Task TestPackCreation() {
            string filename = Path.Combine(dataDirectory, "test-pack.json");
            
            Debug.WriteLine("Creating pack object.");
            Pack p = new Pack();
            p.Name = "Testing";
            p.MinecraftVersion = "1.12.2";

            Debug.WriteLine("Saving to {0}...", filename);
            await p.SaveAs(filename);

            Assert.True(File.Exists(filename));
        }

        [Fact]
        public async Task TestPackRead() {
            Pack original = CreatePack();

            string file = Path.Combine(dataDirectory, "original.json");
            await original.SaveAs(file);

            Pack loaded = Pack.Load(file);

            Assert.Equal(loaded.Name, original.Name);
            Assert.Equal(loaded.MinecraftVersion, original.MinecraftVersion);
            Assert.Equal(loaded.Mods.Count, original.Mods.Count);
        }

        [Fact]
        public void TestPackAdd() {
            Pack original = CreatePack();

            // Should not be able to add the mod: it already exists
            bool added = original.AddMod("test:test-mod", "1.0.0");
            Assert.False(added);

            // However, a new mod should return true
            bool addedNew = original.AddMod("test:new-mod", "1.0.0");
            Assert.True(addedNew);
        }

        [Fact]
        public void TestPackRemove() {
            Pack original = CreatePack();

            // Should be able to remove one
            bool removed = original.RemoveMod("test:test-mod");
            Assert.True(removed);

            // Cannot remove a mod twice
            bool removedTwice = original.RemoveMod("test:test-mod");
            Assert.False(removedTwice);

            // Cannot remove a mod that does not exist in the pack
            bool removeNonexistent = original.RemoveMod("test:non-existant");
            Assert.False(removeNonexistent);
        }

        [Fact]
        public void TestPackDownload() { }

        [Fact]
        public void TestPackModStatus() {
            Pack original = CreatePack();

            // Test mod that hasn't been added to the pack
            Assert.Equal(ModStatus.NotInstalled, original.GetModStatus("test:not-valid"));

            // Added to the pack, but not downloaded
            Assert.Equal(ModStatus.Requested, original.GetModStatus("test:test-mod"));

            // TODO: Pack Download
            
            // TODO: Test for "downloaded" mod
        }
    }
}