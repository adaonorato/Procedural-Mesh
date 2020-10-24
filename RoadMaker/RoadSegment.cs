using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour {
    [SerializeField] Mesh2D shape2D;
    [SerializeField] Transform[] controlPoints = new Transform[4];
    [SerializeField] [Range(0,1)]float t = 0;
    Vector3 GetPos(int i) => controlPoints[i].position;



    [Range(2, 32)] [SerializeField] int edgeRingCount = 8;
    
    Mesh mesh;
    
    void Awake() {
        mesh = new Mesh();
        mesh.name = "Segment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update() => CreateMesh();
    
    void CreateMesh() {
        mesh.Clear();
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        for (int ring = 0; ring < edgeRingCount+1; ring++) {
            float t = ring / (edgeRingCount - 1f);
            OrientPoint op = GetBezierOP(t);
            for (int i = 0; i < shape2D.vertexCount; i++) {
                vertices.Add(op.LocalToWorld(shape2D.vertices[i].point));
                normals.Add(op.LocalToVec(shape2D.vertices[i].normal));
                uvs.Add(new Vector2(shape2D.vertices[i].u,t));
            }
        }

        
        List<int> triIndices = new List<int>();  
        // get index for triangles
        for (int ring = 0; ring < edgeRingCount-1; ring++) {
            int rootIndexRing = ring * shape2D.vertexCount; //pega 0
            int rootIndexNextRing = (ring+1) * shape2D.vertexCount;  //pega o proximo segmento lá da frente, sabe? da rua.
            
            //triangles
            for (int line = 0; line < shape2D.lineCount; line+=2) {
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line+1];
                int currentA = rootIndexRing  + lineIndexA;
                int currentB = rootIndexRing  + lineIndexB;
                int nextA = rootIndexNextRing + lineIndexA;
                int nextB = rootIndexNextRing + lineIndexB;
                {
                    triIndices.Add(currentA);
                    triIndices.Add(nextA);
                    triIndices.Add(nextB);
                    triIndices.Add(currentA);
                    triIndices.Add(nextB);
                    triIndices.Add(currentB);
                } 
            }
        }
        
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0,uvs);
        mesh.SetTriangles(triIndices,0);
        
    }

    
    
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.05f);
            
        }
        
        Handles.DrawBezier(
            GetPos(0)
            ,GetPos(3)
            ,GetPos(1)
            ,GetPos(2),
            Color.cyan, EditorGUIUtility.whiteTexture, 1f);

        OrientPoint testP = GetBezierOP(t);
        Gizmos.color = Color.yellow;
        Handles.PositionHandle(testP.pos, testP.rot);

        void drawPoint(Vector2 localpos) => Gizmos.DrawSphere(testP.LocalToWorld(localpos),0.15f);

        Vector3[] verts  =  shape2D.vertices.Select(v => testP.LocalToWorld(v.point)).ToArray();
        for (int i = 0; i < shape2D.lineIndices.Length; i+=2) {
            Vector3 a = verts[shape2D.lineIndices[i]];
            Vector3 b = verts[shape2D.lineIndices[i+1]];
            
            Gizmos.DrawLine(a,b);
        }
 
        
        
        
    }

   

     OrientPoint GetBezierOP(float t) {
        Vector3 p0  =  GetPos(0);
        Vector3 p1  =  GetPos(1);
        Vector3 p2  =  GetPos(2);
        Vector3 p3  =  GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);
        
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d,e,t);
        Vector3 tangent = (e - d).normalized;

        return new OrientPoint(pos,tangent);
     } 
}
