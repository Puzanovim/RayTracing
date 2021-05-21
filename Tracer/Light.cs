using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Light
    {
        public Point position; // позиция источника
        public float intensity; // интенсивность света
        public Light(Point position, float intensity)
        {
            this.position = position;
            this.intensity = intensity;
        }
    }
}
