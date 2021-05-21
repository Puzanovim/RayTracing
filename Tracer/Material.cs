using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Material
    {
        public Point diffuse_color;  // рассеянный цвет, по сути основной цвет материала
        public Point4 albedo;  // отражательная способность
        public float specular_exponent;  // показатель отражение, говорит о силе отражения. Чем больше, тем более блестящая поверхность
        public float refractive_index;  // показатель преломления
        public Material()
        {
            this.albedo = new Point4(1, 0, 0, 0);
            this.diffuse_color = new Point();
            this.specular_exponent = 0;
            this.refractive_index = 1;
        }

        public Material(float r_index, Point4 albedo, Point color, float spec)
        {
            this.albedo = albedo;
            this.diffuse_color = color;
            this.specular_exponent = spec;
            this.refractive_index = r_index;
        }        
    }
}
