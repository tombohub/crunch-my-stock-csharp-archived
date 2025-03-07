﻿using System.Drawing;
using Crunch.Core.Multiplots;
using Crunch.Images;

namespace Crunch.Strategies.Overnight.AreaContents
{
    internal class SpyRoiReport : IAreaContent
    {
        /// <summary>
        /// SPY roi metric in percentage
        /// </summary>
        private decimal _spyRoi;

        public SpyRoiReport(decimal spyRoi)
        {
            _spyRoi = spyRoi;
        }

        /// <inheritdoc/>
        public Bitmap RenderImage(int width, int height)
        {
            string text = $"SPY ROI\n{_spyRoi}%";
            var plotter = new Plotter();

            var spyRoiPlot = plotter.RenderTextRectangle(text, width, height);
            return spyRoiPlot;
        }
    }
}