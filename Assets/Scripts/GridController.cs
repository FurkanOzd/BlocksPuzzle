using UnityEngine;

public class GridController
{
    private GridCell _gridCellInstance;
    
    private GridCell[,] _gridCells;

    private bool _isInitialized;

    private Vector3 _gridTopLeft;
    private Vector3 _gridBottomRight;

    private Transform _cellParent;
    
    public GridController(Transform cellParent, GridCell gridCellInstance, Vector3 gridTopLeft, Vector3 gridBottomRight)
    {
        _gridTopLeft = gridTopLeft;
        _gridBottomRight = gridBottomRight;

        _cellParent = cellParent;

        _gridCellInstance = gridCellInstance;
    }
        
    public void Initialize(int boardSize)
    {
        ClearGrid();
        
        _gridCells = new GridCell[boardSize, boardSize];

        Vector3 pointDifference = _gridBottomRight - _gridTopLeft;
        
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                _gridCells[i,j] = Object.Instantiate(_gridCellInstance,
                    _gridTopLeft + Vector3.right * (pointDifference.x / (boardSize - 1)) * j +
                    Vector3.up * (pointDifference.y / (boardSize - 1)) * i, Quaternion.identity, _cellParent);
            }
        }

        _isInitialized = true;
    }

    public Vector3 GetCellPosition(int i, int j)
    {
        return _gridCells[i, j].transform.position;
    }
    
    private void GetSnapPoint()
    {
        
    }

    private void ClearGrid()
    {
        if (!_isInitialized)
        {
            return;
        }
        
        int boardSize = _gridCells.GetLength(0);
        
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Object.Destroy(_gridCells[i,j]);
            }
        }
    }
}