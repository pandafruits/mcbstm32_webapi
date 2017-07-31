using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace McbWebApi.Request
{
    public class EnvironmentalRequest
    {
        [Required]
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        [Range(1, 1440)]
        public int? IntervalMinutes { get; set; }
    }
}