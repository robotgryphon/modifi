using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modifi.Storage {
    public abstract class StorageUtilities {

        public static JsonSerializerSettings PACK_SERIALIZER_SETTINGS = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },

            Formatting = Formatting.Indented
        };

        public static JsonSerializer PACK_SERIALIZER = JsonSerializer.Create(PACK_SERIALIZER_SETTINGS);
    }
}
