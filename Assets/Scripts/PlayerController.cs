using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Shape _selectedShape;

    private Camera _camera;
    
    private Vector3 _clickOffset = Vector3.zero;
 
    private bool _isAnyShapeSelected;
    
    private float _mouseZ;

    private void Start()
    {
        _camera = Camera.main;
        
        ListenEvents();
    }

    private void ListenEvents()
    {
        Shape.ShapeSelectedEvent += ShapeSelectedEvent;
    }

    private void ShapeSelectedEvent(Shape shape)
    {
        _selectedShape = shape;

        SetShapeClickOffset();
        
        _isAnyShapeSelected = true;
    }

    private void SetShapeClickOffset()
    {
        _mouseZ = _selectedShape.transform.position.z;
        _clickOffset = _selectedShape.transform.position - GetMouseWorldPos();
    }

    void Update()
    {
        if (!_isAnyShapeSelected)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            DragObject();
            return;
        }

        if (Input.GetMouseButtonUp(0))
        { 
            _selectedShape.ReleaseShape();
            _selectedShape = null;
            _isAnyShapeSelected = false;
        }
    }

    private void DragObject()
    {
        _selectedShape.transform.position = GetMouseWorldPos() + _clickOffset;
        _selectedShape.transform.position -= Vector3.forward * .1f;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _mouseZ + (Vector3.forward * (-Camera.main.transform.position.z)).z;
        return _camera.ScreenToWorldPoint(mousePos);
    }

    private void UnsubscribeFromEvents()
    {
        Shape.ShapeSelectedEvent -= ShapeSelectedEvent;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}
