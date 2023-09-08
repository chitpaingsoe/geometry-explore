using System.Text.Json.Serialization;

namespace GeomertryDev.Dtos
{
    public class SearchDto
    {

        [JsonPropertyName("south")]
        public double Y1 { get; set; } = 0;

        [JsonPropertyName("west")]
        public double X1 { get; set; } = 0;

        [JsonPropertyName("north")]
        public double Y2 { get; set; } = 0;

        [JsonPropertyName("east")]
        public double X2 { get; set; } = 0;
    }
}
