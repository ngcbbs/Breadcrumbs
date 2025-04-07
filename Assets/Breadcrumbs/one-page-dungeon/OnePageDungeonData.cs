using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Breadcrumbs.one_page_dungeon {
    public partial class OnePageDungeonData {
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("story")] public string Story { get; set; }
        [JsonProperty("rects")] public Rect[] Rects { get; set; }
        [JsonProperty("doors")] public Door[] Doors { get; set; }
        [JsonProperty("notes")] public Note[] Notes { get; set; }
        [JsonProperty("columns")] public object[] Columns { get; set; }
        [JsonProperty("water")] public Water[] Water { get; set; }
    }

    public partial class Door {
        [JsonProperty("x")] public long X { get; set; }
        [JsonProperty("y")] public long Y { get; set; }
        [JsonProperty("dir")] public Water Dir { get; set; }
        [JsonProperty("type")] public long Type { get; set; }
    }

    public partial class Water {
        [JsonProperty("x")] public long X { get; set; }
        [JsonProperty("y")] public long Y { get; set; }
    }

    public partial class Note {
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("ref")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Ref { get; set; }

        [JsonProperty("pos")] public Water Pos { get; set; }
    }

    public partial class Rect {
        [JsonProperty("x")] public long X { get; set; }
        [JsonProperty("y")] public long Y { get; set; }
        [JsonProperty("w")] public long W { get; set; }
        [JsonProperty("h")] public long H { get; set; }

        [JsonProperty("rotunda", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Rotunda;

        [JsonProperty("ending", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ending { get; set; }

        public override string ToString() {
            return $"{X}, {Y}, {W}, {H}, {Rotunda?.ToString()}, {Ending?.ToString()}";
        }
    }

    public partial class OnePageDungeonData {
        public static OnePageDungeonData FromJson(string json) =>
            JsonConvert.DeserializeObject<OnePageDungeonData>(json, Converter.Settings);
    }

    public static class Serialize {
        public static string ToJson(this OnePageDungeonData self) =>
            JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l)) {
                return l;
            }

            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}