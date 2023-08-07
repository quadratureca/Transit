using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitWorker.Models
{

    // Real Time Positions
    // https://retro.umoiq.com/service/publicJSONFeed?command=vehicleLocations&a=ttc&t=0
    // Static Data
    // http://opendata.toronto.ca/toronto.transit.commission/ttc-routes-and-schedules/OpenData_TTC_Schedules.zip


    public class TTCMessage
    {
        public Lasttime lastTime { get; set; }
        public string copyright { get; set; }
        public Vehicle[] vehicle { get; set; }
    }

    public class Lasttime
    {
        public string time { get; set; }
    }

    public class Vehicle
    {
        public string routeTag { get; set; }
        public string predictable { get; set; }
        public string heading { get; set; }
        public string speedKmHr { get; set; }
        public string lon { get; set; }
        public string id { get; set; }
        public string dirTag { get; set; }
        public string lat { get; set; }
        public string secsSinceReport { get; set; }
    }


}
