using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{

    private static CursorManager instance;

    // Static singleton property
    public static CursorManager Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get { return instance ; }
    }
    private void Awake()
    {
        // Ensure there's only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 cursorHotspot;
    [Header("DEBUG")]
    [SerializeField] public List<CursorModel> models;

    void Start()
    {
        cursorHotspot = new Vector2(0f ,5f);
        Cursor.SetCursor(cursorTexture,cursorHotspot,CursorMode.Auto);

    }

    public void SetCursor(CursorModel _model){
        Cursor.SetCursor(_model.cursorTexture,_model.cursorHotspot,CursorMode.Auto);
    }

    public CursorModel GetModelByName(string name){
        foreach(CursorModel model in models){
            if(model.ModelName == name) return model;
        }
        return null;
    }

    [System.Serializable]
    public class CursorModel{
        public string ModelName;
        public Texture2D cursorTexture;
        public Vector2 cursorHotspot;
        

        public CursorModel(){}

    }
}
