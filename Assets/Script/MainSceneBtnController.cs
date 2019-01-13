using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Xml;

public class MainSceneBtnController : MonoBehaviour {
    
    public Text calendarDate;
    public Canvas noteCanvas;
    public Canvas heroCanvas;
    public Canvas alertCanvas;
    public Canvas drawerCanvas;
    public Canvas newspaperCanvas;

    public Text[] hints = new Text[5];
    public Text[] hintsCategory = new Text[5];
    public Text heroMessage;
    public Text[] drawerText = new Text[4];
    private Player playerManager;

    public string PATH;

    void Start ()
    {
        playerManager = Player.getPlayer();

        if (playerManager.CheckDate() > 1)
        {
            playerManager.setNextInterview();

            //10턴부터 연계기사 발동
            if (playerManager.CheckDate() == 10)
            {
                Player.getPlayer().comboDate = 0; //comboDate가 시작부터 올라가므로 이 시점에서 초기화해줍니다.
                Player.getPlayer().isCombo = true; //콤보 시작! isCombo는 여기 또는 Load시점에서만 true로 설정됩니다.
                Player.getPlayer().isComboStarted = true; //콤보 시작! isCombo는 여기 또는 Load시점에서만 true로 설정됩니다.
                Player.getPlayer().setComboInterview(); //콤보인터뷰 설정
            }
        }
        SetHint();
        SetDrawerDay();

        hintsCategory[3].enabled = false;
        hintsCategory[4].enabled = false;
        hints[3].enabled = false;
        hints[4].enabled = false;
        noteCanvas.enabled = false;
        heroCanvas.enabled = false;
        alertCanvas.enabled = false;
        drawerCanvas.enabled = false;
        newspaperCanvas.enabled = false;

        calendarDate.text = playerManager.CheckDate().ToString() + "일";

        //26일에 엔딩
        if (playerManager.CheckDate() > 25)
        {
            SceneManager.LoadScene("Ending");
        }
        
        if (playerManager.CheckEnding())
        {
            EndingController.setFlag(playerManager.EndingNumber(), playerManager.getMost());
            SceneManager.LoadScene("Ending");
        }
        if (1 < playerManager.comboIndex && playerManager.comboIndex < 7 && (playerManager.isCombo == false) && playerManager.isComboStarted == true)
        {
            SpecialEndingController.setSPEnding("matiz");
            SceneManager.LoadScene("SpEndingScene");
        }

        //4일째부터 20%확률로 선배 등장
        if(playerManager.CheckDate() > 3)
        {
            ShowHero();
        }
    }
	
	void Update ()
    {

    }
    public void TestSaveFuntion()
    {
        playerManager.SaveGameState();
    }

    public void gainScoreFuntion(int score)
    {
        playerManager.addTotalScore(score);
        Debug.Log("TotalScore: " + playerManager.getTotalScore().ToString());
    }

    public void loadInterview(string article)
    {
        InterviewList.Category category = InterviewList.StringToCategory(article);
        //string을 category화 시킴
        if (Player.getPlayer().isCombo == true && category == Player.getPlayer().findComboCategory())
            category = InterviewList.Category.Combo;
        

        int number = playerManager.getNextInterview(category);        //몇번째 기사냐? 모두 다 한 경우는 -1
        
        if(number > 0 || (number==0 && Player.getPlayer().isCombo==true))
        {
            if(ReadIntro(category))
            {
                PaparazziController.MakePath(category, number);
                SceneManager.LoadScene("PaparazziScene");
            }
            else
            {
                HappyNARU.MakePath(category, number);
                SceneManager.LoadScene("Interview");
            }
        }
        //else if(number == 0)
        //{
        //    Debug.Log("LOCK: " + category.ToString() + " interviews are lock!");
        //}
        else
        {
            ShowAlert();
            Debug.Log("All " + category.ToString() + " interviews are done!");
        }
    }
    
    public void getBack()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void CallNote()
    {
        noteCanvas.enabled = true;
    }

    public void HideNote()
    {
        noteCanvas.enabled = false;
    }

    public void ToggleNotePage()
    {
        for (int i = 0; i < hints.Length; i++)
        {
            hintsCategory[i].enabled = !hintsCategory[i].enabled;
            hints[i].enabled = !hints[i].enabled;
        }
    }

    //각 분야별 취재노트에서 힌트 내용을 설정
    private void SetHint()
    {
        hints[0].text = GetHintFromXML(InterviewList.Category.Economy);
        hints[1].text = GetHintFromXML(InterviewList.Category.Entertainment);
        hints[2].text = GetHintFromXML(InterviewList.Category.Politics);
        hints[3].text = GetHintFromXML(InterviewList.Category.Science);
        hints[4].text = GetHintFromXML(InterviewList.Category.Society);
    }

    private string GetHintFromXML(InterviewList.Category category)
    {
        int nextIndex = playerManager.getNextInterview(category);
        InterviewList.Category comboCategory = Player.getPlayer().findComboCategory();
        if ((nextIndex == -1||nextIndex==0)&&!(Player.getPlayer().isCombo==true&&category==comboCategory))    //다한 경우
            return "";
        //else if (nextIndex == 0)        //해금되지 않은 경우
        //    return "";
        else if (Player.getPlayer().isCombo == true && comboCategory == category) //연계기사가 진행중일 경우 해당 카테고리 힌트를 바꿔줍니다.
        {
            int nextidx = playerManager.comboIndex;
            string filepath = Path.Combine(Path.Combine("XML", "Combo"), nextidx.ToString());
            filepath = Path.Combine(filepath, Type.Intro.ToString());
            TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(RestaurantMenu.text);
            XmlElement Todaymenu = Document[Type.Intro.ToString()];

            return Todaymenu.HasAttribute("Hint") ? Todaymenu.GetAttribute("Hint") : "일해라 기획자";
        }
        else
        {
            string filepath = Path.Combine(Path.Combine("XML", category.ToString()), nextIndex.ToString());
            filepath = Path.Combine(filepath, Type.Intro.ToString());
            TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(RestaurantMenu.text);
            XmlElement Todaymenu = Document[Type.Intro.ToString()];

            return Todaymenu.HasAttribute("Hint") ? Todaymenu.GetAttribute("Hint") : "일해라 기획자";
        }
    }

    private void ShowHero()
    {
        System.Random rnd = new System.Random();
        if(rnd.Next(0, 4) == 0)
        {
            List<string> tmp = new List<string>();

            TextAsset RestaurantMenu = (TextAsset)Resources.Load(Path.Combine("XML", "Hero"));
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(RestaurantMenu.text);
            
            XmlElement Todaymenu = Document["Hero"];



            foreach (XmlElement Dishes in Todaymenu.ChildNodes)
            {
                tmp.Add(Dishes.GetAttribute("Content"));
            }

            heroMessage.text = tmp[rnd.Next(0, tmp.Count - 1)];
            heroCanvas.enabled = true;
        }
    }

    public void HideHero()
    {
        heroCanvas.enabled = false;
    }

    public void ShowAlert()
    {
        alertCanvas.enabled = true;
    }

    public void HideAlert()
    {
        alertCanvas.enabled = false;
    }

    public void ShowDrawer()
    {
        drawerCanvas.enabled = true;
    }

    public void HideDrawer()
    {
        drawerCanvas.enabled = false;
    }

    private int drawerOffset = 0;
    public void SetDrawerDay(int offset = 0)
    {
        drawerOffset += offset;
        drawerOffset = Mathf.Max(0, Mathf.Min(playerManager.CheckDate() - 2, drawerOffset));
        
        for(int i = 0; i < 4; i++)
        {
            int tmp = playerManager.CheckDate() -1 - i - drawerOffset;
            drawerText[i].text = (tmp > 0 && tmp < playerManager.CheckDate()) ? tmp.ToString() : "";
        }
    }
    

    public void ShowNews()
    {
        ResultController r = newspaperCanvas.GetComponent<ResultController>();
        r.Load(drawerOffset);
        newspaperCanvas.enabled = true;
    }

    public void HideNews()
    {
        newspaperCanvas.enabled = false;
    }
    public bool ReadIntro(InterviewList.Category category, bool IPFlag=false)//true이면 paparazzi, false 이면 인터뷰
    {
        int nextIndex = playerManager.getNextInterview(category);
        if (nextIndex == -1)    //다한 경우
            Debug.Log("Error : 모든 인터뷰가 끝났습니다. 이것이 호출된다면 이 앞 함수에서 처리해 주세요.");
        else if (nextIndex == 0)        //해금되지 않은 경우
            Debug.Log("Error : 인터뷰가 잠금되었습니다. 이것이 호출된다면 이 앞 함수에서 처리해 주세요.");
        else
        {
            string filepath = Path.Combine(Path.Combine("XML", category.ToString()), nextIndex.ToString());
            filepath = Path.Combine(filepath, Type.Intro.ToString());
            TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(RestaurantMenu.text);
            XmlElement Todaymenu = Document[Type.Intro.ToString()];

            return Todaymenu.HasAttribute("IPFlag") ? System.Convert.ToBoolean(Todaymenu.GetAttribute("IPFlag")) : false;
        }
        return false;
    }
}
