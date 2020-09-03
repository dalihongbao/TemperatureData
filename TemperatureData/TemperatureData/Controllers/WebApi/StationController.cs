using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Assignment2.Models;

namespace Assignment2.Controllers
{
    public class StationController : ApiController
    {
        private BureauDBEntities db = new BureauDBEntities();

        // GET: api/Station
        public IQueryable<object> GetDailyDatas()
        {
            // this query returns a list of stations with:
            // 1. station number
            // 2. min and max year of BOM data for the station
            // 3. count of daily records with a temperature value between the min and max year
            // 4. count of daily records missing a min & max temperature values.
            var stations = db.Stations.Include(d => d.DailyDatas)
                        .GroupBy(d => new { d.stationName, d.stationNumber })
                        .OrderBy(d => d.Key.stationName)
                        .Select(d => new
                        {
                            d.Key.stationNumber,
                            d.Key.stationName,
                            fromYear = d.Select(s => s.DailyDatas.Min(dd => dd.year)).FirstOrDefault(),
                            toYear = d.Select(s => s.DailyDatas.Max(dd => dd.year)).FirstOrDefault(),
                            records = d.Select(s => s.DailyDatas.Where(dd => dd.maxTemp.HasValue || dd.minTemp.HasValue).Count()).FirstOrDefault(),
                            missing = d.Select(s => s.DailyDatas.Where(dd => !dd.maxTemp.HasValue && !dd.minTemp.HasValue).Count()).FirstOrDefault()
                        }
                        );
            return stations;
        }

    }
}