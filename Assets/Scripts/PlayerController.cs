using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float mouseZ = 0f;
    Vector3 clickOffset = Vector3.zero;
    GameObject selectedObject = null;
    LevelBuilder builder;
    private void Start()
    {
        builder = FindObjectOfType<LevelBuilder>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            DragObject();
        }

        if (Input.GetMouseButtonDown(0) && selectedObject==null)
        {
            OnClick();
        }
        if (Input.GetMouseButtonUp(0) && selectedObject!=null)
        {
            selectedObject.transform.position = builder.GetSnapPoint(selectedObject);
            selectedObject = null;
            builder.CheckGrid();
        }
    }
    void OnClick()
    {
        bool hits = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,50);
        if (hits)
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Objects"))
            {
                selectedObject = hit.transform.gameObject;
                mouseZ = selectedObject.transform.position.z;
                clickOffset = selectedObject.transform.position - GetMouseWorldPos();
            }
        }
    }

    private void DragObject()
    {
        selectedObject.transform.position = GetMouseWorldPos() + clickOffset;
        selectedObject.transform.position -= Vector3.forward * .1f;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mouseZ + (Vector3.forward * (-Camera.main.transform.position.z)).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
