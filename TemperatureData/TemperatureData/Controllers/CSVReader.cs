using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Assignment2.Controllers
{
    public static class CSVReader
    {
        private enum search { contains, startswith};

        public static void readData(HttpPostedFileBase file, HttpServerUtilityBase Server)
        {
            var path = string.Empty;

            int stationIndex = -1, // station ID column
                yearIndex = -1,    // year of record column
                monthIndex = -1,
                dayIndex = -1,
                minIndex = -1, // min temp column
                maxIndex = -1, // max temp column
                rainIndex = -1,// rainfall column
             periodIndex = -1; // period of rainfall (days) column

            bool stationExists = false;

            if (file.ContentLength > 0)
            {
                var db = new Models.BureauDBEntities();

                string FileName = Path.GetFileName(file.FileName);
                path = Path.Combine(Server.MapPath("~/UploadedFiles"), FileName);
                file.SaveAs(path);

                var allLines = System.IO.File.ReadLines(path);
                List<string> colTitles = new List<string>();
                int row = 0;

                foreach (var line in allLines)
                {
                   // System.Diagnostics.Debug.WriteLine(line);
                    var values = line.Split(',');

                    // header row
                    if (row == 0)
                    {
                        for (var c = 0; c < values.Length; c++)
                        {
                            colTitles.Add(values[c]);
                   //         System.Diagnostics.Debug.WriteLine("[Index: " + c + " " + values[c] + "]");
                        }

                        stationIndex = getColIndex(colTitles, "station", search.contains);
                        yearIndex = getColIndex(colTitles, "year");
                        monthIndex = getColIndex(colTitles, "month");
                        dayIndex = getColIndex(colTitles, "day");

                        minIndex = getColIndex(colTitles, "minimum");
                        maxIndex = getColIndex(colTitles, "maximum");

                        rainIndex = getColIndex(colTitles, "rainfall");
                        periodIndex = getColIndex(colTitles, "period");

                        

                    }
                   
                    else 
                    { 
                        // else capture the data  
                        try
                        {
                            var stationNumber = Convert.ToInt32(values[stationIndex]);
                            var year = Convert.ToInt32(values[yearIndex]);
                            var month = Convert.ToInt32(values[monthIndex]);
                            var day = Convert.ToInt32(values[dayIndex]);
                            var newRecord = false;

                            //chec our station exists for the first data
                            if (row == 1)
                            {
                                stationExists = Models.StationCheck.exists(stationNumber, db);
                                if(!stationExists) { throw new Exception("Station Not Found!"); }
                            }

                            // only process data from 2000 or newer.
                            if (year >= 2000)
                            {
                                // try to find existing record to update
                                var bomData = db.DailyDatas.Find(stationNumber, year, month, day);

                                // if not found, create a new object
                                if (bomData == null)
                                {
                                    bomData = new Models.DailyData();
                                    bomData.stationNumber = stationNumber;
                                    bomData.year = year;
                                    bomData.month = month;
                                    bomData.day = day;
                                    newRecord = true;
                                }

                                if (maxIndex >= 0)
                                {
                                    bomData.maxTemp = getDecimal(values[maxIndex]);
                                }
                                else if (minIndex >= 0)
                                {
                                    bomData.minTemp = getDecimal(values[minIndex]);
                                }
                                if (rainIndex >= 0)
                                {
                                    bomData.rainfall = getDecimal(values[rainIndex]);
                                }
                                if (periodIndex >= 0)
                                {
                                    bomData.rainDays = getInt(values[periodIndex]);
                                }

                                // if record doesn't exist, update appropriate column
                                if (newRecord)
                                {

                                    db.DailyDatas.Add(bomData);
                                }

                                // save changes
                                db.SaveChanges();
                            }// end year > 1980
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }

                        //  System.Diagnostics.Debug.Write(values[c] + "\t");

                    } // end loop through cols
                    row++;
                   // System.Diagnostics.Debug.WriteLine("");
                }
            }
        }

        private static decimal? getDecimal(string value)
        {
            decimal? typedVal = null;
            if (!("null".Equals(value) || string.IsNullOrEmpty(value)))
            {
                typedVal = Convert.ToDecimal(value);
            }
            return typedVal;
        }

        private static int? getInt(string value)
        {
            int? typedVal = null;
            if (!("null".Equals(value) || string.IsNullOrEmpty(value)))
            {
                typedVal = Convert.ToInt32(value);
            }
            return typedVal;
        }

        private static int getColIndex(List<string> colTitles, string colText, search searchType = search.startswith)
        {
            int index = -1;
            if (searchType == search.contains) { index = colTitles.FindIndex(x => x.ToLower().Contains(colText)); }
            else { index = colTitles.FindIndex(x => x.ToLower().StartsWith(colText)); }
            return index;
        }

    }
}