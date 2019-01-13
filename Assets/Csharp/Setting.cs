using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Setting{

    public static float BGMVolume;
    public static float EffectVolume;

    public static void saveOption()
    {
        PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
        PlayerPrefs.SetFloat("EffectVolume", EffectVolume);
    }
    public static void loadOption()
    {
        BGMVolume = PlayerPrefs.GetFloat("BGMVolume");
        EffectVolume = PlayerPrefs.GetFloat("EffectVolume");
    }
}
