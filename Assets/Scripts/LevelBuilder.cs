using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField]
    private Transform _gridtopLeftCorner;
    [SerializeField]
    private Transform _gridBottomRightCorner;
    [SerializeField] 
    private Transform _shapesParent;

    [SerializeField]
    private GridCell _cellInstance;
    
    [SerializeField] 
    GameObject levelCompleteUI;

    [SerializeField] 
    private Shape _shapeInstance;

    private ShapeFactory _shapeFactory;
    
    [SerializeField]
    Transform shapeCreationPoint;
    
    private Shape[] _shapes;

    private int _boardSize;
    private int _pieceCount;
    
    public List<Difficulty> levelDifficulties;
    bool _isPlaying = false;

    private GridController _gridController;
    
    void Start()
    {
        _shapeFactory = new ShapeFactory();

        _gridController = new GridController(transform, _cellInstance, _gridtopLeftCorner.position,
            _gridBottomRightCorner.position);
        
        InitBoard();
    }

    private void InitBoard()
    {
        ClearBoard();
        
        Difficulty levelDiff = levelDifficulties[Random.Range(0, levelDifficulties.Count)];
        
        int boardSize = Random.Range(levelDiff.minBoardSize, levelDiff.maxBoardSize + 1);
        _pieceCount = Random.Range(levelDiff.minPieceCount, levelDiff.maxPieceCount + 1);

        _boardSize = boardSize + 1;

        Vector3 startPoint = _gridtopLeftCorner.position;
        Vector3 endPoint = _gridBottomRightCorner.position;
        
        _gridController.Initialize(_boardSize);
        
        List<Triangle> triangles = _gridController.GetTriangles();
        
        _shapes = new Shape[_pieceCount];
        int[] randomPositions = new int[_pieceCount];

        for (int i = 0; i < randomPositions.Length; i++)    //Get Random CenterPoint From Vertices For Shapes
        {
            int randNum = Random.Range(0, triangles.Count);
            while (randomPositions.Contains(randNum))
            {
                randNum = Random.Range(0, triangles.Count);
            }
            randomPositions[i] = randNum;
        }

        for (int i = 0; i < _shapes.Length; i++)
        {
            string shapeName = $"shape {i + 1}";
            _shapes[i] = _shapeFactory.Create(_shapeInstance, _shapesParent, triangles[randomPositions[i]].CenterPoint,
                new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1), shapeName);
        }
        
        for (int i = 0; i < triangles.Count; i++)
        {
            float dist = Mathf.Infinity;
            Shape selectedShape = _shapes[0];
            for (int j = 0; j < _shapes.Length; j++)
            {
                if (dist > Vector3.Distance(triangles[i].CenterPoint, _shapes[j].Position))
                {
                    dist = Vector3.Distance(triangles[i].CenterPoint, _shapes[j].Position);
                    selectedShape = _shapes[j];
                }
            }
            selectedShape.AddNewTriangle(triangles[i]);
        }

        for (int i = 0; i < _shapes.Length; i++)  //Create Shape Objects
        {
            _shapes[i].GenerateShapeMesh();
            _shapes[i].transform.position = shapeCreationPoint.position +
                                            Vector3.right * (endPoint - startPoint).x / _shapes.Length * i +
                                            Vector3.forward * (i + 1) * -.1f;
        }
        
        GameData data = new GameData();
        data.boardSize = boardSize;
        data.pieceCount = _shapes.Length;
        data.difficulty = levelDiff.name;
        string json = JsonUtility.ToJson(data);
        Debug.Log(json);

        File.WriteAllText(Application.dataPath+"//levelOutput.json",json);
        _isPlaying = true;
    }
    
    /*public void CheckGrid()
    {
        List<Vector3> mainShapeCenters = new List<Vector3>();

        for (int j = 0; j < _shapePieces.Length; j++)
        {
            MeshFilter filter = _shapePieces[j].GetComponent<MeshFilter>();
            for (int i = 0; i < filter.mesh.triangles.Count(); i += 3)
            {
                Vector3 p1 = _shapePieces[j].transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i]]);
                Vector3 p2 = _shapePieces[j].transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i + 1]]);
                Vector3 p3 = _shapePieces[j].transform.localToWorldMatrix.MultiplyPoint3x4(filter.mesh.vertices[filter.mesh.triangles[i + 2]]);

                mainShapeCenters.Add((p1 + p2 + p3) / 3);
            }
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
    }*/
    
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
            Destroy(transform.GetChild(i).gameObject);
        }

        if (_shapes != null)
        {
            int shapeCount = _shapes.Length;
            for (int i = shapeCount - 1; i >= 0; i--)
            {
                Destroy(_shapes[i].gameObject);
            }
        }
    }
}
