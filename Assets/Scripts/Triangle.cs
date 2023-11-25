using UnityEngine;
public class Triangle
{
    public Vector3 centerPoint;
        
    private Vector3[] _vertices;

    private int[] _indexes;
        
    public Triangle(Vector3[] vertices, int[] indexOrder)
    {
        _vertices = vertices;
            
        float centerPointX = 0;
        float centerPointY = 0;
            
        for (int i = 0; i < vertices.Length; i++)
        {
            centerPointX += vertices[i].x;
            centerPointY += vertices[i].y;
        }
            
        centerPoint = new Vector3(centerPointX / 3, centerPointY / 3, 0f);
        _indexes = indexOrder;
    }
        
    public Vector3[] GetVertices()
    {
        return _vertices;
    }
        
    public int[] GetIndexes()
    {
        return _indexes;
    }
}