using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;


public class Ending
{
    public int Id;
    public string EndingText;
}

public class EndingController : MonoBehaviour
{
    

    private static InterviewList.Category Part;
    

    private static int EndingFlag;
    
    public Image EndingImage;
    public Text Output;
    public List<Ending> Stream = new List<Ending>();

    private const float WAITING_TIME = 0.05f; // 이 시간마다 글자 출력.
    private float timer = 0.0f; // 시간
    private float turn = 0.0f; // 아직 안쓰는거임

    private string wholeText = ""; // 출력할 전체 텍스트

    private int currentIndex = 0; //현재 몇번째 글자를 출력해야 하는가
    private int index = 0; // 포인터같은거임, 아래 보면 이해될듯
    private int score = 0; // 점수
    private bool standby; // 내용 읽기전에 버튼 누르는거 방지
    private bool endOfStory; //이야기 끝났을까요?

    private static bool setSpecialEnding;
    private static bool sEFlag; // 특수 엔딩 구현!
    private static bool cat; // 고양이 엔딩


    void Start()
    {
        Screen.SetResolution(Screen.width, Screen.width * 9 / 16, true);
        Stream = ReadStream(Path.Combine(Path.Combine("XML", "Ending"), "Ending"));

        wholeText = Stream[0].EndingText.Replace("##", InterviewList.CategoryToKorean(Part));
        Output.text = "";
        index = 0;
        standby = false;
        endOfStory = false;
        setSpecialEnding = true; //일단 테스트에서는 고정!
        EndingImage.sprite = (Sprite)Resources.Load(Path.Combine("Ending", InterviewList.CategoryToKorean(Part)),typeof(Sprite));
    }

    void Update () 
    {
        timer += Time.deltaTime;

        if(!endOfStory)
        {
            if (timer > WAITING_TIME && currentIndex < wholeText.Length) // 다 출력 안했으면
            {
                Output.text += wholeText[currentIndex]; //한글자씩
                timer = 0.0f; //출력을
                ++currentIndex; //합니다
            }
            else if (currentIndex >= wholeText.Length)
            {
                standby = true;
            }
        }
        else
        {
            turn += Time.deltaTime;
            if(turn >= 2.0f)
            {
                SceneManager.LoadScene("StartMenu");
            }
        }
	}

    public void CallNext()
    {
        if (standby)
        {
            Output.text = "";
            if(index == 0)
            {
                index = EndingFlag;
            }
            else if(1 <= index && index <= 4)
            {
                index += 4;
            }
            else if(5 <= index && index <= 8)
            {
                if(setSpecialEnding)
                {
                    EndingImage.sprite = (Sprite)Resources.Load(Path.Combine("Ending", index.ToString()));
                    index += 4;
                }
                else
                {
                    endOfStory = true;
                }

            }
            else if((9 <= index && index <= 12) && cat)
            {
                EndingImage.sprite = (Sprite)Resources.Load(Path.Combine("Ending", "고양이엔딩"));
                index = 15;
            }
            else
            {
                endOfStory = true;
            }

            Debug.Log(Part);

            Output.text = "";
            currentIndex = 0;
            wholeText = Stream[index].EndingText;
            wholeText = wholeText.Replace("##", InterviewList.CategoryToKorean(Part));
            wholeText = wholeText.Replace("%%", "시공의 폭풍");
            wholeText = wholeText.Replace("$$", "히오스");
            timer = 0.0f;
            standby = false;
        }
    }

    public static void setFlag(int x, InterviewList.Category part)
    {
        EndingFlag = x;
        Part = part;
    }

    public List<Ending> ReadStream(string filepath)
    {
        Debug.Log(filepath);
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);

        XmlElement TodayMenu = Document["Ending"];

        List<Ending> Menu = new List<Ending>();

        foreach (XmlElement Dishes in TodayMenu.ChildNodes)
        {
            Ending ending = new Ending();
            ending.EndingText = Dishes.GetAttribute("Content").ToString();
            ending.Id = System.Convert.ToInt32(Dishes.GetAttribute("Id"));
            Menu.Add(ending);
        }
        return Menu;
    }
    public static void isCat()
    {
        cat = true;
    }
    public static void setSpecialEndingFlag(bool sE)
    {
        sEFlag = sE;
    }
}
