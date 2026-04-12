using DV.PointSet;
using UnityEngine;

namespace Signals.Game.Util
{
    internal class GLHelper
    {
        private static Material? s_mat;
        public static Material LineMaterial
        {
            get
            {
                if (s_mat == null)
                {
                    s_mat = new Material(Shader.Find("Hidden/Internal-Colored"));
                }

                return s_mat;
            }
        }

        public static void DrawBezierCurve(BezierCurve curve, int resolution, Color c)
        {
            float step = 1.0f / resolution;

            LineMaterial.SetPass(0);
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c);
            GL.Vertex(curve.GetPointAt(0));

            for (float f = step; f < 1; f += step)
            {
                GL.Vertex(curve.GetPointAt(step));
            }

            GL.Vertex(curve.GetPointAt(1));
            GL.End();
        }

        public static void DrawPointSet(EquiPointSet.Point[] points, Vector3 offset, int step, Color c)
        {
            LineMaterial.SetPass(0);
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c);

            for (int i = 0; i < points.Length; i += step)
            {
                GL.Vertex((Vector3)points[i].position + offset);
            }

            GL.Vertex((Vector3)points[points.Length - 1].position + offset);
            GL.End();
        }

        public static void DrawLine(Vector3 a, Vector3 b, Color c)
        {
            LineMaterial.SetPass(0);
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c);
            GL.Vertex(a);
            GL.Vertex(b);
            GL.End();
        }

        public static void DrawLozenge(Vector3 position, Vector3 forward, Color c)
        {
            var cross = Vector3.Cross(forward, Vector3.up) * 0.5f;

            LineMaterial.SetPass(0);
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c);
            GL.Vertex(position + Vector3.up);
            GL.Vertex(position + cross);
            GL.Vertex(position - Vector3.up);
            GL.Vertex(position - cross);
            GL.Vertex(position + Vector3.up);
            GL.End();
        }
    }
}
