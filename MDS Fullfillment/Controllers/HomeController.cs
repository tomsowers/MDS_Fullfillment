using MDS_Fullfillment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MDS_Fullfillment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public JsonResult TimeInterfaceData()
        {
            TimeInterfaceJSON timeInterface = new TimeInterfaceJSON(); //gets copy of the time interface class to serialize
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(timeInterface);

            return Json(json);
        }
        [HttpPut]
        public JsonResult TimeData(TimeRecord timeRecord)
        {
            //Add Time Record

            
            DataCon.SQLSubmitTimeData(false, timeRecord);

            return Json(true);
        }
        [HttpPut]
        public JsonResult TimeDataEnd(TimeRecord timeRecord)
        {
            //Add Time Record for end of day button
            DataCon.SQLSubmitTimeData(true, timeRecord);
            return Json(true);
        }
    }
}
