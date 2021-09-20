﻿using Crunch.Domain;
using Crunch.Database.Models;
using Crunch.DataSources;
using Crunch.Strategies.Overnight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crunch.DataSources.Fmp.Endpoints;
using Microsoft.EntityFrameworkCore;
using ScottPlot;


namespace Crunch.UseCases
{
    class UseCase
    {
        #region weekly overnight
        /// <summary>
        /// Import prices needed for Weekly Overnight strategy into the database
        /// </summary>
        /// <param name="weekNum">calendar week number</param>
        public static void ImportPricesForOvernight(int weekNum)
        {
            // initialize
            var options = new PriceDownloadOptions(weekNum);
            var fmp = new FmpDataSource();
            List<Database.Models.Security> securities;

            using (var db = new stock_analyticsContext())
            {
                securities = db.Securities.ToList();
                foreach (var security in securities)
                {
                    Console.WriteLine($"Downloading {security.Symbol}");
                    try
                    {
                        var prices = fmp.GetPrices(security.Symbol, options.Interval,
                                                   options.Start,
                                                   options.End);
                        foreach (var price in prices)
                        {
                            string interval;
                            switch (price.interval)
                            {
                                case PriceInterval.OneDay:
                                    interval = "1d";
                                    break;
                                case PriceInterval.ThirtyMinutes:
                                    interval = "30m";
                                    break;
                                default:
                                    throw new ArgumentException("Interval doesn't exist");
                            }
                            var dbPrice = new Price
                            {
                                Close = price.close,
                                High = price.high,
                                Interval = interval,
                                Low = price.low,
                                Open = price.open,
                                Symbol = price.symbol,
                                Timestamp = price.timestamp,
                                Volume = (long)price.volume
                            };
                            db.Prices.Add(dbPrice);
                        }
                        db.SaveChanges();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    Console.WriteLine($"Symbol {security.Symbol} saved");
                }
            }
        }

        private static List<WinnersLosersReport> CalculateWinnersLosers(int weekNum)
        {
            #region database
            var db = new stock_analyticsContext(); 
            var stats = db.WeeklyOvernightStats
                .Where(s => s.WeekNum == weekNum).ToList();
            #endregion

            #region calculation
            var winnersCount = stats.Where(s => s.ReturnOnInitialCapital >= 0)
                .Where(s=> s.SecurityType == "stocks")
                .Where(s=> s.Strategy == "overnight")
                .Count();
            var losersCount = stats.Where(s => s.ReturnOnInitialCapital < 0)
                .Where(s => s.SecurityType == "stocks")
                .Where(s => s.Strategy == "overnight")
                .Count();
            #endregion

            #region return
            var reportData = new List<WinnersLosersReport>
            {
                new WinnersLosersReport { Type = "Winners", Count = winnersCount },
                new WinnersLosersReport { Type = "Losers", Count = losersCount }
            };
            return reportData;
            #endregion

        }
        public static void PlotWinnersLosers(int weekNum) 
        {
            var plt = new ScottPlot.Plot();
            var reportData = UseCase.CalculateWinnersLosers(weekNum);

            // HACK: this should be this method parameter
            double[] values = reportData.Select(d => (double)d.Count).ToArray();
            double[] positions = { 0, 1 };
            string[] labels = { reportData[0].Type, reportData[1].Type };
            var bar = plt.AddBar(values, positions);
            bar.ShowValuesAboveBars = true;
            plt.XTicks(positions, labels);
            plt.SetAxisLimits(yMin: 0);
            plt.SaveFig("D:\\PROJEKTI\\bar.png");
        }
        #endregion

        /// <summary>
        /// Update the list of securities in database
        /// </summary>
        public static void UpdateSecurities()
        {
            #region data source
            // instantiate sources
            var stocksSource = new StocksListEndpoint();
            var etfSource = new EtfListEndpoint();
            var symbolSource = new TradableSymbolsListEndpoint();
            // get symbols
            var updatedStocks = stocksSource.GetStocks();
            var updatedEtfs = etfSource.GetEtfs();
            var symbols = symbolSource.GetSymbols();
            // filter symbols from result
            var newStockSymbols = updatedStocks
                .Where(s => s.Symbol.Length <= 4)
                .Select(s => s.Symbol).ToList();
            var newEtfSymbols = updatedEtfs
                .Where(s => s.Symbol.Length <= 4)
                .Select(s => s.Symbol).ToList();
            var newTradablSymbols = symbols
                .Where(s => s.Symbol.Length <= 4)
                .Select(s => s.Symbol).ToList();
            // filter separate new stocks and new Etfs
            var newTradableStockSymbols = newTradablSymbols.Intersect(newStockSymbols).ToList();
            var newTradableEtfSymbols = newTradablSymbols.Intersect(newEtfSymbols).ToList();
            #endregion 

            #region database
            // initialize database context
            var db = new stock_analyticsContext();
            // truncate the table
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE securities");
            
            // insert symbols
            foreach (var symbol in newTradableStockSymbols)
            {
                var security = new Crunch.Database.Models.Security
                {
                    Symbol = symbol,
                    Type = "stocks"
                };
                db.Securities.Add(security);
                Console.Write("s");
            }
            foreach (var symbol in newTradableEtfSymbols)
            {
                var security = new Crunch.Database.Models.Security
                {
                    Symbol = symbol,
                    Type = "etfs"
                };
                db.Securities.Add(security);
                Console.Write('e');
            }
            db.SaveChanges();
            db.Dispose();
            #endregion


        }
    }
}
