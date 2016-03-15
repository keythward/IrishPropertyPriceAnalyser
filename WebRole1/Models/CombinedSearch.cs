

using DotNet.Highcharts;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System.Collections.Generic;
using System.ComponentModel;


namespace WebRole1.Models
{
    public class CombinedSearch
    {
        public IList<Line> lineList { get; set; }
        public CombinedSearch()
        {
            lineList= new List<Line>();
            lineList.Add(new Line());
        }

        public Highcharts createChart
        {
            get
            {
                Highcharts chart = new Highcharts("chart")
                .SetXAxis(new XAxis
                {
                    Categories = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
                })
                .SetSeries(new Series
                {
                    Data = new Data(new object[] { 29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4 })
                });
                return chart;
            }
        }
    }

    // line object
    public class Line
    {
        public string County { get; set; }
        public string PostCode { get; set; }
        public string Area { get; set; }
        [DisplayName("Date Range")]
        public string Date { get; set; }
        public string Year { get; set; }
    }
}
