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
            // создаем переменную fov = field of view - угол видимости, задаем ему значение в 60 градусов, то есть 1.05 радиан.
            // const float fov = (float)Math.PI / (float)3.0;  // вариант, задания значения через Пи
            const float fov = 1.05f;

            // лист точек -- наш экран. Инициализируем его, заполняя черным цветом.
            List<Point> framebuffer = new List<Point>(width * height);
            for(int i = 0; i < width * height; i++)
            {
                framebuffer.Add(new Point(1, 1, 1));
            }
            // позиция камеры
            Point camera = new Point(0, 0, 0);


            // проходимся по нашему экрану и рисуем каждый пиксель
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    // вычисляем координаты точки на экране
                    float dir_x = (float)(i + 0.5) - width / 2f;
                    float dir_y = (float)-(j + 0.5) + height / 2f;
                    float dir_z = -height / (2f * (float)Math.Tan(fov / 2f));

                    Point dir = new Point(dir_x, dir_y, dir_z).normalize();  // находим направление к данной точке на экране
                    framebuffer[i + j * width] = Cast_ray(ref camera, ref dir, ref spheres, ref lights);  // получаем цвет данной точки
                    // framebuffer[i + j * width] = new Point(j / (float)height, i / (float)width, 0);  // градиент для теста

                    var point = framebuffer[i + j * width] * 255;  // умножаем каждую координату на 255, получаем значение цвета RGB
                    Color color = Color.FromArgb(Correct_color(point.x), Correct_color(point.y), Correct_color(point.z));  // создаем цвет, полученный из наших координат
                    graphics.DrawRectangle(new Pen(color), i, j, 1.0f, 1.0f);  // рисуем точку на экране
                }
            }
        }

        private int Correct_color(float color)
        {
            // функция корректирует цвет, чтобы он не выходил за границы диапазона от 0 до 255
            return (int)Math.Min(Math.Max(color, 0f), 255f);
        }

        private bool Scene_intersect(ref Point orig, ref Point dir, ref List<Sphere> spheres, ref Point hit, ref Point N, ref Material material) {
            float spheres_dist = float.MaxValue;  // расстояние до ближайшей сферы

            for (int i = 0; i < spheres.Count; i++) {
                float dist_i = 0;  // расстояние до точки пересечения со сферой
                if (spheres[i].Ray_intersect(ref orig, ref dir, ref dist_i) && dist_i < spheres_dist) {
                    spheres_dist = dist_i;  // обновляем расстояние до ближайшей сферы
                    hit = orig + dir * dist_i;  // получаем точку пересечения со сферой
                    N = (hit - spheres[i].center).normalize();  // нормаль в данной точке
                    material = spheres[i].material;  // сохраняем материал данной точки
                }
            }

             float checkerboard_dist = float.MaxValue;  // расстояние до плоскости
            // здесь создается плоскость. Узначть, почему черная. 
            if (Math.Abs(dir.y) > 1e-3)
            {
                float d = -(orig.y + 15) / dir.y; // the checkerboard plane has equation y = -15
                Point pt = orig + dir * d;
                if (d > 0 && Math.Abs(pt.x) < 10 && pt.z < -10 && pt.z > -30 && d < spheres_dist)
                {
                    checkerboard_dist = d;
                    hit = pt;
                    N = new Point(0, 1, 0);
                    material.diffuse_color = new Point(0, 0, 1);
                    // material.diffuse_color = (((int)(0.5 * hit.x + 1000) + (int)(0.5 * hit.z)) & 1) != 0 ? new Point(1f, 1f, 1f) : new Point(1f, 0.7f, 0.3f);
                    // material.diffuse_color = material.diffuse_color * 0.3f;
                }
            }
            return Math.Min(spheres_dist, checkerboard_dist) < 1000;
        }

        // считаем лучь преломления по закону Снеллиуса
        private Point Refract(ref Point I, ref Point N, ref float refractive_index)
        {
            float cosi = -Math.Max(-1f, Math.Min(1f, I * N));
            float refractive_index_out = 1, refractive_index_in = refractive_index;
            Point n = N;
            if (cosi < 0)
            {
                cosi = -cosi; 
                var copy = refractive_index_out;
                refractive_index_out = refractive_index_in;
                refractive_index_in = copy;
                n = N * -1;
            }
            float refractive_relation = refractive_index_out / refractive_index_in;
            float k = 1 - refractive_relation * refractive_relation * (1 - cosi * cosi);
            return k < 0 ? new Point(0, 0, 0) : I * refractive_relation + n * (float)(refractive_relation * cosi - Math.Sqrt(k));


            // return I + (Math.Sqrt(((I.norm)/((I * N) *(I * N))) + 1) - 1) * I * N * N;
        }


        // формула луча отражения
        private Point Reflect(ref Point I, ref Point N)  // получает луч I и нормаль N, возвращает отраженный луч I
        {
            return N * 2f * (I * N) - I;
        }

        private Point Cast_ray(ref Point camera, ref Point dir, ref List<Sphere> spheres, ref List<Light> lights, int depth = 0)
        {
            // вычисляем цвет для каждого пикселя

            Point point = new Point(0, 0, 0);  // текущая точка
            Point N = new Point(0, 0, 0);  // нормаль в текущей точке
            Material material = new Material();  // материал текущей точки

            // Если нет пересечения со сценой или глубина отражения больше 4
            if (depth > 4 || !Scene_intersect(ref camera, ref dir, ref spheres, ref point, ref N, ref material))
            {
                return new Point((float)0.2, (float)0.7, (float)0.8); // background color
            }

            Point reflect_dir = Reflect(ref dir, ref N).normalize();  // получаем вектор направления отражения
            Point refract_dir = Refract(ref dir, ref N, ref material.refractive_index).normalize();  // получаем вектор направления преломления
            Point reflect_orig = reflect_dir * N < 0 ? point - N * (float)1e-3 : point + N * (float)1e-3;  // двигаем точку на маленькую величину в сторону нормали
            Point refract_orig = refract_dir * N < 0 ? point - N * (float)1e-3 : point + N * (float)1e-3;  // двигаем точку как и для отражения
            Point reflect_color = Cast_ray(ref reflect_orig, ref reflect_dir, ref spheres, ref lights, depth + 1);  // получаем цвет отражения в данной точке, запустив CastRay с reflect_orig, reflect_dir и depth + 1
            Point refract_color = Cast_ray(ref refract_orig, ref refract_dir, ref spheres, ref lights, depth + 1);  // получаем цвет преломления в данной точке, запустив CastRay с refract_orig, refract_dir и depth + 1

            float diffuse_light_intensity = 0;  // диффузная (рассеянная) яркость света
            float specular_light_intensity = 0; // зеркальная яркость света

            // проходимся по каждому источнику света
            for (int i = 0; i < lights.Count; i++)
            {
                Point light_dir = (lights[i].position - point).normalize();  // получаем направление до источника света
                float light_distance = (lights[i].position - point).norm();  // получаем расстояние до источника света

                Point shadow_orig = light_dir * N < 0 ? point - N * (float)1e-3 : point + N * (float)1e-3;  // двигаем точку на маленькую величину в сторону нормали, чтобы она не оставляла тень на себя
                Point shadow_pt = new Point(), shadow_N = new Point();  // точка, которая пускает тень, и нормаль этой точки
                Material tmpmaterial = new Material();  // материал для тени

                // проверяем, если между точкой и лучом есть объект, то здесь будет тень, значит свет добавлять не нужно и мы переходим к следующему источнику света
                if (Scene_intersect(ref shadow_orig, ref light_dir, ref spheres, ref shadow_pt, ref shadow_N, ref tmpmaterial) && (shadow_pt - shadow_orig).norm() < light_distance)
                    continue;

                diffuse_light_intensity += lights[i].intensity * Math.Max(0f, light_dir * N);  // добавляем свет в общую осещенность
                // получаем зеркальную яркость света (белую точку, которую мы видим на объекте)
                specular_light_intensity += (float)Math.Pow(Math.Max(0f, (Reflect(ref light_dir, ref N) * -1) * dir), material.specular_exponent) * lights[i].intensity;
            }


            // возвращаем: рассеянный цвет материала на интенсивность света + зеркальную яркость света + отраженный цвет + преломленный цвет
            return material.diffuse_color * diffuse_light_intensity * material.albedo.x + new Point(1, 1, 1) * specular_light_intensity * material.albedo.y + reflect_color * material.albedo.z + refract_color * material.albedo.t;
        }

        private void Render_Click(object sender, EventArgs e)
        {
            // кнопка рендера.
            // создаем графику в picturebox, получаем ширину и высоту
            graphics = pictureBox1.CreateGraphics();
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;

            // создаем материалы наших элементов
            Material ivory = new Material(1f, new Point4(0.6f, 0.3f, 0.1f, 0.0f), new Point(0.4f, 0.4f, 0.3f), 50f);
            Material red_rubber = new Material(1f, new Point4(0.9f, 0.1f, 0.0f, 0.0f), new Point(0.3f, 0.1f, 0.1f), 10f);
            Material mirror = new Material(1f, new Point4(0.0f, 10.0f, 0.8f, 0.0f), new Point(1.0f, 1.0f, 1.0f), 1425f);
            Material glass = new Material(1.5f, new Point4(0.0f, 0.5f, 0.1f, 0.8f), new Point(0.6f, 0.7f, 0.8f), 125f);

            // создаем сферы
            List<Sphere> spheres = new List<Sphere>();
            spheres.Add(new Sphere(new Point(-6f, -3f, -16f), 2, ivory));
            spheres.Add(new Sphere(new Point(-3.5f, -5.5f, -12f), 2, glass));
            spheres.Add(new Sphere(new Point(0f, -4.5f, -18f), 3, red_rubber));
            spheres.Add(new Sphere(new Point(7f, 1f, -18f), 4, mirror));

            // создаем источники света
            List<Light> lights = new List<Light>();
            lights.Add(new Light(new Point(-20f, 20f, 20f), 2f));
            lights.Add(new Light(new Point(30f, 20f, 20f), 2f));

            // вызываем функцию Render, в которую передаем ширину, высоту, свет и объекты.
            Render(width, height, spheres, lights);
        }
    }
}
