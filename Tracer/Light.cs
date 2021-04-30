using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Light
    {
        public Point position;
        public float intensity;
        public Light(Point position, float intensity)
        {
            this.position = position;
            this.intensity = intensity;
        }
    }
}
