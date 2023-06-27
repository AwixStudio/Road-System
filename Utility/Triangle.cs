using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRS
{
    public struct Triangle
    {
        public int a;
        public int b;
        public int c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        
        public Triangle(Vertex[] vertices, Vector3 up)
        {            
            vertices.SortClockwise(up);
            
            this.a = vertices[0].index;
            this.b = vertices[1].index;
            this.c = vertices[2].index;
        }

        public Triangle Reverse() => new Triangle(c, b, a);
    }

    public static class TrianglesExtensions
    {
        public static int[] ListToIntArray(this List<Triangle> list)
        {
            int[] result = new int[list.Count * 3];
            for (int i = 0, j = 0; j < list.Count; j++)
            {
                result[i++] = list[j].a;
                result[i++] = list[j].b;
                result[i++] = list[j].c;
            }

            return result;
        }
    }
}

