using ShapeModule;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Shape _selectedShape;

    private Camera _camera;
    
    private Vector3 _clickOffset = Vector3.zero;
 
    private bool _isAnyShapeSelected;
    
    private float _mouseZ;

    private bool _canPlay;

    private void Awake()
    {
        _camera = Camera.main;
        
        ListenEvents();
    }

    private void ListenEvents()
    {
        Shape.ShapeSelectedEvent += OnShapeSelected;
        GameManager.PlayStateChangedEvent += OnPlayStateChanged;
    }

    private void OnPlayStateChanged(bool canPlay)
    {
        _canPlay = canPlay;
    }

    private void OnShapeSelected(Shape shape)
    {
        _selectedShape = shape;

        SetShapeClickOffset();
        
        _isAnyShapeSelected = true;
    }

    private void SetShapeClickOffset()
    {
        Vector3 shapePosition = _selectedShape.transform.position;
        _mouseZ = shapePosition.z;
        _clickOffset = shapePosition - GetMouseWorldPos();
    }

    void Update()
    {
        if (!_isAnyShapeSelected || !_canPlay)
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
        Shape.ShapeSelectedEvent -= OnShapeSelected;
        GameManager.PlayStateChangedEvent -= OnPlayStateChanged;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}
