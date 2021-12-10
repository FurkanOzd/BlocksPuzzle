using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelBuilder : MonoBehaviour
{
    int boardSize;
    int _boardSize;
    List<Vector3> gridVertices;
    int pieceCount;
    [SerializeField] GameObject pointInstance;
    [SerializeField] Transform plane;
    List<Triangle> triangles;
    [SerializeField] Material matInstance;
    [SerializeField] Transform shapeCreationPoint;
    [SerializeField] GameObject levelCompleteUI;
    GameObject[] shapePieces;
    List<Vector3> centerPoints;
    Mesh main;
    public List<Difficulty> levelDifficulties;
    bool _isPlaying = false;
    public bool isPlaying()
    {
        return _isPlaying;
    }
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

    void Start()
    {
        InitBoard();
    }

    void InitBoard()
    {
        ClearBoard();
        boardSize = 0;
        Difficulty levelDiff = levelDifficulties[Random.Range(0, levelDifficulties.Count)];
        boardSize = Random.Range(levelDiff.minBoardSize, levelDiff.maxBoardSize + 1);
        pieceCount = Random.Range(levelDiff.minPieceCount, levelDiff.maxPieceCount + 1);


        _boardSize = boardSize + 1;


        Vector3 startPoint = plane.GetChild(0).position;     // Init Board Points
        Vector3 endPoint = plane.GetChild(1).position;

        for (int i = 0; i < _boardSize; i++)
        {
            for (int j = 0; j < _boardSize; j++)
            {
                Instantiate(pointInstance, startPoint + transform.right * ((endPoint - startPoint).x / (_boardSize - 1)) * j + transform.up * ((endPoint - startPoint).y / (_boardSize - 1)) * i, Quaternion.identity, transform);
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
            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, matInstance));

            p1 = transform.GetChild(i).position;
            p2 = transform.GetChild(i + _boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 1, 0, 2 }, matInstance));

            p1 = transform.GetChild(i + 1).position;
            p2 = transform.GetChild(i + 1 + _boardSize).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }, matInstance));

            p1 = transform.GetChild(i + _boardSize).position;
            p2 = transform.GetChild(i + _boardSize + 1).position;

            triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 2, 1 }, matInstance));
        }

        gridVertices = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++)
        {
            List<Vector3> temp = triangles[i].GetVertices().ToList();
            for (int j = 0; j < temp.Count; j++)
            {
                gridVertices.Add(temp[j]);
            }
        }

        Shape[] shapes = new Shape[pieceCount];
        int[] randomPositions = new int[pieceCount];

        for (int i = 0; i < randomPositions.Length; i++)    //Get Random CenterPoint From Vertices For Shapes
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
            newObject.transform.position = shapeCreationPoint.position + Vector3.right * (endPoint - startPoint).x / shapes.Length * i + Vector3.forward * (i + 1) * -.1f;
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
            renderer.material = matInstance;
            renderer.materials[0].color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1); ;

            newObject.AddComponent<MeshCollider>();

            newObject.layer = LayerMask.NameToLayer("Objects");
            newObject.tag = "Objects";

            Vector3 xPos= newObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[0]);
            Vector3 xNeg= newObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[0]);
            Vector3 yPos= newObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[0]);
            Vector3 yNeg= newObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[0]);
            for (int j = 1; j < vertices.Length; j++)
            {
                Vector3 vertex = newObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[j]);
                if (yNeg.y < vertex.y)
                {
                    yNeg = vertex;
                }
                if (xNeg.x < vertex.x)
                {
                    xNeg = vertex;
                }
                if (yPos.y > vertex.y)
                {
                    yPos = vertex;
                }
                if (xPos.x > vertex.x)
                {
                    xPos = vertex;
                }
            }
            GameObject snapPoint = new GameObject();
            snapPoint.transform.position = xPos;
            snapPoint.transform.parent = newObject.transform;

            snapPoint = new GameObject();
            snapPoint.transform.position = yPos;
            snapPoint.transform.parent = newObject.transform;

            snapPoint = new GameObject();
            snapPoint.transform.position = xNeg;
            snapPoint.transform.parent = newObject.transform;

            snapPoint = new GameObject();
            snapPoint.transform.position = yNeg;
            snapPoint.transform.parent = newObject.transform;

        }


        centerPoints = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++)
        {
            Destroy(triangles[i].GetObject());
            centerPoints.Add(triangles[i].centerPoint);
        }
        shapePieces = GameObject.FindGameObjectsWithTag("Objects");
        _isPlaying = true;
    }

    public Vector3 GetSnapPoint(GameObject shape)
    {
        float dist = Mathf.Infinity;
        Vector3 distance = Vector3.zero;
        for(int i = 0; i < shape.transform.childCount; i++)
        {
            for(int j = 0; j < gridVertices.Count; j++)
            {
                Transform child = shape.transform.GetChild(i);
                Vector3 shapeChildPos = new Vector3(child.position.x,child.position.y,gridVertices[0].z);
                if (Vector3.Distance(shapeChildPos, gridVertices[j]) < dist)
                {
                    dist = Vector3.Distance(shapeChildPos,gridVertices[j]);
                    distance = gridVertices[j] - shapeChildPos;
                }
            }
        }
        if (distance.magnitude < .5f)
        {
            Vector3 pos = shape.transform.position + distance;
            pos.z = -.5f;
            return pos;
        }
        else
            return shape.transform.position;
    }
    public void CheckGrid()
    {
        List<Vector3> shapeVertices = new List<Vector3>();

        Mesh mainMesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[shapePieces.Length];

        for (int i = 0; i < combine.Count(); i++)
        {
            combine[i].mesh = shapePieces[i].GetComponent<MeshFilter>().mesh;
            combine[i].transform = shapePieces[i].transform.localToWorldMatrix;
        }
        GameObject newGameObject = new GameObject("Check");
        newGameObject.SetActive(false);
        MeshFilter filter = newGameObject.AddComponent<MeshFilter>();
        filter.mesh.CombineMeshes(combine);
        newGameObject.transform.position = new Vector3(0, 0, 0);
        List<Vector3> mainShapeCenters = new List<Vector3>();

        filter.mesh.RecalculateBounds();
        for(int i = 0; i < filter.mesh.triangles.Count(); i+=3)
        {
            Vector3 p1 = newGameObject.transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i]]);
            Vector3 p2 = newGameObject.transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i + 1]]);
            Vector3 p3 = newGameObject.transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i + 2]]);

            mainShapeCenters.Add((p1+p2+p3)/3);
        }
        int exist = 0;
        List<Vector3> matchedVertices = new List<Vector3>();
        for(int i = 0; i < mainShapeCenters.Count; i++)
        {
            for(int j = 0; j < centerPoints.Count; j++)
            {
                if ((new Vector3(mainShapeCenters[i].x,mainShapeCenters[i].y,centerPoints[j].z) == centerPoints[j]) && !matchedVertices.Contains(centerPoints[j]))
                {
                    exist++;
                    matchedVertices.Add(centerPoints[j]);
                    break;
                }
            }
        }
        if (exist == centerPoints.Count)
        {
            StartCoroutine(LevelComplete());
        }
    }
    IEnumerator LevelComplete()
    {
        _isPlaying = false;
        yield return new WaitForSeconds(1f);
        levelCompleteUI.SetActive(true);
    }

    public void NextLevel()
    {
        levelCompleteUI.SetActive(false);
        InitBoard();
    }

    void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        GameObject[] shapes = GameObject.FindGameObjectsWithTag("Objects");

        for (int i = shapes.Length - 1; i >= 0; i--)
        {
            DestroyImmediate(shapes[i]);
            shapes = GameObject.FindGameObjectsWithTag("Objects");
        }

        if (triangles != null)
            triangles.Clear();

        if (gridVertices != null)
            gridVertices.Clear();
    }
}
