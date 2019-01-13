using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

public class SpecialEndingController : MonoBehaviour {

    private static string specialEnd;

    public Image background;

    private const float WAITING_TIME = 0.05f; // 이 시간마다 글자 출력.
    private float timer = 0.0f; // 시간
    private float turn = 0.0f; // 아직 안쓰는거임
    private string path;
    private bool end;
    private int num;
                               // Use this for initialization
    void Start () {
        path = Path.Combine("Ending", specialEnd);
        background.sprite = (Sprite)Resources.Load(path + 1.ToString(), typeof(Sprite));
        timer += Time.deltaTime;
        timer = 0;
        num = 1;
        end = false;
    }
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if (end)
        {
            if (timer >= 10.0)
            {
                SceneManager.LoadScene("StartMenu");
            }
        }
        else if (timer>=2.0)
        {
            num++;
            background.sprite = (Sprite)Resources.Load(path + num.ToString(), typeof(Sprite));
            if(background.sprite == null)
            {
                background.sprite = (Sprite)Resources.Load(Path.Combine("Ending", "끝"), typeof(Sprite));
                end = true;
            }
            timer = 0;
        }
    }
    public static void setSPEnding(string sp)
    {
        specialEnd = sp;
    }
}
