using UnityEngine;

namespace ShapeModule
{
    public class ShapeFactory
    {
        public Shape Create(Shape shapeInstance, Transform parent, Vector3 position, Color color, string gameObjectName)
        {
            Shape shape = Object.Instantiate(shapeInstance, position, Quaternion.identity, parent);
            shape.Construct(gameObjectName, color);

            return shape;
        }
    }
}