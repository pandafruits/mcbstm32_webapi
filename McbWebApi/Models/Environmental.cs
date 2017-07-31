using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace McbWebApi.Models
{
    public class Environmental : EnvironmentalRT
    {
        public int id { get; set; }
        public DateTime? acquisition_time { get; set; }
    }
}