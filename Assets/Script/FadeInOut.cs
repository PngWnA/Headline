using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionsMeasures
{

    public static void FadeIn(this Graphic g, float second = 3f, float start = 0f, float end = 1f)
    {
        g.canvasRenderer.SetAlpha(start);
        g.CrossFadeAlpha(end, second, false);
    }

    public static void FadeOut(this Graphic g, float second = 3f, float start = 0f, float end = 1f)
    {
        g.canvasRenderer.SetAlpha(1f - start);
        g.CrossFadeAlpha(1f - end, second, false);
    }
}
