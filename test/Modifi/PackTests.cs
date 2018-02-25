using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
                { "curseforge:jei", "latest" }
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
            Pack test = CreatePack();

            original.AddMod("test:testmod", "latest");

            Assert.NotEqual(original.Mods, test.Mods);
            Assert.True(original.Mods.Count > test.Mods.Count);
        }

        [Fact]
        public void TestPackRemove() {
            Pack original = CreatePack();
            Pack test = CreatePack();

            original.RemoveMod("curseforge:jei");

            Assert.NotEqual(original.Mods, test.Mods);
            Assert.True(original.Mods.Count < test.Mods.Count);
        }
    }
}