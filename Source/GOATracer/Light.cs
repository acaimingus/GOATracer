using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer
{
    public class Light
    {
        public string name { get; set; }
        public bool isEnabled { get; set; }

        public double LightPositionX { get; set; }   
        public double LightPositionY { get; set; }   
        public double LightPositionZ { get; set; }
    }
}
