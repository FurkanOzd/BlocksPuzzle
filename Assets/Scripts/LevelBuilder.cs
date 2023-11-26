using System.Collections.Generic;
using System.Linq;
using GridModule;
using ShapeModule;
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
    Transform shapeCreationPoint;

    [SerializeField]
    private GridCell _cellInstance;

    [SerializeField] 
    private Shape _shapeInstance;

    private ShapeFactory _shapeFactory;
    
    private Shape[] _shapes;

    private int _boardSize;
    private int _pieceCount;
    
    public List<Difficulty> levelDifficulties;

    private GridController _gridController;
    
    private void Awake()
    {
        _shapeFactory = new ShapeFactory();

        _gridController = new GridController(transform, _cellInstance, _gridtopLeftCorner.position,
            _gridBottomRightCorner.position);
    }
    
    public void InitBoard()
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

        for (int i = 0; i < randomPositions.Length; i++)
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

        for (int i = 0; i < _shapes.Length; i++)
        {
            _shapes[i].GenerateShapeMesh();
            _shapes[i].transform.position = shapeCreationPoint.position +
                                            Vector3.right * (endPoint - startPoint).x / _shapes.Length * i +
                                            Vector3.forward * (i + 1) * -.1f;
        }
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