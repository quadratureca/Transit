using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitRealtime;
using TransitWorker.Models;

namespace TransitWorker
{
    interface IParser
    {
        void Parse(FeedMessage message);
    }
}
