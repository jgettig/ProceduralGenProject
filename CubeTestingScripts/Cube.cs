using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public Vector3 pos;

    public Mesh mesh;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    public int lastVertex;
    public bool transparent = false;
    public bool empty = false;

    public void Init(Material m, bool _empty, bool _transparent) {
        pos = Vector3.zero;
        empty = _empty;
        if (empty && transparent) transparent = false;
        transparent = _transparent;
        GetComponent<MeshRenderer>().material = m;
    }

    public void Draw(Cube[] adjacents)
    {
        
        if (!empty) {
            //initialize mesh
            mesh = new Mesh();

            //create mesh data
            DrawCube(adjacents);

            //set mesh data
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.SetUVs(0, uvs.ToArray());

            //recalculate lighting
            mesh.RecalculateNormals();

            //set mesh
            GetComponent<MeshFilter>().mesh = mesh;
        }

    }

    private void DrawCube(Cube[] adjacents) {
        if (adjacents[1] == null || adjacents[1].empty || !transparent && adjacents[1].transparent) Front_GenerateFace();
        if (adjacents[3] == null || adjacents[3].empty || !transparent && adjacents[3].transparent) Back_GenerateFace();
        if (adjacents[4] == null || adjacents[4].empty || !transparent && adjacents[4].transparent) Right_GenerateFace();
        if (adjacents[2] == null || adjacents[2].empty || !transparent && adjacents[2].transparent) Left_GenerateFace();
        if (adjacents[0] == null || adjacents[0].empty || !transparent && adjacents[0].transparent) Top_GenerateFace();
        if (adjacents[5] == null || adjacents[5].empty || !transparent && adjacents[5].transparent) Bottom_GenerateFace();
    }

    private void Front_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos); //0
        vertices.Add(pos + Vector3.up); //1
        vertices.Add(pos + Vector3.up + Vector3.right); //2
        vertices.Add(pos + Vector3.right); //3

        AddFaceTriangles();
    }

    private void Back_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos + Vector3.forward + Vector3.right); //0
        vertices.Add(pos + Vector3.forward + Vector3.up + Vector3.right); //1
        vertices.Add(pos + Vector3.forward + Vector3.up); //2
        vertices.Add(pos + Vector3.forward); //3

        AddFaceTriangles();
    }

    private void Right_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos + Vector3.right); //0
        vertices.Add(pos + Vector3.right + Vector3.up); //1
        vertices.Add(pos + Vector3.right + Vector3.up + Vector3.forward); //2
        vertices.Add(pos + Vector3.right + Vector3.forward); //3

        AddFaceTriangles();
    }

    private void Left_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos + Vector3.forward); //0
        vertices.Add(pos + Vector3.forward + Vector3.up); //1
        vertices.Add(pos + Vector3.up); //2
        vertices.Add(pos); //3

        AddFaceTriangles();
    }

    private void Top_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos + Vector3.up); //0
        vertices.Add(pos + Vector3.up + Vector3.forward); //1
        vertices.Add(pos + Vector3.up + Vector3.forward + Vector3.right); //2
        vertices.Add(pos + Vector3.up + Vector3.right); //3

        AddFaceTriangles();
    }

    private void Bottom_GenerateFace() {
        lastVertex = vertices.Count;

        //declare vertices in clockwise order as viewer facing face
        vertices.Add(pos + Vector3.forward); //0
        vertices.Add(pos); //1
        vertices.Add(pos + Vector3.right); //2
        vertices.Add(pos + Vector3.right + Vector3.forward); //3

        AddFaceTriangles();
    }

    private void AddFaceTriangles() {
        //first triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex+2);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+3);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
