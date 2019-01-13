using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingSceneControler : MonoBehaviour {

    public Slider EffectSlider;
    public Slider BGMSlider;
	// Use this for initialization
	void Start ()
    {
        Screen.SetResolution(Screen.width, Screen.width*9 / 16, true);
        Setting.loadOption();
        EffectSlider.value = Setting.EffectVolume;
        BGMSlider.value = Setting.BGMVolume;
        Debug.Log("EffectSlider.value = " + EffectSlider.value.ToString());
        Debug.Log("BGMSlider.value = " + BGMSlider.value.ToString());
    }
	
	// Update is called once per frame
	void Update () {
        Setting.EffectVolume = EffectSlider.value;
        Setting.BGMVolume = BGMSlider.value;
        Setting.saveOption();
	}
    public void returnToStartSceneFuntion()
    {
        SceneManager.LoadScene("StartMenu");
    }
    public void doTutorial()
    {
        Debug.Log("Do Tutorial");
        return;
    }
    public void doCredit()
    {
        Debug.Log("Do Credit");
        return;
    }
}
