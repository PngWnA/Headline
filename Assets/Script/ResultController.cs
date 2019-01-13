using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour {

    private int resultNumber;

    private const float doubleClickTimer= 0.18f;
    public Image frontimage;
    public Image rearimage;
    private Sprite frontspr;
    private Sprite rearspr;
    private float timer = 0.0f; // 시간
    private string[] paths = new string[8];

    private int frontIndex;

    // Use this for initialization
    void Start ()
    {
        Load();
	}
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    public void ShowPages(int offset = 0)
    {
        frontIndex += offset;

        frontIndex = Mathf.Max(Mathf.Min(frontIndex, 6), 0);        //index의 범위는 [0, 6]

        frontimage.sprite = (Sprite)Resources.Load(paths[frontIndex], typeof(Sprite));
        rearimage.sprite = (Sprite)Resources.Load(paths[frontIndex + 1], typeof(Sprite));
    }
    
    public void Commute()
    {
        SceneManager.LoadScene("MainScene");
    }
    
    public void Load(int daysAgo = 0)
    {
        resultNumber = Player.getPlayer().getResultNumber(daysAgo);
        if (Player.getPlayer().isCombo == true && resultNumber > 2 && Player.getPlayer().lastCategory() == InterviewList.Category.Combo) //연계기사 결과가 2면보다 뒤일경우 연계를 끝낸다.
        {
            Player.getPlayer().isCombo = false;
        }

        //resultNumber는 [1,5]의 값. 그 외에는 의미가 없으므로 -1로 처리
        if (resultNumber > 5 || resultNumber < 1)
        {
            resultNumber = -1;
        }

        frontIndex = 0;
        System.Random rnd = new System.Random();

        //미리 페이지들 경로를 설정해둔다
        paths[0] = Path.Combine("Newspaper", "0");
        paths[7] = Path.Combine("Newspaper", "0");

        for (int i = 1; i < 7; i++)
        {
            paths[i] = resultNumber == i ?
                Path.Combine(Player.getPlayer().getLastArticle(daysAgo), "page" + resultNumber)
                : Path.Combine("Newspaper", i + " - " + (rnd.Next(1, 3)));
        }

        ShowPages();
    }
}
