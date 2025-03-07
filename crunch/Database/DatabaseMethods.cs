﻿using Crunch.Core;
using Crunch.Database.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Crunch.Database
{
    /// <summary>
    /// General methods for performing various tasks
    /// </summary>
    public class DatabaseMethods
    {
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private readonly stock_analyticsContext _db = new stock_analyticsContext();

        /// <summary>
        /// Get list of all symbols from database
        /// </summary>
        /// <returns></returns>
        public List<Core.Security> GetSecurities()
        {
            var symbols = _db.Securities
                .Where(x => x.Status == SecurityStatus.Active.ToString())
                .Select(s => new Core.Security
                {
                    Exchange = Enum.Parse<Exchange>(s.Exchange),
                    IpoDate = s.IpoDate,
                    Status = Enum.Parse<SecurityStatus>(s.Status),
                    Type = Enum.Parse<SecurityType>(s.Type),
                    Symbol = new Symbol(s.Symbol),
                }).ToList();
            return symbols;
        }

        /// <summary>
        /// Get list of Symbols by security type
        /// </summary>
        /// <param name="securityType"></param>
        /// <returns></returns>
        public List<Symbol> GetSecuritySymbols(SecurityType securityType)
        {
            var securites = _db.Securities
                .Where(x => x.Type == securityType.ToString())
                .Select(x => new Symbol(x.Symbol))
                .ToList();

            return securites;
        }

        /// <summary>
        /// Get multiplot image size in pixels for the given strategy.
        /// Uses SQL to calculate the size.
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public Size GetMultiplotSize(StrategyName strategy)
        {
            var dbConnection = DbConnections.CreatePsqlConnection();
            var sql = @"SELECT  max((x+width)*scale) as width, max((y+height)*scale) as height
                        FROM public.multiplot_coordinates
                        WHERE strategy = 'Overnight' and is_included = true";
            var parameters = new { Strategy = strategy.ToString() };
            var multiplotSize = dbConnection.QuerySingle<Size>(sql, parameters);
            return multiplotSize;
        }

        /// <summary>
        /// Returns list of dates for which there is daily price data in database
        /// </summary>
        /// <returns></returns>
        public List<DateOnly> priceDates()
        {
            var dates = _db.PricesDailies
                .Select(x => x.Date)
                .Distinct()
                .ToList();
            return dates;
        }

        /// <summary>
        /// Save daily price pricesDto to database. If price for that day/symbol exists
        /// then it will be updated.
        /// </summary>
        /// <param name="price"></param>
        public void SaveDailyPrice(SecurityPrice price)
        {
            var security = _db.Securities
                .Where(x => x.Symbol == price.Symbol.Value)
                .Single();

            var priceDb = new PricesDaily
            {
                Date = price.TradingDay.Date,
                Symbol = price.Symbol.Value,
                SecurityId = security.Id,
                Open = price.OHLC.Open,
                High = price.OHLC.High,
                Low = price.OHLC.Low,
                Close = price.OHLC.Close,
                Volume = price.Volume,
            };
            _db.PricesDailies
                .Upsert(priceDb)
               .On(x => new { x.SecurityId, x.Date })
               .WhenMatched(x => new PricesDaily
               {
                   Open = price.OHLC.Open,
                   High = price.OHLC.High,
                   Low = price.OHLC.Low,
                   Close = price.OHLC.Close
               })
               .Run();
        }

        public void SaveDailyPrice(List<SecurityPrice> prices)
        {
            foreach (SecurityPrice price in prices)
            {
                SaveDailyPrice(price);
            }
        }


        /// <summary>
        /// Save security to database. If security exists in database it will be updated.
        /// </summary>
        /// <param name="security"></param>
        public void SaveSecurity(Core.Security security)
        {
            string sql = $@"INSERT INTO public.securities (symbol, type, exchange, updated_at, status, ipo_date, delisting_date)
                            VALUES(
                                    @Symbol,
                                    @Type,
                                    @Exchange,
                                    @UpdatedAt,
                                    @Status,
                                    @IpoDate,
                                    @DelistingDate
                                    )
                            ON CONFLICT ON CONSTRAINT securities_symbol_un
                            DO UPDATE SET type = @Type,
                                           exchange = @Exchange,
                                           updated_at = @UpdatedAt,
                                            status = @Status,
                                            ipo_date = @IpoDate,
                                            delisting_date = @DelistingDate";
            var parameters = new
            {
                Symbol = security.Symbol.Value,
                Type = security.Type.ToString(),
                Exchange = security.Exchange.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Status = security.Status.ToString(),
                IpoDate = security.IpoDate,
                DelistingDate = security.DelistingDate
            };
            using var conn = DbConnections.CreatePsqlConnection();
            conn.Execute(sql, parameters);
        }

        /// <summary>
        /// Get all pricesDb for overnight strategy
        /// </summary>
        /// <param name="tradingDay"></param>
        /// <returns></returns>
        public DailyPricesRegular GetPrices(TradingDay tradingDay)
        {
            // get today pricesDb
            var pricesDb = _db.PricesDailies
                .Include(x => x.Security)
                .Where(x => x.Date == tradingDay.Date)
                .ToList();

            var securityPrices = new List<SecurityPrice>();
            foreach (var price in pricesDb)
            {
                Symbol symbol = new Symbol(price.Symbol);
                SecurityType securityType = Enum.Parse<SecurityType>(price.Security.Type);
                OHLC ohlc = new OHLC(price.Open, price.High, price.Low, price.Close);

                securityPrices.Add(new SecurityPrice
                {
                    TradingDay = tradingDay,
                    Symbol = symbol,
                    SecurityType = securityType,
                    OHLC = ohlc,
                    Volume = (int)price.Volume
                });
            };

            var dailyPrices = new DailyPricesRegular
            {
                TradingDay = tradingDay,
                SecurityPrices = securityPrices
            };
            return dailyPrices;
        }


        public Core.WinnersLosersCount GetWinnersLosers(TradingDay tradingDay, SecurityType securityType)
        {
            var winnersLosersDb = _db.WinnersLosersCounts
                .Where(x => x.Date == tradingDay.Date)
                .Where(x => x.SecurityType == securityType.ToString())
                .Single();

            var winnersLosers = new Core.WinnersLosersCount
            {
                TradingDay = new TradingDay(winnersLosersDb.Date),
                WinnersCount = winnersLosersDb.WinnersCount,
                LosersCount = winnersLosersDb.LosersCount,
                SecurityType = Enum.Parse<SecurityType>(winnersLosersDb.SecurityType)
            };

            return winnersLosers;
        }

        public Core.WinnersLosersCount WinnersLosersCount(TradingDay tradingDay)
        {
            var winnersCount = _db.DailyOvernightPerformances
                .Count(x => x.Date == tradingDay.Date && x.ChangePct > 0);

            var losersCount = _db.DailyOvernightPerformances
                .Count(x => x.Date == tradingDay.Date && x.ChangePct < 0);

            return new Core.WinnersLosersCount
            {
                TradingDay = tradingDay,
                WinnersCount = winnersCount,
                LosersCount = losersCount
            };
        }


        /// <summary>
        /// Saves overnight prices into the database
        /// </summary>
        /// <param name="overnightPrices"></param>
        public void SaveOvernightPrices(List<SecurityPriceOvernight> overnightPrices)
        {
            var performancesDb = new List<DailyOvernightPerformance>();
            foreach (var price in overnightPrices)
            {
                Console.WriteLine($"saving {price.Symbol.Value}");
                var security = _db.Securities.Where(x => x.Symbol == price.Symbol.Value).Single();
                var perfDb = new DailyOvernightPerformance
                {
                    Date = price.TradingDay.Date,
                    Open = price.OHLC.Close,
                    PrevDayClose = price.OHLC.Open,
                    SecurityId = security.Id,
                    ChangePct = (price.OHLC.Close - price.OHLC.Open) / price.OHLC.Open * 100,
                };
                performancesDb.Add(perfDb);


            }
            _db.DailyOvernightPerformances
                    .UpsertRange(performancesDb)
                    .On(x => new { x.SecurityId, x.Date })
                    .Run();
        }

        public SecurityType GetSymbolSecurityType(Symbol symbol)
        {
            var secTypeDb = _db.Securities
                .Where(x => x.Symbol == symbol.Value)
                .Select(x => x.Type)
                .Single();

            SecurityType secType = Enum.Parse<SecurityType>(secTypeDb);
            return secType;
        }

        /// <summary>
        /// Gets the last recorded date in overnight prices table
        /// </summary>
        /// <returns></returns>
        public DateOnly GetLastRecordedOvernightDate()
        {
            DateOnly lastDate = _db.DailyOvernightPerformances
                .OrderBy(x => x.Date)
                .Select(x => x.Date)
                .Last();
            return lastDate;
        }

        /// <summary>
        /// Get average roi accross all securities on the given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal GetAverageRoi(DateOnly date)
        {
            decimal avgRoi = _db.DailyOvernightPerformances
                .Where(x => x.Date == date)
                .Average(x => x.ChangePct)
                .Value;

            return avgRoi;
        }
    }
}