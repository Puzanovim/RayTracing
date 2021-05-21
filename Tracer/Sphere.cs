using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Sphere
    {
        public Point center;  // координата центра
        public float radius;  // радиус сферы
        public Material material;  // материал сферы

        public Sphere(Point center, float radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }


        public bool Ray_intersect(ref Point orig, ref Point dir, ref float t0)
        {
            // пересечение лучей
            Point L = this.center - orig;
            float tca = L * dir;
            float d2 = L * L - tca * tca;
            if (d2 > this.radius * this.radius) return false;
            float thc = (float)Math.Sqrt(this.radius * this.radius - (float)d2);
            t0 = tca - thc;
            float t1 = tca + thc;
            if (t0 < 0) t0 = t1;
            if (t0 < 0) return false;
            return true;
        }
    }
}
