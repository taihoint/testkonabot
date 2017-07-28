using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application1.Models
{
    public class TestDriveLuisResult
    {
        public string intent { get; set; }
        public string entity { get; set; }
        public string entity_value { get; set; }
    }
}