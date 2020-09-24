using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArbitraryInput : MonoBehaviour
{
    public static List<ArbitraryInput> instances;

    public bool buttonPressed;

    private bool lastButtonPressed;

    [HideInInspector] public bool buttonDown;

    [HideInInspector] public bool buttonUp;

    public PointerEventData PointEvent;

    protected RectTransform Cursor;

    public bool isHittingUI;
    public Vector3 CursorPosition;
    public Quaternion CursorRotation;
    public Vector3 CursorScale;

    [HideInInspector] public GameObject CurrentPoint;
    [HideInInspector] public GameObject CurrentPressed;
    [HideInInspector] public GameObject CurrentDragging;

    public Sprite CursorSprite;
    public Material CursorMaterial;

    #region Unity Methods

    protected virtual void Awake()
    {
        if(instances == null)
        {
            instances = new List<ArbitraryInput>();
        }

        if(!instances.Contains(this))
        {
            instances.Add(this);
        }
        else
        {
            Debug.LogError("How was this InputPointer already in instances");
        }
    }

    protected virtual void Start()
    {
        InitalizeCursor();
    }

    protected virtual void OnDestroy()
    {
        if(instances != null && instances.Contains(this))
        {
            instances.Remove(this);
        }
        else
        {
            Debug.LogError("Why wasn't this InputPointer in instances");
        }
    }

    protected virtual void Update()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);

        buttonDown = buttonPressed && !lastButtonPressed;
        buttonUp = !buttonPressed && lastButtonPressed;

        lastButtonPressed = buttonPressed;

        UpdateCursor();
    }

    #endregion

    public void Press()
    {
        buttonPressed = true;
    }

    public void Unpress()
    {
        buttonPressed = false;
    }

    protected virtual void InitalizeCursor() { }

    protected virtual void UpdateCursor()
    {
        Cursor.position = CursorPosition;
        Cursor.rotation = CursorRotation;
        Cursor.localScale = CursorScale;

        Cursor.gameObject.SetActive(isHittingUI);
    }
}
