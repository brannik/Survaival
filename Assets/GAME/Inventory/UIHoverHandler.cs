using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverHandler : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AllowPointerLock.Instance.AllowToLockThePointer = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AllowPointerLock.Instance.AllowToLockThePointer = true;
    }

    void OnDisable(){
        AllowPointerLock.Instance.AllowToLockThePointer = true;
    }
    void OnDestroy(){
        AllowPointerLock.Instance.AllowToLockThePointer = true;
    }
}
