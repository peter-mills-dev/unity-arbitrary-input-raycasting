using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TapCursorController : MonoBehaviour
{
    public Image ring, gradient;

    [Range(0,1)][SerializeField]
    private float closeness;
    float minAlpha = 0;
    float maxAlpha = 0.3f;
    public Vector3 minGradientSize, maxGradientSize;

    private void OnValidate()
    {
        UpdateCursor(closeness);
    }

    public void UpdateCursor(float value)
    {
        closeness = value;

        gradient.transform.localScale = Vector3.Lerp(minGradientSize, maxGradientSize, closeness);

        gradient.color = new Color(gradient.color.r, gradient.color.g, gradient.color.b, Mathf.Lerp(minAlpha, maxAlpha, closeness));
    }
}
