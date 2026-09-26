using System.Collections.Generic;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class BeveledPrimitive
    {
        public static Mesh Create(){
            var points=new List<Vector3>();var triangles=new List<int>();const float a=.5f,b=.42f;
            void Face(params Vector3[] face){int start=points.Count;points.AddRange(face);for(int i=1;i<face.Length-1;i++){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}}
            for(int axis=0;axis<3;axis++)foreach(int sign in new[]{-1,1}){
                int u=(axis+1)%3,v=(axis+2)%3;Vector3 P(float x,float y){var p=Vector3.zero;p[axis]=sign*a;p[u]=x;p[v]=y;return p;}
                var face=new[]{P(-b,-b),P(b,-b),P(b,b),P(-b,b)};if(sign<0)System.Array.Reverse(face);Face(face);
            }
            for(int free=0;free<3;free++)foreach(int s in new[]{-1,1})foreach(int t in new[]{-1,1}){
                int u=(free+1)%3,v=(free+2)%3;Vector3 P(float f,float x,float y){var p=Vector3.zero;p[free]=f;p[u]=s*x;p[v]=t*y;return p;}
                var face=new[]{P(-b,a,b),P(b,a,b),P(b,b,a),P(-b,b,a)};var normal=Vector3.Cross(face[1]-face[0],face[2]-face[0]);if(Vector3.Dot(normal,(face[0]+face[2])*.5f)<0)System.Array.Reverse(face);Face(face);
            }
            foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1}){
                var face=new[]{new Vector3(x*a,y*b,z*b),new Vector3(x*b,y*a,z*b),new Vector3(x*b,y*b,z*a)};if(Vector3.Dot(Vector3.Cross(face[1]-face[0],face[2]-face[0]),new Vector3(x,y,z))<0)System.Array.Reverse(face);Face(face);
            }
            var mesh=new Mesh{name="Beveled cube"};mesh.SetVertices(points);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
