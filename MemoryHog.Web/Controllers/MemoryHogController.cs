using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace MemoryHog.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MemoryHogController : ControllerBase
    {      
        private readonly ILogger<MemoryHogController> _logger;
        private List<XmlNode> memList = new List<XmlNode>();

        public MemoryHogController(ILogger<MemoryHogController> logger)
        {
            _logger = logger;
            var currentMemoryUsage = GC.GetTotalMemory(false);
            _logger.LogInformation($"Initial Memory Usage: {ConvertBytesToMegabytes(currentMemoryUsage).ToString("#.##")} MB");         
        }

        [HttpGet("hog")]
        public void Hog([FromQuery]int limit = 1000, [FromQuery] int sleep = 2000)
        {
            int memoryTotalInMegaBytes = limit;
            int secondsToPause = sleep;

            long runningTotal = GC.GetTotalMemory(false);
            long endingMemoryLimit = Convert.ToInt64(memoryTotalInMegaBytes) * 1000 * 1000;

            _logger.LogInformation("Total Memory To Eat Up: " + limit  + " MB");
            _logger.LogInformation("Pause Between Increments: " + sleep + "ms");
            _logger.LogInformation($"EndingMemoryLimit: {ConvertBytesToMegabytes(endingMemoryLimit).ToString("#.##")} MB");
                        
            while (runningTotal <= endingMemoryLimit)
            {
                XmlDocument doc = new XmlDocument();
                for (int i = 0; i < 1000000; i++)
                {
                    XmlNode x = doc.CreateNode(XmlNodeType.Element, "hello", "");
                    memList.Add(x);
                }
                runningTotal = GC.GetTotalMemory(false);
                _logger.LogInformation($"Increasing memory usage: {ConvertBytesToMegabytes(runningTotal).ToString("#.##")} MB");                
                Thread.Sleep(secondsToPause);
            }
        }
        
        [HttpGet("reset")]
        public void Reset()
        {        
            for (int i = 0; i < memList.Count; i++)
            {
                var currentMemoryUsage = GC.GetTotalMemory(false);
                _logger.LogInformation($"Clearing memory in usage: {ConvertBytesToMegabytes(currentMemoryUsage).ToString("#.##")} MB");
                memList.RemoveAt(i);
                Thread.Sleep(1000);
            }

            memList.Clear();
        }

        [HttpGet("memory")]
        public void Memory()
        {
            var runningTotal = GC.GetTotalMemory(false);
            _logger.LogInformation($"Memory Usage: {ConvertBytesToMegabytes(runningTotal).ToString("#.##")} MB");
        }

        double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }       
    }
}
