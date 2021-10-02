﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crunch.Strategies.Overnight;
using Crunch.Database;
using Crunch.Database.Models;
using Crunch.Domain;


namespace CrunchTests
{
    [TestClass]
    public class OvernightReportsTests
    {
        public static List<WeeklyOvernightStat> Stats { get; set; }

        [ClassInitialize]
        public static void GetStats(TestContext context)
        {
            Stats = DatabaseAPI.GetWeeklyOvernightStats(37);
        }
        [TestMethod]
        public void GetSpyOvernightRoi_OvernightStatsData_ReturnsCorrectNumber()
        {
            //var stats = DatabaseAPI.GetWeeklyOvernightStats(37);
            var overnightStats = new OvernightStats(Stats);
            var spyOvernightRoi = overnightStats.GetSpyOvernightRoi();
            Assert.AreEqual(spyOvernightRoi, -0.016582899999999946);
        }

        [TestMethod]
        public void GetSpyBenchmarkRoi_OvernightStatsData_ReturnsCorrectNumber()
        {
            //var stats = DatabaseAPI.GetWeeklyOvernightStats(37);
            var overnightStats = new OvernightStats(Stats);
            var spyBenchmarkRoi = overnightStats.GetSpyBenchmarkRoi();
            Assert.AreEqual(spyBenchmarkRoi, -0.026090999999999975);
        }

        [DataTestMethod]
        [DataRow(SecurityType.Stock, -0.017692364756611095)]
        [DataRow(SecurityType.Etf, -0.016191809945275748)]
        public void CalculateAverageOvernightRoi_OvernightStocksStatsData_ReturnsCorrectNumber(SecurityType securityType, double result)
        {
            //var stats = DatabaseAPI.GetWeeklyOvernightStats(37);
            var overnightStats = new OvernightStats(Stats);
            var avgRoi = overnightStats.CalculateAverageOvernightRoi(securityType);
            Assert.AreEqual(avgRoi, result);
        }

    }
}
