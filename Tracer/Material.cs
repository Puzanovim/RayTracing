using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Material
    {
        public Point diffuse_color;
        public Point albedo;
        public float specular_exponent;
        // public float refractive_index;
        public Material()
        {
            this.albedo = new Point(1, 0, 0);
            this.diffuse_color = new Point();
            this.specular_exponent = 0;
            // this.refractive_index = 1;
        }

        public Material(Point albedo, Point color, float spec)
        {
            this.albedo = albedo;
            this.diffuse_color = color;
            this.specular_exponent = spec;
            // this.refractive_index = r_index;
        }        
    }
}
