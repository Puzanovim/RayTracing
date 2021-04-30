using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tracer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Graphics graphics;

        public void Render(int width, int height, List<Sphere> spheres, List<Light> lights)
        {
            const float fov = (float)Math.PI / (float)3.0;

            List<Point> framebuffer = new List<Point>(width * height);
            for(int i = 0; i < width * height; i++)
            {
                framebuffer.Add(new Point(1, 1, 1));
            }
            Point camera = new Point(0, 0, 0);


            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    float x = (2 * (i + 0.5f) / (float)width - 1) * (float)Math.Tan(fov / 2.0) * width / (float)height;
                    float y = (2 * (j + 0.5f) / (float)height - 1) * (float)Math.Tan(fov / 2.0);
                    Point dir = new Point(x, y, -1).normalize();
                    framebuffer[i + j * width] = Cast_ray(ref camera, ref dir, ref spheres, ref lights);
                    // framebuffer[i + j * width] = new Point(j / (float)height, i / (float)width, 0);  градиент для теста

                    var point = framebuffer[i + j * width] * 255;
                    Color color = Color.FromArgb(correct_color(point.x), correct_color(point.y), correct_color(point.z));
                    graphics.DrawRectangle(new Pen(color), i, j, 1.0f, 1.0f);
                }
            }
        }

        private int correct_color(float color)
        {
            return (int)Math.Min(Math.Max(color, 0f), 255f);
        }

        private bool Scene_intersect(ref Point orig, ref Point dir, ref List<Sphere> spheres, ref Point hit, ref Point N, ref Material material) {
            float spheres_dist = float.MaxValue;

            for (int i = 0; i < spheres.Count; i++) {
                float dist_i = 0;
                if (spheres[i].ray_intersect(ref orig, ref dir, ref dist_i) && dist_i < spheres_dist) {
                    spheres_dist = dist_i;
                    hit = orig + dir * dist_i;
                    N = (hit - spheres[i].center).normalize();
                    material = spheres[i].material;
                }
            }
            return spheres_dist < 1000;
        }

        private Point reflect(Point I, ref Point N)
        {
            return I - N * 2f * (I * N);
        }

        private Point Cast_ray(ref Point camera, ref Point dir, ref List<Sphere> spheres, ref List<Light> lights, int depth = 0)
        {
            // вычисляем цвет для каждого пикселя

            Point point = new Point(0, 0, 0);
            Point N = new Point(0, 0, 0);
            Material material = new Material();

            if (depth > 4 || !Scene_intersect(ref camera, ref dir, ref spheres, ref point, ref N, ref material))
            {
                return new Point((float)0.2, (float)0.7, (float)0.8); // background color
            }

            Point reflect_dir = reflect(dir, ref N).normalize();
            Point reflect_orig = reflect_dir * N < 0 ? point - N * (float)1e-3 : point + N * (float)1e-3; // offset the original point to avoid occlusion by the object itself
            Point reflect_color = Cast_ray(ref reflect_orig, ref reflect_dir, ref spheres, ref lights, depth + 1);

            float diffuse_light_intensity = 0;
            float specular_light_intensity = 0;
            for (int i = 0; i < lights.Count; i++)
            {
                Point light_dir = (lights[i].position - point).normalize();
                float light_distance = (lights[i].position - point).norm();

                Point shadow_orig = light_dir * N < 0 ? point - N * (float)1e-3 : point + N * (float)1e-3; // checking if the point lies in the shadow of the lights[i]
                Point shadow_pt = new Point(), shadow_N = new Point();
                Material tmpmaterial = new Material();
                if (Scene_intersect(ref shadow_orig, ref light_dir, ref spheres, ref shadow_pt, ref shadow_N, ref tmpmaterial) && (shadow_pt - shadow_orig).norm() < light_distance)
                    continue;

                diffuse_light_intensity += lights[i].intensity * Math.Max(0f, light_dir * N);
                specular_light_intensity += (float)Math.Pow(Math.Max(0f, (reflect((light_dir * -1), ref N) * -1) * dir), material.specular_exponent) * lights[i].intensity;
            }

            // return material.diffuse_color * diffuse_light_intensity;
            return material.diffuse_color * diffuse_light_intensity * material.albedo.x + new Point(1, 1, 1) * specular_light_intensity * material.albedo.y + reflect_color * material.albedo.z;
        }

        private void render_Click(object sender, EventArgs e)
        {
            graphics = pictureBox1.CreateGraphics();
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;

            Material ivory = new Material(new Point(0.6f, 0.3f, 0.1f), new Point(0.4f, 0.4f, 0.3f), 50f);
            Material red_rubber = new Material(new Point(0.9f, 0.1f, 0.0f), new Point(0.3f, 0.1f, 0.1f), 10f);
            Material mirror = new Material(new Point(0.0f, 10.0f, 0.8f), new Point(1.0f, 1.0f, 1.0f), 1425f);

            List<Sphere> spheres = new List<Sphere>();
            spheres.Add(new Sphere(new Point(-3f, 0f, -16f), 2, ivory));
            spheres.Add(new Sphere(new Point(-1.0f, -1.5f, -12f), 2, mirror));
            spheres.Add(new Sphere(new Point(1.5f, -0.5f, -18f), 3, red_rubber));
            spheres.Add(new Sphere(new Point(7f, 5f, -18f), 4, mirror));

            List<Light> lights = new List<Light>();
            lights.Add(new Light(new Point(-20f, -20f, 20f), 2f));
            Render(width, height, spheres, lights);
        }
    }
}
