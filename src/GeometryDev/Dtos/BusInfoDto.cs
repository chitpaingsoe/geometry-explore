﻿using System.Text.Json.Serialization;

namespace GeomertryDev.Dtos
{
    public class BusInfoDto
    {

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; } = 0;

        [JsonPropertyName("lng")]
        public double Longitude { get; set; } = 0;
    }
}
