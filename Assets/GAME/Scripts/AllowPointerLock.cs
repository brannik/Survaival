using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllowPointerLock : MonoBehaviour
{
    private static AllowPointerLock instance;
    public static AllowPointerLock Instance
    {
        get { return instance ; }
    }   
    void Awake()
	{
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
	}

    private bool allowToLockThePointer = true;
    private bool isHoldingItem = false;
    public bool AllowToLockThePointer{
        set{ allowToLockThePointer = value;}
        get{ 
            if(allowToLockThePointer && !IsHoldingItem){
                return true;
            }else{
                return false;
            }
        }
    }
    public bool IsHoldingItem{
        set{isHoldingItem = value;}
        private get{return isHoldingItem;}
    }
}
