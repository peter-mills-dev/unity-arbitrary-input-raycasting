using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArbitraryInputPointer : ArbitraryInput
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void Update()
    {
        base.Update();
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

        Image image = temp.AddComponent<Image>();
        image.sprite = CursorSprite;
        image.material = CursorMaterial;

        if (CursorSprite == null)
            Debug.LogError("Set CursorSprite on " + this.gameObject.name + " to the sprite you want to use as your cursor.", this.gameObject);

        Cursor = temp.GetComponent<RectTransform>();
    }
}
