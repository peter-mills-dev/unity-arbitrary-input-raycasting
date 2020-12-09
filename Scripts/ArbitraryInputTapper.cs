using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArbitraryInputTapper : ArbitraryInput
{
    /// <summary>
    /// At what distance from a UI Canvas should this module call <see cref="ArbitraryInput.Press"/>
    /// </summary>
    [Tooltip("At what distance from a UI Canvas should this module call ArbitraryInput.Press")]
    [SerializeField] private float pointToActivateAt = 0.1f;
    /// <summary>
    /// The distance from <see cref="pointToActivateAt"/> that will trigger <see cref="ArbitraryInput.Press"/> calls
    /// </summary>
    [Tooltip("The distance from pointToActivateAt that will trigger ArbitraryInput.Press calls")]
    [SerializeField] private float activationRange = 0.1f;
    /// <summary>
    /// Cursor will become visible when this is greater than <see cref="distanceToTarget"/>
    /// </summary>
    [Tooltip("Cursor will become visible when this is greater than distanceToTarget")]
    [SerializeField] private float distanceForCursorActivation = 0.5f;

    [SerializeField] private float distanceToTarget = Mathf.Infinity;

    [SerializeField]
    private GameObject tapCursorPrefab;

    [SerializeField] public UnityEvent OnTouch;
    [SerializeField] public UnityEventFloat OnNearTouch;

    private TapCursorController tapCursorController;

    private LookAtConstraint lookAtConstraint;
    private LookAtConstraint LookAtConstraint
    {
        get
        {
            if (lookAtConstraint == null)
            {
                lookAtConstraint = GetComponent<LookAtConstraint>();
            }

            return lookAtConstraint;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        //transform.localPosition = new Vector3(-(pointToActivateAt + activationRange), 0, 0);
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        if(Cursor != null)
            Cursor.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

        if (Cursor != null)
            Cursor.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        base.OnDestroy();
    }

    private void OnValidate()
    {
        //transform.localPosition = new Vector3(pointToActivateAt - activationRange, 0 , 0);
    }

    protected override void Update()
    {
        if (PointEvent != null)
        {
            distanceToTarget = PointEvent.pointerCurrentRaycast.distance;

            OnNearTouch.Invoke(Mathf.InverseLerp(0, distanceForCursorActivation, distanceToTarget));
        }

        if(distanceToTarget <= pointToActivateAt + activationRange && distanceToTarget >= pointToActivateAt - activationRange)
        {
            if (!buttonPressed)
            {
                Press();
                OVRInput.SetControllerVibration(.3f, .3f, OVRInput.Controller.RTouch);
            }

            OnTouch.Invoke();
        }
        else
        {

            if (buttonPressed)
            {
                Unpress();
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            }
        }
        
        base.Update();

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        Gizmos.DrawSphere(transform.position, 0.01f);

        Gizmos.color = Color.grey;

        Gizmos.DrawSphere(transform.position + transform.forward * distanceForCursorActivation, 0.01f);
        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(transform.position + transform.forward * pointToActivateAt, 0.01f);
        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(transform.position + transform.forward * (pointToActivateAt - activationRange), 0.01f);
        Gizmos.DrawSphere(transform.position + transform.forward * (pointToActivateAt + activationRange), 0.01f);

        Gizmos.color = Color.black;

        Gizmos.DrawSphere(transform.position + transform.forward * distanceToTarget, 0.01f);
    }

    protected override void InitalizeCursor()
    {
        GameObject temp = new GameObject("Cursor " + instances.FindIndex(a => a == this));
        Canvas canvas = temp.AddComponent<Canvas>();
        temp.AddComponent<CanvasRenderer>();
        temp.AddComponent<CanvasScaler>();
        temp.AddComponent<UIIgnoreRaycast>();
        temp.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000; //set to be on top of everything

        /*Image image = temp.AddComponent<Image>();
        image.sprite = CursorSprite;
        image.material = CursorMaterial;*/

        tapCursorController = Instantiate(tapCursorPrefab, temp.transform).GetComponent<TapCursorController>();

        /*if (CursorSprite == null)
            Debug.LogError("Set CursorSprite on " + this.gameObject.name + " to the sprite you want to use as your cursor.", this.gameObject);*/

        Cursor = temp.GetComponent<RectTransform>();
    }

    protected override void UpdateCursor()
    {
        if(Cursor == null)
        {
            return;
        }

        Cursor.position = CursorPosition;
        Cursor.rotation = CursorRotation;
        Cursor.localScale = CursorScale;

        Cursor.gameObject.SetActive(isHittingUI && (distanceToTarget <= distanceForCursorActivation));

        tapCursorController.UpdateCursor(Mathf.InverseLerp(distanceForCursorActivation, pointToActivateAt + activationRange, distanceToTarget));
    }
}

[Serializable]
public class UnityEventFloat : UnityEvent<float> { }
