﻿using System;
using System.Collections.Generic;

namespace Crunch.Database.Models
{
    public partial class PricesDaily
    {
        public int Id { get; set; }
        public DateOnly Timestamp { get; set; }
        public string Symbol { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public string Interval { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
