using System;
using System.Collections.Generic;
using ShapeModule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GridModule
{
    public class GridController : IDisposable
    {
        private GridCell _gridCellInstance;
    
        private GridCell[,] _gridCells;

        private bool _isInitialized;

        private Vector3 _gridTopLeft;
        private Vector3 _gridBottomRight;

        private readonly Transform _cellParent;

        private int _boardSize;

        private List<Triangle> _triangles = new List<Triangle>();
        
        public static event Action GridCompletedEvent;
    
        public GridController(Transform cellParent, GridCell gridCellInstance, Vector3 gridTopLeft, Vector3 gridBottomRight)
        {
            _gridTopLeft = gridTopLeft;
            _gridBottomRight = gridBottomRight;
            _cellParent = cellParent;
            _gridCellInstance = gridCellInstance;
        
            ListenEvents();
        }
    
        public void Initialize(int boardSize)
        {
            ClearGrid();

            _boardSize = boardSize;
        
            _gridCells = new GridCell[boardSize, boardSize];

            Vector3 pointDifference = _gridBottomRight - _gridTopLeft;
        
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    _gridCells[i,j] = Object.Instantiate(_gridCellInstance,
                        _gridTopLeft + Vector3.right * (pointDifference.x / (boardSize - 1)) * j +
                        Vector3.up * (pointDifference.y / (boardSize - 1)) * i, Quaternion.identity, _cellParent);

                    _gridCells[i, j].gameObject.name = $"Grid[{i},{j}]";
                }
            }
        
            CreateTriangles();

            _isInitialized = true;
        }

        private void ListenEvents()
        {
            Shape.ShapeReleasedEvent += OnShapeReleased;
        }
    
        private void OnShapeReleased(Shape shape)
        {
            bool isSnapped = SnapShapeToGrid(shape);

            if (isSnapped)
            {
                CheckGrid();
            }
        }

        private bool SnapShapeToGrid(Shape shape)
        {
            Vector3[] snapPoints = shape.GetSnapPoints();

            int snapCount = snapPoints.Length;

            float closestPoint = Mathf.Infinity;
        
            Vector3 offset = Vector3.zero;
        
            for (int pointIndex = 0; pointIndex < snapCount; pointIndex++)
            {
                for (int i = 0; i < _boardSize; i++)
                {
                    for (int j = 0; j < _boardSize; j++)
                    {
                        float distance = Vector3.Distance(snapPoints[pointIndex], _gridCells[i, j].transform.position);
                        if (distance < closestPoint)
                        {
                            closestPoint = distance;
                            offset = _gridCells[i, j].transform.position - snapPoints[pointIndex];
                        }
                    }
                }
            }

            if (offset.magnitude > 1f)
            {
                shape.ResetToPreviousPosition();
                return false;
            }

            Vector3 newPos = shape.transform.position + offset;
            newPos.z = -1f;
            shape.transform.position = newPos;
        
            shape.CheckForGridFill();
            
            return true;
        }

        private void CheckGrid()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    if (_gridCells[i,j].IsEmpty)
                    {
                        return;
                    }
                }
            }
            
            GridCompletedEvent?.Invoke();
        }
    
        private void CreateTriangles()
        {
            for (int i = 0; i < _boardSize - 1; i++)
            {
                for (int j = 0; j < _boardSize - 1; j++)
                {
                    Vector3 p1 = GetCellPosition(i,j);
                    Vector3 p2 = GetCellPosition(i + 1, j);
                    Vector3 p3 = (p2 + GetCellPosition(i, j + 1)) / 2;
                    _triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 2, 1 }));
                
                    p1 = GetCellPosition(i,j);
                    p2 = GetCellPosition(i,j + 1);
                
                    _triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 1, 2, 0 }));

                    p1 = GetCellPosition(i+1,j);
                    p2 = GetCellPosition(i+1,j + 1);
                
                    _triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 2, 1 }));

                    p1 = GetCellPosition(i,j + 1);
                    p2 = GetCellPosition(i+1,j + 1);

                    _triangles.Add(new Triangle(new Vector3[] { p1, p2, p3 }, new int[] { 0, 1, 2 }));
                }
            }
        }
    
        public List<Triangle> GetTriangles()
        {
            return _triangles;
        }

        private Vector3 GetCellPosition(int i, int j)
        {
            return _gridCells[i, j].transform.position;
        }
    
        private void ClearGrid()
        {
            if (!_isInitialized)
            {
                return;
            }
        
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    Object.Destroy(_gridCells[i,j]);
                }
            }
            _triangles.Clear();
        }

        private void UnsubscribeFromEvents()
        {
            Shape.ShapeReleasedEvent -= OnShapeReleased;
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}