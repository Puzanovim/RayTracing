using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer
{
    public class Point
    {
        public float x;
        public float y;
        public float z;

        public Point(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float norm()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public Point normalize()
        {
            float norm = this.norm();
            x = x / norm;
            y = y / norm;
            z = z / norm;
            return this;
        }


        public static Point operator +(Point point1, Point point2)
        {
            return new Point(
                x: point1.x + point2.x,
                y: point1.y + point2.y,
                z: point1.z + point2.z);
        }
        public static Point operator -(Point point1, Point point2)
        {
            return new Point(
                x: point1.x - point2.x,
                y: point1.y - point2.y,
                z: point1.z - point2.z);
        }
        public static float operator *(Point point1, Point point2)
        {
            float x = point1.x * point2.x;
            float y = point1.y * point2.y;
            float z = point1.z * point2.z;
            return x + y + z;
        }

        public static Point operator *(Point point, float scalar)
        {
            float x = point.x * scalar;
            float y = point.y * scalar;
            float z = point.z * scalar;
            return new Point(x, y, z);
        }
        public static Point operator /(Point point1, Point point2)
        {
            float x = point1.x / point2.x;
            float y = point1.y / point2.y;
            float z = point1.z / point2.z;
            return new Point(x: x, y: y, z: z);
        }
    }
}
