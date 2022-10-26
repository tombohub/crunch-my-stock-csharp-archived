﻿using System.Text.Json.Serialization;

namespace CrunchImport.FmpDataProvider
{
    internal record TradeableSymbolsJsonModel
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; init; }

        [JsonPropertyName("exchangeShortName")]
        public string ExchangeShortName { get; init; }
    }
}
