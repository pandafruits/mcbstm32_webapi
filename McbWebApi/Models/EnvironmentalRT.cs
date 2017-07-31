using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace McbWebApi.Models
{
    public class EnvironmentalRT
    {
        public double? temperature { get; set; }
        public double? humidity { get; set; }
        public double? core_temperature { get; set; }
        public DateTime? device_time { get; set; }
    }
}