﻿using Crunch.DataProviders.Fmp.Responses;
using System;
using System.Net.Http;
using System.Text.Json;

namespace Crunch.DataProviders.Fmp
{
    internal class FmpProvider
    {
        private const string _fmpApiKey = "***REMOVED***";
        private const string _fmpDomain = "https://financialmodelingprep.com/";
        private readonly HttpClient _httpClient = new();
        private readonly string _dateFormat = "yyyy-MM-dd";


        /// <summary>
        /// Get historical prices for a security during the given period
        /// </summary>
        /// <param name="security"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public SymbolPricesJsonResponse GetSecurityDailyPrice(string symbol, DateOnly start, DateOnly end)
        {
            string priceApiResponse = RequestDailyPrice(symbol, start.ToString(_dateFormat), end.ToString(_dateFormat));
            return JsonSerializer.Deserialize<SymbolPricesJsonResponse>(priceApiResponse);
        }

        /// <summary>
        /// Send an API request for the daily price of particular security for period between start and end date
        /// </summary>
        /// <example>https://financialmodelingprep.com/api/v4/historical-price/AAPL/1/minute/2021-02-12/2021-02-16?apikey=***REMOVED***</example>
        /// <param name="symbol">security symbol on exchange</param>
        /// <param name="startDate">first day to get prices data</param>
        /// <param name="endDate">last day to get prices data</param>
        /// <returns>Daily prices JSON data in OHLC format</returns>
        private string RequestDailyPrice(string symbol, string startDate, string endDate)
        {
            string query = $"/api/v4/historical-price/{symbol}/1/day/{startDate}/{endDate}?apikey={_fmpApiKey}";
            string url = _fmpDomain + query;
            var response = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            return response;
        }
    }
}