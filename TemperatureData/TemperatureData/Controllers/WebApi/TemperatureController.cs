using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Assignment2.Models;

namespace Assignment2.Controllers
{
    [RoutePrefix("api/Temperature")]
    public class TemperatureController : ApiController
    {
        private BureauDBEntities db = new BureauDBEntities();

        // GET: api/Temperature/19017
        public object GetAnnualData(int station)
        {
            var data = db.Stations
                .Where(s => s.stationNumber == station)
                .Select(s => new
                {
                    // get the basic station detail to display (name, number)
                    s.stationNumber,
                    s.stationName,

                    // get the annual statistics to show (year, avg temps and rainfall)
                    annualStats =
                      db.DailyDatas
                     .Where(dd => dd.stationNumber == station)
                     .GroupBy(dd => new { dd.stationNumber, dd.year })
                     .OrderBy(dd => dd.Key.year)
                     .Select(p => new
                     {
                         p.Key.year,
                         minTemp = p.Where(dd => dd.minTemp.HasValue).Min(d => d.minTemp),
                         avgMinTemp = p.Where(dd => dd.minTemp.HasValue).Average(d => d.minTemp),
                         maxTemp = p.Where(dd => dd.maxTemp.HasValue).Max(d => d.maxTemp),
                         avgMaxTemp = p.Where(dd => dd.maxTemp.HasValue).Average(d => d.maxTemp),
                         avgTemp = p.Where(dd => dd.maxTemp.HasValue && dd.minTemp.HasValue).Average(d => d.minTemp + d.maxTemp),
                         minRain = p.Where(dd => dd.rainfall.HasValue).Min(d => d.rainfall),
                         maxRain = p.Where(dd => dd.rainfall.HasValue).Max(d => d.rainfall),
                         avgRain = p.Where(dd => dd.rainfall.HasValue).Average(d => d.rainfall),
                         records = p.Where(dd => dd.maxTemp.HasValue || dd.minTemp.HasValue).Count(),
                         missing = p.Where(dd => !dd.maxTemp.HasValue && !dd.minTemp.HasValue).Count()
                     }).ToList()
                }).FirstOrDefault();



            return data;
        }

        // GET: api/Temperature/{stationNumber}/{year}
        // GET: api/Temperature/19017/2017
        public object GetDetailData(int station, int year)
        {
            var data = db.Stations
                 .Where(s => s.stationNumber == station)
                 .Select(s => new
                 {
                     // get the basic station detail to display (name, number, year shown)
                     s.stationNumber,
                     s.stationName,
                     year,

                     // get the monthly statistics to show (temps and rainfall)
                     monthlyStats =
                       db.DailyDatas
                      .Where(dd => dd.stationNumber == station && dd.year == year)
                      .GroupBy(dd => new { dd.stationNumber, dd.year, dd.month })
                      .OrderBy(dd => dd.Key.month)
                      .Select(p => new MonthDetail
                      {
                          month = p.Key.month,
                          monthName = string.Empty,
                          minTemp = p.Where(dd => dd.minTemp.HasValue).Min(d => d.minTemp),
                          avgMinTemp = p.Where(dd => dd.minTemp.HasValue).Average(d => d.minTemp),
                          maxTemp = p.Where(dd => dd.maxTemp.HasValue).Max(d => d.maxTemp),
                          avgMaxTemp = p.Where(dd => dd.maxTemp.HasValue).Average(d => d.maxTemp),
                          avgTemp = p.Where(dd => dd.maxTemp.HasValue && dd.minTemp.HasValue).Average(d => d.minTemp + d.maxTemp),
                          minRain = p.Where(dd => dd.rainfall.HasValue).Min(d => d.rainfall),
                          maxRain = p.Where(dd => dd.rainfall.HasValue).Max(d => d.rainfall),
                          avgRain = p.Where(dd => dd.rainfall.HasValue).Average(d => d.rainfall),
                          records = p.Where(dd => dd.maxTemp.HasValue || dd.minTemp.HasValue).Count(),
                          missing = p.Where(dd => !dd.maxTemp.HasValue && !dd.minTemp.HasValue).Count(),

                          // get the daily stats of each month
                          dailyStats = p.Select(dd => new DayDetail
                          {
                              day = dd.day,
                              dayName = string.Empty,
                              minTemp = dd.minTemp,
                              maxTemp = dd.maxTemp,
                              rainfall = dd.rainfall,
                              rainDays = dd.rainDays
                          })
                          .OrderBy(dd => dd.day)
                          .ToList()
                      }).ToList()
                 }).FirstOrDefault();

            // the below updates can also be written as Lambda expressions
            // Here, the monthName and dayNames for the bureau records are being calculated
            // this is not possible using LINQ/Lambda -> SQL as it is unable to perform the translation
            // instead we assign the values after the database has been queried!
            foreach (var d in data.monthlyStats)
            {
                d.monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(d.month);
                foreach (var dd in d.dailyStats)
                {
                    dd.recordDate = new DateTime(year, d.month, dd.day);
                    dd.dayName = dd.recordDate.ToString("dddd");
                }
            }

            // return the final processed data with month and day names
            return data;
        }


        // complex data classes for internal stat arrays
        public class MonthDetail
        {
            public int month { get; set; }
            public string monthName { get; set; }
            public decimal? minTemp { get; set; }
            public decimal? avgMinTemp { get; set; }
            public decimal? maxTemp { get; set; }
            public decimal? avgMaxTemp { get; set; }
            public decimal? avgTemp { get; set; }
            public decimal? maxRain { get; set; }
            public decimal? minRain { get; set; }
            public decimal? avgRain { get; set; }
            public double? rainDays { get; set; }
            public int records { get; set; }
            public int missing { get; set; }
            public List<DayDetail> dailyStats { get; set; }


        }
        public class DayDetail
        {
            public DateTime recordDate { get; set; }
            public int day { get; set; }
            public string dayName { get; set; }
            public decimal? minTemp { get; set; }
            public decimal? maxTemp { get; set; }
            public decimal? rainfall { get; set; }
            public double? rainDays { get; set; }
        }



    }
}