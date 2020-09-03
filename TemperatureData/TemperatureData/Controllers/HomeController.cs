using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Assignment2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult UploadDailyData()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UploadDailyData(HttpPostedFileBase file)
        {
            try
            {
                CSVReader.readData(file, Server);
                ViewBag.Message = "File Uploaded Successfully!!";
                return View("Success");
            }
            catch(Exception ex)
            {
                ViewBag.Message = "File Uploaded Failed:";
                ViewBag.Message += Environment.NewLine;
                ViewBag.Message += ex.Message;
                return View("Failure");
            }
            //return View();
        }


        public ActionResult Index()
        {
            return View();
        }

       
    }
}