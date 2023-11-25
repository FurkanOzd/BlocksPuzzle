    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Shape : MonoBehaviour
    {
        [SerializeField] 
        private MeshFilter _meshFilter;

        [SerializeField] 
        private MeshRenderer _meshRenderer;

        [SerializeField] 
        private MeshCollider _meshCollider;
        
        private List<Triangle> _triangles = new List<Triangle>();
        
        public Vector3 Position => transform.position;
        
        private Vector3 _bottomSnapPoint;
        private Vector3 _topSnapPoint;
        private Vector3 _leftSnapPoint;
        private Vector3 _rightSnapPoint;
        private Vector3 _previousPosition;

        private List<GridCell> _gridCells = new List<GridCell>();

        private int _vertexCount = 0;
        
        public static event Action<Shape> ShapeSelectedEvent;
        public static event Action<Shape> ShapeReleasedEvent;

        public void Construct(string shapeName, Color color)
        {
            gameObject.name = shapeName;
            _meshRenderer.material.color = color;
        }

        private void OnMouseDown()
        {
            //EmptyGridCells();
            
            _previousPosition = transform.position;
            
            ShapeSelectedEvent?.Invoke(this);
        }

        private void EmptyGridCells()
        {
            for (int index = 0; index < _gridCells.Count; index++)
            {
                _gridCells[index].Toggle(true);
            }
            _gridCells.Clear();
        }

        public void GenerateShapeMesh()
        {
            Mesh mesh = new Mesh();
            
            List<Mesh> triangleMeshes = new List<Mesh>();
            
            for (int i = 0; i < _triangles.Count; i++)
            {
                Triangle triangle = _triangles[i];
                
                Mesh triangleMesh = new Mesh();
                triangleMesh.vertices = triangle.GetVertices();
                triangleMesh.triangles = triangle.GetIndexes();
                
                triangleMeshes.Add(triangleMesh);
            }
            
            CombineInstance[] combine = new CombineInstance[triangleMeshes.Count];

            for (int i = 0; i < combine.Count(); i++)
            {
                combine[i].mesh = triangleMeshes[i];
                combine[i].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 1f);
            }
            mesh.CombineMeshes(combine);
            
            Vector3[] vertices = mesh.vertices;
            Vector3 offset = mesh.bounds.center;
            
            _vertexCount = vertices.Length;
            
            for (int j = 0; j < _vertexCount; j++)
            {
                vertices[j] -= offset;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;            
            CalculateSnapPoints();
        }

        private void CalculateSnapPoints()
        {
            Vector3[] vertices = _meshFilter.mesh.vertices;
            
            _bottomSnapPoint = _topSnapPoint = _leftSnapPoint = _rightSnapPoint = vertices[0];

            int verticesLength = vertices.Length;
            for (int index = 1; index < verticesLength; index++)
            {
                Vector3 vertex = vertices[index];
                
                if (_bottomSnapPoint.y < vertex.y)
                {
                    _bottomSnapPoint = vertex;
                }
                if (_leftSnapPoint.x < vertex.x)
                {
                    _leftSnapPoint = vertex;
                }
                if (_topSnapPoint.y > vertex.y)
                {
                    _topSnapPoint = vertex;
                }
                if (_rightSnapPoint.x > vertex.x)
                {
                    _rightSnapPoint = vertex;
                }
            }
        }

        public void ResetToPreviousPosition()
        {
            transform.position = _previousPosition;
        }

        public void ReleaseShape()
        {
            ShapeReleasedEvent?.Invoke(this);
        }
        
        public Vector3[] GetSnapPoints()
        {
            return new Vector3[]
            {
                transform.TransformPoint(_leftSnapPoint),
                transform.TransformPoint(_rightSnapPoint),
                transform.TransformPoint(_topSnapPoint),
                transform.TransformPoint(_bottomSnapPoint),
            };
        }
        
        public void AddNewTriangle(Triangle triangle)
        {
            _triangles.Add(triangle);
        }

        public void CheckForGridFill()
        {
            Vector3[] vertices = _meshFilter.mesh.vertices;
            for (int index = 0; index < _vertexCount; index++)
            {
                Ray ray = new Ray(transform.TransformPoint(vertices[index]), Vector3.forward);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    if (hitInfo.transform.TryGetComponent(out GridCell gridCell))
                    {
                        gridCell.Toggle(false);
                        _gridCells.Add(gridCell);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3[] vertices = _meshFilter.mesh.vertices;
            
            for (int index = 0; index < _vertexCount; index++)
            {
                Gizmos.DrawRay(new Ray(transform.TransformPoint(vertices[index]), Vector3.forward));
            }
        }
    }