﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Crunch.Database.Models;

public partial class stock_analyticsContext : DbContext
{
    public stock_analyticsContext()
    {
    }

    public stock_analyticsContext(DbContextOptions<stock_analyticsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AverageRoi> AverageRois { get; set; }

    public virtual DbSet<DailyOvernightPerformance> DailyOvernightPerformances { get; set; }

    public virtual DbSet<MultiplotCoordinate> MultiplotCoordinates { get; set; }

    public virtual DbSet<PricesDaily> PricesDailies { get; set; }

    public virtual DbSet<PricesIntraday> PricesIntradays { get; set; }

    public virtual DbSet<PricesJoinSecurity> PricesJoinSecurities { get; set; }

    public virtual DbSet<Security> Securities { get; set; }

    public virtual DbSet<SpyRoi> SpyRois { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<WeeklyOvernightStat> WeeklyOvernightStats { get; set; }

    public virtual DbSet<WinnersLosersCount> WinnersLosersCounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=***REMOVED***;Port=5432;Database***REMOVED***;User Id=***REMOVED***;Password=***REMOVED***;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("exchange", new[] { "AMEX", "NYSE", "NASDAQ" })
            .HasPostgresEnum("price_interval", new[] { "OneDay", "ThirtyMinutes" })
            .HasPostgresEnum("security_type", new[] { "Stock", "Etf", "Trust" });

        modelBuilder.Entity<AverageRoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("average_roi_pk");

            entity.ToTable("average_roi", "overnight", tb => tb.HasComment("Daily average overnight ROI for the strategy accross all securities. Value is in %"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AverageRoi1)
                .HasComment("average roi in %")
                .HasColumnName("average_roi");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.SecurityId).HasColumnName("security_id");

            entity.HasOne(d => d.Security).WithMany(p => p.AverageRois)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("average_roi_fk");
        });

        modelBuilder.Entity<DailyOvernightPerformance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("overnight_daily_stats_pk");

            entity.ToTable("daily_overnight_performance", "overnight");

            entity.HasIndex(e => new { e.Date, e.SecurityId }, "daily_overnight_performance_un").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ChangePct)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("(((open - prev_day_close) / prev_day_close) * (100)::numeric)", true)
                .HasColumnName("change_pct");
            entity.Property(e => e.Date)
                .HasComment("Date of the strategy")
                .HasColumnName("date");
            entity.Property(e => e.Open)
                .HasComment("Strategy date opening price")
                .HasColumnName("open");
            entity.Property(e => e.PrevDayClose)
                .HasComment("Previous trading day closing price")
                .HasColumnName("prev_day_close");
            entity.Property(e => e.SecurityId).HasColumnName("security_id");

            entity.HasOne(d => d.Security).WithMany(p => p.DailyOvernightPerformances)
                .HasForeignKey(d => d.SecurityId)
                .HasConstraintName("prices_daily_overnight_fk");
        });

        modelBuilder.Entity<MultiplotCoordinate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("multiplot_coordinates_pk");

            entity.ToTable("multiplot_coordinates", tb => tb.HasComment("Coordinates  and size for each report plot so it can be draw properly inside the code and create multiplot image.\r\n\r\nCoordinates x and y represen top left corner of the rectangle. Size is marked by 'width' and 'height'. width goes along x axis left to right and height along y axis top down."));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AreaName)
                .IsRequired()
                .HasComment("name of the report which also corresponds with Enum in application code")
                .HasColumnName("area_name");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.IsIncluded)
                .HasComment("true if report is included in multiplot")
                .HasColumnName("is_included");
            entity.Property(e => e.Strategy)
                .IsRequired()
                .HasComment("name of the strategy which also corresponds with Enum in application code")
                .HasColumnName("strategy");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");
        });

        modelBuilder.Entity<PricesDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prices_daily_pk");

            entity.ToTable("prices_daily");

            entity.HasIndex(e => new { e.Date, e.Symbol }, "date_symbol_un").IsUnique();

            entity.HasIndex(e => new { e.Date, e.SecurityId }, "prices_daily__security_id_date_un").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.High).HasColumnName("high");
            entity.Property(e => e.Low).HasColumnName("low");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.SecurityId).HasColumnName("security_id");
            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnName("symbol");
            entity.Property(e => e.Volume).HasColumnName("volume");

            entity.HasOne(d => d.Security).WithMany(p => p.PricesDailies)
                .HasForeignKey(d => d.SecurityId)
                .HasConstraintName("prices_daily_fk");
        });

        modelBuilder.Entity<PricesIntraday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prices_intraday_pk");

            entity.ToTable("prices_intraday");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.High).HasColumnName("high");
            entity.Property(e => e.Interval)
                .HasMaxLength(10)
                .HasColumnName("interval");
            entity.Property(e => e.Low).HasColumnName("low");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.Symbol)
                .HasMaxLength(50)
                .HasColumnName("symbol");
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.Volume).HasColumnName("volume");
        });

        modelBuilder.Entity<PricesJoinSecurity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("prices_join_securities", "overnight");

            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DelistingDate).HasColumnName("delisting_date");
            entity.Property(e => e.Exchange).HasColumnName("exchange");
            entity.Property(e => e.IpoDate).HasColumnName("ipo_date");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Symbol)
                .HasMaxLength(4)
                .HasColumnName("symbol");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<Security>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("securities_pk");

            entity.ToTable("securities", tb => tb.HasComment("List of available securities on market. Stocks and ETFs"));

            entity.HasIndex(e => e.Symbol, "securities_symbol_un").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DelistingDate)
                .HasComment("date when security was delisted, if delisted")
                .HasColumnName("delisting_date");
            entity.Property(e => e.Exchange)
                .IsRequired()
                .HasColumnName("exchange");
            entity.Property(e => e.IpoDate)
                .HasComment("date of initial public offering")
                .HasColumnName("ipo_date");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasComment("active or delisted")
                .HasColumnName("status");
            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnName("symbol");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<SpyRoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("spy_roi_pk");

            entity.ToTable("spy_roi", "overnight", tb => tb.HasComment("SPY roi (pct change) for a benchmark"));

            entity.HasIndex(e => e.Date, "spy_roi_un").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Roi).HasColumnName("roi");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tasks_pk");

            entity.ToTable("tasks", tb => tb.HasComment("Tasks to be run periodically"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Task1)
                .IsRequired()
                .HasColumnName("task");
        });

        modelBuilder.Entity<WeeklyOvernightStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("weekly_overnight_stats_pk");

            entity.ToTable("weekly_overnight_stats", "overnight");

            entity.HasIndex(e => new { e.Symbol, e.Strategy, e.WeekNum }, "symbol_strategy_week_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AvgDay).HasColumnName("avg_day");
            entity.Property(e => e.AvgLossPerLosingTrade).HasColumnName("avg_loss_per_losing_trade");
            entity.Property(e => e.AvgPctGainPerTrade).HasColumnName("avg_pct_gain_per_trade");
            entity.Property(e => e.AvgProfitPerTrade).HasColumnName("avg_profit_per_trade");
            entity.Property(e => e.AvgProfitPerWinningTrade).HasColumnName("avg_profit_per_winning_trade");
            entity.Property(e => e.AvgWeeklyClosedOutDrawdown).HasColumnName("avg_weekly_closed_out_drawdown");
            entity.Property(e => e.AvgWeeklyClosedOutRunup).HasColumnName("avg_weekly_closed_out_runup");
            entity.Property(e => e.BeginningBalance).HasColumnName("beginning_balance");
            entity.Property(e => e.BestDay).HasColumnName("best_day");
            entity.Property(e => e.DailyStd).HasColumnName("daily_std");
            entity.Property(e => e.End).HasColumnName("end");
            entity.Property(e => e.EndingBalance).HasColumnName("ending_balance");
            entity.Property(e => e.GrossLoss).HasColumnName("gross_loss");
            entity.Property(e => e.GrossProfit).HasColumnName("gross_profit");
            entity.Property(e => e.LargestLossLosingTrade).HasColumnName("largest_loss_losing_trade");
            entity.Property(e => e.LargestPctLosingTrade).HasColumnName("largest_pct_losing_trade");
            entity.Property(e => e.LargestPctWinningTrade).HasColumnName("largest_pct_winning_trade");
            entity.Property(e => e.LargestProfitWinningTrade).HasColumnName("largest_profit_winning_trade");
            entity.Property(e => e.MaxClosedOutDrawdown).HasColumnName("max_closed_out_drawdown");
            entity.Property(e => e.MaxConsecutiveLosingTrades).HasColumnName("max_consecutive_losing_trades");
            entity.Property(e => e.MaxConsecutiveWinningTrades).HasColumnName("max_consecutive_winning_trades");
            entity.Property(e => e.MaxWeeklyClosedOutDrawdown).HasColumnName("max_weekly_closed_out_drawdown");
            entity.Property(e => e.MaxWeeklyClosedOutRunup).HasColumnName("max_weekly_closed_out_runup");
            entity.Property(e => e.NumEvenTrades).HasColumnName("num_even_trades");
            entity.Property(e => e.NumLosingTrades).HasColumnName("num_losing_trades");
            entity.Property(e => e.NumWinningTrades).HasColumnName("num_winning_trades");
            entity.Property(e => e.PctProfitableTrades).HasColumnName("pct_profitable_trades");
            entity.Property(e => e.ProfitFactor).HasColumnName("profit_factor");
            entity.Property(e => e.RatioAvgProfitWinLoss).HasColumnName("ratio_avg_profit_win_loss");
            entity.Property(e => e.ReturnOnInitialCapital).HasColumnName("return_on_initial_capital");
            entity.Property(e => e.SecurityType)
                .IsRequired()
                .HasMaxLength(5)
                .HasColumnName("security_type");
            entity.Property(e => e.SharpeRatio).HasColumnName("sharpe_ratio");
            entity.Property(e => e.SharpeRatioMax).HasColumnName("sharpe_ratio_max");
            entity.Property(e => e.SharpeRatioMin).HasColumnName("sharpe_ratio_min");
            entity.Property(e => e.SortinoRatio).HasColumnName("sortino_ratio");
            entity.Property(e => e.Start).HasColumnName("start");
            entity.Property(e => e.StartPrice).HasColumnName("start_price");
            entity.Property(e => e.Strategy)
                .IsRequired()
                .HasMaxLength(15)
                .HasColumnName("strategy");
            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnName("symbol");
            entity.Property(e => e.TotalNetProfit).HasColumnName("total_net_profit");
            entity.Property(e => e.WeekNum).HasColumnName("week_num");
            entity.Property(e => e.WorstDay).HasColumnName("worst_day");
        });

        modelBuilder.Entity<WinnersLosersCount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("winners_losers_count_pk");

            entity.ToTable("winners_losers_count", "overnight", tb => tb.HasComment("Report counting how many stocks were up overnight (winners) how many down (losers)"));

            entity.HasIndex(e => new { e.Date, e.SecurityType }, "winners_losers_count_un").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.LosersCount).HasColumnName("losers_count");
            entity.Property(e => e.SecurityType)
                .IsRequired()
                .HasColumnName("security_type");
            entity.Property(e => e.WinnersCount).HasColumnName("winners_count");
        });
        modelBuilder.HasSequence("id");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
