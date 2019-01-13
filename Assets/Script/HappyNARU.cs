using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

// P: 3개의 클래스는 공통점이 뚜렷하므로, 하나의 클래스로 통합, 타입은 enum으로 구분
// 여기있는 클래스 3개는 구조체라고 생각하시면 됩니다.
public enum Type { Intro, Question, Answer }

public class Box
{
    public Type Type;
    public string Content;
    public int Id;
    public int Score;

    public Box(Type Type, string Content, int Id = 0, int Score = 0)
    {
        this.Type = Type;
        this.Content = Content;
        this.Id = Id;
        this.Score = Score;
    }
}

public class HappyNARU : MonoBehaviour
{
    // 문장 출력에 관련된 부분입니다

    public static string PATH;//읽을 XML위치

    public Image background;
    public Image interviewee;
    public CanvasGroup note;
    
    private const string DELAY = "           ";      //끝부분 delay용
    private const int MAXLINE_ANSWER = 95;     //인터뷰 내용의 최대 출력 글자수
    private const int MAXLINE_QUESTION = 45;
    private const float WAITING_TIME = 0.04f; // 이 시간마다 글자 출력.
    private const float FADEOUT_INTRO = 1f;      //검은 인트로 화면 fadeOut 속도
    private const float FADEIN_NOTE = 0.5f;    //노트의 fadeIn 속도
    private const float FADEOUT_MAIN = 2f;      //인터뷰 종료후 fadeOut 속도
    private float timer = 0.0f; // 시간

    private List<string> wholeText = new List<string>(); // 출력할 전체 텍스트
    private string currentAnswerText = ""; // 현재까지 출력된 텍스트

    private int currentIndex = 0; //현재 몇번째 글자를 출력해야 하는가
    private int indexOfWholeText = 0;
    private int index = 0; // 포인터같은거임, 아래 보면 이해될듯
    private int score = 0; // 점수

    private bool blackIntro;    //시작하기 전에 검은화면 인트로
    private bool endOfStory; //이야기 끝났을까요?
    private bool introSet; //인트로 땜빵용
    private bool endflag; //땜빵용22

    // 우리가 클릭하는 버튼
    public Button[] Buttons = new Button[4];
    

    // 점수
    public Image Score;

    //XML 관련된 부분입니다.
    public List<Box> Intros = new List<Box>(); // XML 구조체 선언
    public List<Box> Questions = new List<Box>();
    public List<Box> Answers = new List<Box>();
    public Text IntervieweeName;    //인터뷰이 이름과 나이
    public Text OutputText;     //인터뷰 내용이 나오는 곳
    public Text QuestionText;       //질문 직후에 나오는 노란색 질문내용

    //fadeOut 관련부분
    public Image blackImage;


    void Start()
    {
        //인터뷰가 담긴 XML 읽기
        Debug.Log("PATH=" + PATH);
        interviewee.sprite = (Sprite)Resources.Load(Path.Combine(PATH, "interviewee"), typeof(Sprite));        
        background.sprite = (Sprite)Resources.Load(Path.Combine(PATH, "background"), typeof(Sprite));

        EnableNote(false);  //노트 비활성화

        Intros = ReadContent(Type.Intro);
        Questions = ReadContent(Type.Question);
        Answers = ReadContent(Type.Answer);

        // 초기화를 시켜야겠죠?
        index = 0;
        blackIntro = true;
        endOfStory = false;
        introSet = true;
        endflag = false;
        Score.gameObject.SetActive(false);
        shuffle();      //대답 버튼 셔플
        
        //fadeout을 위한 blackImage 초기화, 그리고 인트로 첫부분을 검은화면에 띄우기위한 작업
        bool tooShortIntro = Intros.Count <= 1;
        blackImage.GetComponentInChildren<Text>().text = tooShortIntro ? "인트로를 덜 적은 기획자와 인터뷰해보자" : Intros[0].Content;

        //첫부분을 제외한 나머지 인트로는 대사로.
        for (int i = 1; i < Intros.Count; i++)
        {
            List<string> tmp = Split(Intros[i].Content, MAXLINE_ANSWER);
            foreach (string s in tmp)
            {
                wholeText.Add(s);
            }
        }

        
        //마지막에는 딜레이를 위해 추가
        wholeText[wholeText.Count - 1] += DELAY;
    }


    void Update()
    {
        if (blackIntro)        //시작하기 전에 검은화면 인트로 부분
        {
            if (!blackImage.GetComponent<Button>().interactable)
            {
                timer += Time.deltaTime;

                if(timer > FADEOUT_INTRO)
                {
                    blackImage.enabled = false;
                    blackImage.GetComponentInChildren<Text>().enabled = false;
                    blackIntro = false;
                    timer = 0.0f;
                }
            }

        }
        else
        {
            //검은화면 인트로가 끝나고 이제부터 인터뷰 시작
            if (!endOfStory) //이야기가 안끝났으면
            {
                timer += Time.deltaTime;

                OutputText.text = currentAnswerText; //유니티에 출력하려고 일단 이렇게 썼네요
                if (timer > WAITING_TIME && currentIndex < wholeText[indexOfWholeText].Length) // 다 출력 안했으면
                {
                    currentAnswerText += wholeText[indexOfWholeText][currentIndex]; //한글자씩
                    timer = 0.0f; //출력을
                    ++currentIndex; //합니다
                }

                if (currentIndex >= wholeText[indexOfWholeText].Length) // 말을 다 출력했으면
                {
                    if (indexOfWholeText < wholeText.Count - 1)
                    {
                        currentAnswerText = "";
                        currentIndex = 0;
                        indexOfWholeText++;     //다음 내용을 읽는다
                        timer = 0.0f;
                    }

                    else
                    {
                        try // 읽어올게 있으면
                        {
                            if (endflag)
                            {
                                EnableNote(false); //버튼을 비활성화시키고
                                endOfStory = true;
                                timer = 0.0f;
                            }
                            else if (!note.blocksRaycasts)
                            {
                                string[] tmp = new string[4];

                                for (int i = 0; i < tmp.Length; i++)
                                {
                                    tmp[i] = Questions[(int)(index / 10) * 4 + i].Content;      //질문 내용을 불러온다.
                                    Buttons[i].GetComponentInChildren<Text>().text = tmp[i];    //질문내용을 버튼에 대입
                                    int fontSize = tmp[i].Length > 55 ? (tmp[i].Length > 85 ? 17 : 20) : 25;
                                    Buttons[i].GetComponentInChildren<Text>().fontSize = fontSize; //질문이 길면 사이즈 조절
                                }

                                EnableNote(true);   //이제부터 버튼 클릭 가능!
                            }
                        }

                        catch // 더 이상 읽어올게 없으면
                        {
                            EnableNote(false); //버튼을 비활성화시키고
                            endOfStory = true; // 이야기를 끝냅니다.
                            timer = 0.0f;
                        }
                    }
                }

            }
            else
            {
                //모든 질문이 끝나고 나서 마무리 부분
                timer += Time.deltaTime;

                if (timer >= 1.0f) //1초가 지나면
                {
                    QuestionText.text = ""; //노란색으로 선택한 질문을 출력
                    OutputText.text = "(인터뷰 종료)"; //노란색으로 선택한 질문을 출력

                    if (!blackImage.enabled)        //fade out
                    {
                        blackImage.enabled = true;
                        blackImage.FadeIn(FADEOUT_MAIN);        //검은 화면을 fadeIn해야 전체씬은 fadeOut되는 것 처럼 보임
                    }

                    if (timer >= 3.3f)
                    {
                        Player.getPlayer().saveScore(Scoring());
                        Player.getPlayer().SaveGameState();
                        SceneManager.LoadScene("ResultScene"); //결과 출력 씬으로
                    }
                }
            }

        }


    }
    void Awake()
    {
        if (PATH == null)
        {
            Debug.Log("Error: No Path");
            SceneManager.LoadScene("MainScene");
        }
    }

    private void shuffle()
    {
        Vector3[] positions = new Vector3[4];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = Buttons[i].transform.position;
        }

        System.Random rnd = new System.Random();
        positions = positions.OrderBy(x => rnd.Next()).ToArray();

        for (int i = 0; i < positions.Length; i++)
        {
            Buttons[i].transform.position = positions[i];
        }
    }

    //불러올 XML의 주소를 가져온다
    public static void MakePath(InterviewList.Category category, int number)
    {
        InterviewList.Category comboCategory = Player.getPlayer().findComboCategory();
        if (Player.getPlayer().isCombo == true && comboCategory == category)
        {
            PATH = Path.Combine(Path.Combine("XML", "Combo".ToString()), Player.getPlayer().comboIndex.ToString());       //XML주소를 설정하고
            Player.getPlayer().addLastArticle(InterviewList.Category.Combo, number);        //최근에 플레이한 인터뷰 종류와 인덱스를 Player에 기록
        }
        else
        {
            PATH = Path.Combine(Path.Combine("XML", category.ToString()), number.ToString());       //XML주소를 설정하고
            Player.getPlayer().addLastArticle(category, number);        //최근에 플레이한 인터뷰 종류와 인덱스를 Player에 기록
        }

    }

    // P: read 함수를 하나로 통합
    private List<Box> ReadContent(Type type)
    {
        string filepath = Path.Combine(PATH, type.ToString());
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);
        XmlElement Todaymenu = Document[type.ToString()];
        List<Box> result = new List<Box>();
        

        //Intro.xml에서 interviewee 이름 나이 로드
        if (type == Type.Intro)
        {
            string name = Todaymenu.HasAttribute("Name") ? Todaymenu.GetAttribute("Name") : "한나루";
            string age = Todaymenu.HasAttribute("Age") ? Todaymenu.GetAttribute("Age") : "15";
            IntervieweeName.text = name + "(" + age + ")";
        }

        foreach (XmlElement Dishes in Todaymenu.ChildNodes)
        {
            int tmpID = (Dishes.HasAttribute("Id")) ? System.Convert.ToInt32(Dishes.GetAttribute("Id")) : 0;
            int tmpScore = (Dishes.HasAttribute("Score")) ? System.Convert.ToInt32(Dishes.GetAttribute("Score")) : 0;
            Box answer = new Box(type, Dishes.GetAttribute("Content"), tmpID, tmpScore);
            result.Add(answer);
        }

        return result;
    }


    //대답 버튼을 누를 때
    public void CallNext(int num)
    {
        string scorePath = "Interview";
        shuffle();      //대답 버튼 셔플

        //버튼에 있던 내용과 대답 내용을 clear
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].GetComponentInChildren<Text>().text = "";
        }
        currentAnswerText = "";

        //선택한 질문을 노란색으로 출력. 길면 자름
        string orignalQuestion = Questions[(int)(index / 10) * 4 + 3 - num].Content;
        QuestionText.text = orignalQuestion.Length > MAXLINE_QUESTION ? orignalQuestion.Substring(0, MAXLINE_QUESTION - 3) + "..." : orignalQuestion;

        //출력할 대답을 찾아서 출력을 준비하고, 길면 자릅니다
        indexOfWholeText = 0;
        string originalAnswer = Answers[(int)(index / 10) * 4 + 3 - num].Content;
        endflag = originalAnswer.Contains("-x-");      //분기인지 판단.
        originalAnswer = originalAnswer.Replace("-x-", "");     //-x-를 떼어낸다.
        wholeText.Clear();
        wholeText = Split(originalAnswer, MAXLINE_ANSWER);

        score += num; //점수를 추가시킵니다.
                      //Score.text = "Score : " + score.ToString(); //점수도 출력을 할거고
        
        Score.sprite = (Sprite)Resources.Load(Path.Combine(scorePath, "score"+num.ToString()), typeof(Sprite));
        Score.gameObject.SetActive(true);
        index += 10; //index를 이동시키고
        EnableNote(false);  //노트를 숨기고
        currentIndex = 0; //처음부터 출력할 준비하고
        timer = 0.0f; //지금부터 시작!
    }

    //string을 쪼갠다
    private List<string> Split(string str, int chunkSize)
    {
        return Enumerable.Range(0, (int)Mathf.Ceil((float)str.Length / chunkSize))
            .Select(i => str.Substring(i * chunkSize, (i * chunkSize + chunkSize <= str.Length) ? chunkSize : str.Length - i * chunkSize) + DELAY).ToList();  //딜레이를 위해 약간의 공백 삽입
    }

    //대화창을 누른 경우 대화를 스킵합니다
    public void Skip() 
    {
        if (currentIndex < wholeText[indexOfWholeText].Length) // 다 출력 안했으면
        {
            currentAnswerText = wholeText[indexOfWholeText]; // 다 출력시켜버립니다.
            currentIndex = wholeText[indexOfWholeText].Length; // Index도 수정하고요
        }
    }

    //인트로창을 누른 경우 인트로를 끝냅니다
    public void FinishIntro()
    {
        blackImage.GetComponent<Button>().interactable = false;
        blackImage.FadeOut(FADEOUT_INTRO);
        blackImage.GetComponentInChildren<Text>().FadeOut(FADEOUT_INTRO);
    }

    //점수를 낸다
    private int Scoring()
    {
        int maxScore = (Questions.Count / 4) * 3;
        int tmp = ((int)(((score * 6) - 1) / maxScore)) * 5;
        tmp = (tmp > 25 || tmp < 0) ? -1 : tmp;

        return tmp;
    }

    //노트와 하위 버튼들을 enable/disable시킨다
    private void EnableNote(bool enable)
    {
        note.blocksRaycasts = enable;
        note.GetComponent<Image>().enabled = enable;
        if (enable)
            note.GetComponent<Image>().FadeIn(FADEIN_NOTE);

        foreach(Button i in Buttons)
        {
            i.enabled = enable;
            i.interactable = enable;
            i.GetComponentInChildren<Text>().enabled = enable;
            if (enable)
                i.GetComponentInChildren<Text>().FadeIn(FADEIN_NOTE);
        }
    }
}
