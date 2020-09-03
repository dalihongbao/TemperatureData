using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2.Models
{
    public static class StationCheck
    {
        public static bool exists(int stationNumber, BureauDBEntities db)
        {
            Station station = null;
            try
            {
                station = db.Stations.Find(stationNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }

            return station != null;
        }

    }
}