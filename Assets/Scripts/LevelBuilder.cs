using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelBuilder : MonoBehaviour
{
    public int boardSize;
    int _boardSize;
    List<Vector3> gridVertices;
    public int pieceCount;
    [SerializeField] GameObject pointInstance;
    [SerializeField] Transform plane;
    int initialSize = 4;
    List<Triangle> triangles;
    [SerializeField] GameObject meshInstance;
    [SerializeField] Transform shapeCreationPoint;

    Color[] colors = { Color.red, Color.green, Color.yellow, Color.blue, Color.cyan, Color.magenta };

    class Shape
    {
        public Color color;
        Mesh mainMesh;
        List<MeshFilter> triangleMeshes;
        Vector3 pos;
        public Shape(Vector3 pos)
        {
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
            triangleObject.transform.position = Vector3.zero;
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
        public GameObject GetObject()
        {
            return triangleObject;
        }
    }

    private void Awake()
    {
        _boardSize = boardSize + 1;
    }


    void Start()
    {
        InitBoard();
    }

    void InitBoard()
    {
        Vector3 startPoint = plane.GetChild(0).position;     // Init Board Points
        Vector3 endPoint = plane.GetChild(1).position;

        for (int i = 0; i < _boardSize; i++)
        {
            for (int j = 0; j < _boardSize; j++)
            {
                Instantiate(pointInstance, startPoint + transform.right *((endPoint-startPoint).x/(_boardSize-1)) * j + transform.up * ((endPoint - startPoint).y / (_boardSize-1)) * i, Quaternion.identity, transform);
            }
        }

        triangles = new List<Triangle>();
        for (int i = 0; i < transform.childCount - _boardSize; i++)  //Create Triangles Based On Grid
        {
            if (i % _boardSize == _boardSize - 1)
            {
                continue;
            }
            Vector3 p1 = transform.GetChild(i).position;
            Vector3 p2 = transform.GetChild(i + 1).position;
            Vector3 p3 = (transform.GetChild(i + 1).position + transform.GetChild(i + _boardSize).position) / 2;
            Material mat = meshInstance.GetComponent<MeshRenderer>().sharedMaterial;
            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, mat));

            p1 = transform.GetChild(i).position;
            p2 = transform.GetChild(i + _boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 1, 0, 2 }, mat));

            p1 = transform.GetChild(i + 1).position;
            p2 = transform.GetChild(i + 1 + _boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, mat));

            p1 = transform.GetChild(i + _boardSize).position;
            p2 = transform.GetChild(i + _boardSize + 1).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 2, 1 }, mat));

        }
        gridVertices = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++)
        {
            List<Vector3> temp = triangles[i].GetVertices().ToList();
            for(int j = 0; j < temp.Count; j++)
            {
                if (!gridVertices.Contains(temp[j]))
                {
                    gridVertices.Add(temp[j]);
                }
            }
        }

        Shape[] shapes = new Shape[pieceCount];
        int[] randomPositions = new int[pieceCount];

        for (int i = 0; i < randomPositions.Length; i++) //Get Random CenterPoint From Vertices For Shapes
        {
            int randNum = Random.Range(0, triangles.Count);
            while (randomPositions.Contains(randNum))
            {
                randNum = Random.Range(0, triangles.Count);
            }
            randomPositions[i] = randNum;
        }

        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i] = new Shape(triangles[randomPositions[i]].centerPoint);
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

        for (int i = 0; i < shapes.Length; i++)  //Create Shape Objects
        {
            GameObject newObject = new GameObject();
            newObject.name = "Shape" + $"{i}";
            newObject.transform.position = shapeCreationPoint.position + Vector3.right*(endPoint - startPoint).x / shapes.Length*i+Vector3.forward*(i+1)*-.1f;
            Mesh objectMesh = shapes[i].GetMesh();
            newObject.AddComponent<MeshFilter>().mesh = objectMesh;
            Vector3[] vertices = objectMesh.vertices;
            Vector3 offset = objectMesh.bounds.center;

            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] -= offset;
            }
            objectMesh.vertices = vertices;
            objectMesh.RecalculateBounds();
            MeshRenderer renderer = newObject.AddComponent<MeshRenderer>();
            renderer.material = meshInstance.GetComponent<MeshRenderer>().sharedMaterial;
            Color color = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),1);
            renderer.materials[0].color = color;

            newObject.AddComponent<MeshCollider>();
            newObject.layer = LayerMask.NameToLayer("Objects");
            newObject.tag = "Objects";
        }

        for(int i = 0; i < triangles.Count; i++)
        {
            Destroy(triangles[i].GetObject());
        }
    }

    public Vector3 GetSnapPoint(GameObject shape)
    {
        Vector3 pos = shape.transform.position;
        float dist = Mathf.Infinity;

        for(int i = 0; i < gridVertices.Count; i++)
        {
            float newDist = Vector3.Distance(gridVertices[i],shape.transform.position);
            if (newDist < dist && newDist<1f)
            {
                dist = newDist;
                pos = gridVertices[i];
            }
        }
        Mesh meshObject = shape.transform.GetComponent<MeshFilter>().mesh;
        dist = Mathf.Infinity;
        Vector3 nearVertPos=Vector3.zero;
        for(int i = 0; i < meshObject.vertices.Length; i++)
        {
            float newDist = Vector3.Distance(shape.transform.localToWorldMatrix.MultiplyPoint3x4(meshObject.vertices[i]), shape.transform.position);
            if (newDist < dist && newDist < .25f)
            {
                dist = newDist;
                nearVertPos = shape.transform.localToWorldMatrix.MultiplyPoint3x4(meshObject.vertices[i]);
            }
        }

        pos = pos + (shape.transform.position - nearVertPos);
        pos.z = -.5f;

        return pos;
    }
    public void CheckGrid()
    {
        List<Vector3> shapeVertices = new List<Vector3>();
        GameObject[] shapePieces = GameObject.FindGameObjectsWithTag("Objects");
        for (int i = 0; i < shapePieces.Length; i++) 
        {
            Mesh meshObject = shapePieces[i].transform.GetComponent<MeshFilter>().mesh;
            for(int j = 0; j < meshObject.vertices.Length; j++)
            {
                Vector3 meshPos = shapePieces[i].transform.localToWorldMatrix.MultiplyPoint3x4(meshObject.vertices[j]);
                if (!shapeVertices.Contains(meshPos))
                {
                    Vector3 pos = new Vector3(meshPos.x,meshPos.y,gridVertices[0].z);
                    shapeVertices.Add(pos);
                }
            }
        }
        Debug.Log(shapePieces.Count());
        Debug.Log(gridVertices.Count);
        Debug.Log(shapeVertices.Count);
        bool complete = true;
        for (int i = 0; i < gridVertices.Count; i++)
        {
            if (!shapeVertices.Contains(gridVertices[i]))
            {
                complete = false;
            }
        }
        if (complete)
        {
            Debug.Log("Level Completed");
        }
    }
}
