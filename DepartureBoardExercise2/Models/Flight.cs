using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartureBoardExercise2.Models
{
    public class Flight
    {
        public string FlightNum { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Gate { get; set; } = "-";
        public string Airline { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}