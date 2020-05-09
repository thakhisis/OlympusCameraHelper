using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OlympusCameraHelper.Types;
using Newtonsoft.Json;

namespace OlympusCameraHelper.Services
{
    public class Configuration {
         [JsonConverter(typeof(DirectoryInfoConverter))]
        public DirectoryInfo LocalPath { get; set; }
         [JsonConverter(typeof(UriConverter))]
        public Uri DefaultUri { get; set; }
    }

    public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo ReadJson(JsonReader reader, Type objectType, [AllowNull] DirectoryInfo existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            new DirectoryInfo((string)reader.Value);

        public override void WriteJson(JsonWriter writer, [AllowNull] DirectoryInfo value, JsonSerializer serializer) => 
            writer.WriteValue(value.FullName);
    }

    public class UriConverter : JsonConverter<Uri>
    {
        public override Uri ReadJson(JsonReader reader, Type objectType, [AllowNull] Uri existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            new Uri((string)reader.Value);

        public override void WriteJson(JsonWriter writer, [AllowNull] Uri value, JsonSerializer serializer) => 
            writer.WriteValue(value.AbsoluteUri);
    }

    public class AppState
    {
        public Configuration Configuration { get; private set; }
        public event EventHandler StateChanged;

        public AppState() {
            Load();
        }

        //private List<MqttSubscription> _subscriptions { get; set; } = new List<MqttSubscription>();
        //public IEnumerable<MqttSubscription> Subscriptions => _subscriptions.AsEnumerable();
        
        public void Save() 
        {
            var fi = new FileInfo("config.json");
            using (var sw = fi.CreateText()) {
                var serializer = Newtonsoft.Json.JsonSerializer.Create();
                serializer.Serialize(sw, Configuration);
            }
            this.StateHasChanged();
        }

        public void Load() 
        {
            var fi = new FileInfo("config.json");
            if (fi.Exists)
            {
                using (var sr = new StreamReader(fi.OpenRead())) {
                    var serializer = Newtonsoft.Json.JsonSerializer.Create();
                    var configuration = serializer.Deserialize(sr, typeof(Configuration));
                    if (configuration is Configuration)
                        this.Configuration = configuration as Configuration;
                }            
            } else {
                this.Configuration = new Configuration();
            }       
        }

        public void StateHasChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
