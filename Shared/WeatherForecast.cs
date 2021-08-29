using JLio.Core.Models;
using JLio.Client;
using System;
using Newtonsoft.Json.Linq;

namespace JLioOnline.Shared
{
    public class WeatherForecast
    {
        public WeatherForecast()
        {
           
        }
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
