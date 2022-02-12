﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch.Domain
{
    /// <summary>
    /// Represents Number of winners and losers on particular date
    /// for the specific price range. Price Range is marked by price upper bound property.
    /// </summary>
    internal record WinnersLosersCountByPrice
    {
        /// <summary>
        /// Number of winning securities in the price range
        /// </summary>
        internal int WinnersCount { get; set;}

        /// <summary>
        /// Number of losing securities in the price range
        /// </summary>
        internal int LosersCount { get; set;}

        /// <summary>
        /// Upper bound of the price range, inclusive.
        /// </summary>
        internal int PriceUpperBound { get; set;}
    }
}
