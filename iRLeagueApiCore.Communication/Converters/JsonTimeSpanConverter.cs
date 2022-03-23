using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace iRLeagueApiCore.Communication.Converters
{
    public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeof(long).IsAssignableFrom(typeToConvert))
            {
                var longValue = reader.GetInt64();
                return new TimeSpan(longValue);
            }
            else
            {
                var timeSpanString = reader.GetString();
                return TimeSpan.ParseExact(timeSpanString, @"hh\:mm\:ss\.fffff", null);
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(@"hh\:mm\:ss\.fffff"));
        }
    }
}
