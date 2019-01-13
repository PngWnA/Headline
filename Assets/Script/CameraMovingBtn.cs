using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMovingBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 MoveVec;
    public Camera MainCamera;
    bool check;
    public void OnPointerDown(PointerEventData eventData)
    {
        check = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        check = false;
    }
    void Update()
    {
        if (check)
            MainCamera.transform.Translate(MoveVec);
    }
}
