using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightHack
{
    public class Globals
    {
        public enum Status
        {
            Initial,
            InQueue,
            InProgress,
            Completed,
            Failed
        }
    }
}
