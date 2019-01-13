using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class HeadLineGameManager : MonoBehaviour {

    //Button newGameBtn, loadGameBtn, OptionBtn, quitBtn;
    Button saveBtn, addBtn;
    public Image BG;
    Player playerManager;
    // Use this for initialization
    void Start ()
    {
        int num = Random.Range(1, 6);
        Debug.Log("Starting HLManager");
        Debug.Log("num = "+num);
        playerManager = Player.getPlayer();     //interviewList는 player가 만들어지면서 같이 만들어짐
        BG.sprite = (Sprite)Resources.Load(Path.Combine("Title", "title"+num), typeof(Sprite));
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.Comma))
            Debug.Log(".");
        if (Input.GetKeyDown(KeyCode.E))
            Debug.Log(":"+ playerManager.getPlayerNum().ToString());
    }

    void Awake()
    {

    }

    public void doorQuit()
    {
        Application.Quit();
    }
    public void newGameButtonFuntion()
    {
        playerManager.makeGameData(1);
        SceneManager.LoadScene("TutorialScene");
    }
    public void loadGameButtonFuntion()
    {
        playerManager.selectGameData(1);
        SceneManager.LoadScene("MainScene");
    }
    public void optionButtonFuntion()
    {
        SceneManager.LoadScene("TitleSettingScene");
    }
    public void loadInterviewFunction()
    {
        SceneManager.LoadScene("Interview");
    }
    public void quitButtonFuntion()
    {
        Application.Quit();
    }    
}
