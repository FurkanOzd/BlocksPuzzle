using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelBuilder : MonoBehaviour
{

    public int boardSize;
    int[,] vertices;
    public int pieceCount;
    [SerializeField] GameObject pointInstance;
    [SerializeField] Transform plane;
    int initialSize = 4;
    List<Triangle> triangles;
    [SerializeField] GameObject meshInstance;

    class Shape
    {
        public Color color;
        Mesh mainMesh;
        List<MeshFilter> triangleMeshes;
        Vector3 pos;
        public Shape(Color color, Vector3 pos)
        {
            this.color = color;
            this.pos = pos;
            triangleCount = 0;
            triangleMeshes = new List<MeshFilter>();
        }
        public void AddTriangleToShape(MeshFilter mesh)
        {
            triangleMeshes.Add(mesh);
            triangleCount++;
        }
        public Vector3 GetPos()
        {
            return pos;
        }
        public int triangleCount;

        public Mesh GetMesh()
        {
            mainMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[triangleMeshes.Count];

            for (int i = 0; i < combine.Count(); i++)
            {
                combine[i].mesh = triangleMeshes[i].mesh;
                combine[i].transform = triangleMeshes[i].transform.localToWorldMatrix;

            }
            mainMesh.CombineMeshes(combine);

            return mainMesh;
        }
    }
    class Triangle
    {
        GameObject triangleObject;
        Vector3[] vertices;
        public Vector3 centerPoint;
        int[] indexes;
        public Mesh mesh;
        public MeshFilter filter = null;
        public Triangle(Vector3[] vertices, int[] indexOrder, Material mat)
        {
            this.vertices = vertices;
            float centerPointX = 0;
            float centerPointY = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                centerPointX += vertices[i].x;
                centerPointY += vertices[i].y;
            }
            centerPoint = new Vector3(centerPointX / 3, centerPointY / 3, 0f);
            indexes = indexOrder;

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indexes;
            triangleObject = new GameObject();
            filter = triangleObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = triangleObject.AddComponent<MeshRenderer>();
            renderer.material = mat;
            filter.mesh = mesh;
            triangleObject.SetActive(false);
        }
        public Vector3[] GetVertices()
        {
            return vertices;
        }
        public int[] GetIndexes()
        {
            return indexes;
        }

        public MeshFilter GetMeshFilter()
        {
            return triangleObject.GetComponent<MeshFilter>();
        }
    }

    private void Awake()
    {
        boardSize = boardSize + 1;
    }


    void Start()
    {
        InitBoard();
    }

    void InitBoard()
    {
        vertices = new int[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                vertices[i, j] = 0;
            }
        }

        Vector3 startPoint = plane.GetChild(0).position;     // Init Board Points
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Instantiate(pointInstance, startPoint + transform.right * ((float)initialSize / boardSize) * j + transform.up * ((float)initialSize / boardSize) * -i, Quaternion.identity, transform);
            }
        }

        triangles = new List<Triangle>();
        for (int i = 0; i < transform.childCount - boardSize; i++)  //Create Triangles Based On Grid
        {
            if (i % boardSize == boardSize - 1)
            {
                continue;
            }
            Vector3 p1 = transform.GetChild(i).position;
            Vector3 p2 = transform.GetChild(i + 1).position;
            Vector3 p3 = (transform.GetChild(i + 1).position + transform.GetChild(i + boardSize).position) / 2;
            Material mat = meshInstance.GetComponent<MeshRenderer>().sharedMaterial;
            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, mat));

            p1 = transform.GetChild(i).position;
            p2 = transform.GetChild(i + boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 1, 0, 2 }, mat));

            p1 = transform.GetChild(i + 1).position;
            p2 = transform.GetChild(i + 1 + boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, mat));

            p1 = transform.GetChild(i + boardSize).position;
            p2 = transform.GetChild(i + boardSize + 1).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 2, 1 }, mat));

        }

        Shape[] shapes = new Shape[pieceCount];
        int[] randomPositions = new int[pieceCount];

        for (int i = 0; i < randomPositions.Length; i++)
        {
            int randNum = Random.Range(0, transform.childCount);
            while (randomPositions.Contains(randNum))
            {
                randNum = Random.Range(0, transform.childCount);
            }
            randomPositions[i] = randNum;
        }

        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i] = new Shape(new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255),1), transform.GetChild(randomPositions[i]).position);
        }


        for (int i = 0; i < triangles.Count; i++)                     //CreateRandomShapes;
        {
            float dist = Mathf.Infinity;
            Shape selectedShape = null;
            for (int j = 0; j < shapes.Length; j++)
            {
                if (dist > Vector3.Distance(triangles[i].centerPoint, shapes[j].GetPos()))
                {
                    dist = Vector3.Distance(triangles[i].centerPoint, shapes[j].GetPos());
                    selectedShape = shapes[j];
                }
            }
            if (triangles[i].filter != null)
                selectedShape.AddTriangleToShape(triangles[i].GetMeshFilter());
        }

        int temp = 0;

        for (int i = 0; i < shapes.Length; i++)
        {
            GameObject newObject = Instantiate(meshInstance, new Vector3(0, 0, -1), Quaternion.identity);
            newObject.GetComponent<MeshFilter>().mesh = shapes[i].GetMesh();

            newObject.GetComponent<MeshRenderer>().material.SetColor("Standard", shapes[i].color);
        }

    }

}
