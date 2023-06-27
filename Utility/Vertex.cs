using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DRS
{
    public class Vertex
    {
        public Vector3 pos;
        public int index;

        public Vertex(Vector3 pos, int index)
        {
            this.pos = pos;
            this.index = index;
        }
    }

    public static class VerticesExtensions
    {
        public static Vertex NewVertex(this List<Vertex> list, Vector3 pos)
        {
            Vertex result = new Vertex(pos, list.Count);
            list.Add(result);
            return result;
        }

        public static Vector3[] ListToVectorArray(this List<Vertex> list)
        {
            Vector3[] result = new Vector3[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i].pos;
            return result;
        }

        public static Vertex AddLast(this List<Vertex> list, List<Vertex> origin)
        {
            list.Add(origin[^1]);
            return origin[^1];
        }

        class VertexToPlane
        {
            public Vertex vertex;
            public Vector2 planePos;

            public VertexToPlane(Vertex vertex, Vector3 up)
            {
                this.vertex = vertex;
                planePos = new Vector2(Vector3.ProjectOnPlane(vertex.pos, up).x, Vector3.ProjectOnPlane(vertex.pos, up).z);
            }
        }
        
        public static void SortClockwise(this Vertex[] array, Vector3 up)
        {                                             
            List<VertexToPlane> points = new List<VertexToPlane>();
            for (int i = 0; i < array.Length; i++)
                points.Add(new VertexToPlane(array[i], up));

            Vector2 triangleCenter = new Vector2((points[0].planePos.x + points[1].planePos.x + points[2].planePos.x) / 3, (points[0].planePos.y + points[1].planePos.y + points[2].planePos.y) / 3);

            points = points.OrderBy(x => Math.Atan2(x.planePos.x - triangleCenter.x, x.planePos.y - triangleCenter.y)).ToList();
            for (int i = 0; i < array.Length; i++)
                array[i] = points[i].vertex;
        }
    }    
}