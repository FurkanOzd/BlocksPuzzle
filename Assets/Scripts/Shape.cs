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

        private Snap _snap = new Snap();
        
        private List<Triangle> _triangles = new List<Triangle>();

        public Vector3 Position => transform.position;
        
        public static event Action<Shape> OnShapeSelected;

        public void Construct(string shapeName, Color color)
        {
            gameObject.name = shapeName;
            _meshRenderer.material.color = color;
        }

        private void OnMouseDown()
        {
            OnShapeSelected?.Invoke(this);
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

            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] -= offset;
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }

        private void CalculateSnapPoints()
        {
            Vector3[] vertices = _meshFilter.mesh.vertices;

            Vector3 firstVertexLocal = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[0]);

            _snap = new Snap();

            _snap.bottom = _snap.top = _snap.left = _snap.right = firstVertexLocal;

            int verticesLength = vertices.Length;
            for (int index = 1; index < verticesLength; index++)
            {
                Vector3 vertex = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[index]);
                
                if (_snap.bottom.y < vertex.y)
                {
                    _snap.bottom = vertex;
                }
                if (_snap.left.x < vertex.x)
                {
                    _snap.left = vertex;
                }
                if (_snap.top.y > vertex.y)
                {
                    _snap.top = vertex;
                }
                if (_snap.right.x > vertex.x)
                {
                    _snap.right = vertex;
                }
            }
        }

        /*public Mesh GetMesh()
        {
            objectMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[triangleMeshes.Count];

            for (int i = 0; i < combine.Count(); i++)
            {
                combine[i].mesh = triangleMeshes[i].mesh;
                combine[i].transform = triangleMeshes[i].transform.localToWorldMatrix;
            }
            objectMesh.CombineMeshes(combine);

            return objectMesh;
        }*/

        public void AddNewTriangle(Triangle triangle)
        {
            _triangles.Add(triangle);
        }

        private class Snap
        {
            public Vector3 left;
            public Vector3 right;
            public Vector3 top;
            public Vector3 bottom;
        }
    }