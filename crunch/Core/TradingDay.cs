﻿using System;
using System.Linq;

namespace Crunch.Core
{
    public record TradingDay
    {
        /// <summary>
        /// Holiday dates in 2021 when market is closed
        /// </summary>
        private readonly DateOnly[] _holidays =
        {
            // memorial day
            new DateOnly(2021, 5, 31),

            // independance day
            new DateOnly(2021, 7, 5),

            // labor day
            new DateOnly(2021, 9, 6),

            // thanksgiving day
            new DateOnly(2021, 11, 25),

            // christmas day
            new DateOnly(2021, 12, 24),

            // Marthing Luther Jr. day
            new DateOnly(2022, 1,17),

            // Washington's birthday
            new DateOnly(2022, 2, 21),

            // Good Friday
            new DateOnly(2022, 4, 15),

            // Memorial day
            new DateOnly(2022, 5, 30),

            // Juneteenth National Independence Day
            new DateOnly(2022, 6, 20),

            // Independence day
            new DateOnly(2022, 7, 4),

            // labor day
            new DateOnly(2022, 9, 5),

            // thanks giving day
            new DateOnly(2022, 11, 24),

            // Christmass
            new DateOnly(2022, 12,26)
        };
        public DateOnly Date { get; init; }
        public TradingDay(DateOnly date)
        {
            Validate(date);
            Date = date;
        }

        private void Validate(DateOnly date)
        {
            if (!CheckIfTradingDay(date))
                throw new ArgumentException($"Date {date} is not a trading day.", nameof(date));
        }

        /// <summary>
        /// Check if stock market is open on the given date
        /// </summary>
        /// <param name="date">Date to check
        /// <returns></returns>
        private bool CheckIfTradingDay(DateOnly date)
        {
            if (
                (date.DayOfWeek == DayOfWeek.Saturday) ||
                (date.DayOfWeek == DayOfWeek.Sunday) ||
                _holidays.Contains(date)
               ) return false;
            else return true;
        }

        /// <summary>
        /// Find the previous trading day from the current date
        /// </summary>
        /// <returns></returns>
        public DateOnly FindPreviousTradingDay()
        {
            DateOnly prevDay = Date.AddDays(-1);
            while (CheckIfTradingDay(prevDay) == false)
            {
                prevDay = prevDay.AddDays(-1);
            }
            return prevDay;
        }
    }
}