using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PaparazziController : MonoBehaviour
{
    public static string PATH;//읽을 리소스 위치

    public Camera mainCamera;
    public Image background;

    public Text xt;
    public Text yt;
    public Text timeLimit;
    public Text Score;

    public Vector2 target;

    public List<Box> Intros = new List<Box>();

    private const float screenMinX = -825.0f;
    private const float screenMinY = -405.0f;
    private const float screenMaxX = 825.0f;
    private const float screenMaxY = 405.0f;

    private float timer=0.0f;

    public  const float Speed = 0.25f;

    public Vector2 nowPos, prePos;
    public Vector3 movePos;


    private bool blackIntro;    //시작하기 전에 검은화면 인트로
    public Image blackImage;
    private const float FADEOUT_INTRO = 1f;
    private const int MAX_S_ENDING = 2;
    public Canvas blackCanvas;

    // Use this for initialization
    void Start()
    {
        
        //Sprite spr1 = (Sprite)Resources.Load("경로", typeof(Sprite));
        //background.sprite = spr1;
        target = readTargetPos(Path.Combine(PATH,"target"));
        Intros = ReadContent(Type.Intro);
        timer = 30.0f;

        //fadeout을 위한 blackImage 초기화, 그리고 인트로 첫부분을 검은화면에 띄우기위한 작업
        blackImage.GetComponentInChildren<Text>().text = Intros[0].Content;
    }

    public void FinishIntro()
    {
        blackCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update ()
    {
        
        Vector3 pos;
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            endPaparazzi();
        }
        timeLimit.text = String.Format("{0:f2}", timer);
        pos = mainCamera.transform.position;

        xt.text = pos.x.ToString();
        yt.text = pos.y.ToString();
        movePos.x = 0;
        movePos.y = 0;
        if (pos.x < screenMinX)
        {
            movePos.x = screenMinX - pos.x;
        }
        else if (pos.x > screenMaxX)
        {
            movePos.x = screenMaxX - pos.x;
        }
        if (pos.y < screenMinY)
        {
            movePos.y = screenMinY - pos.y;
        }
        else if (pos.y > screenMaxY)
        {
            movePos.y = screenMaxY - pos.y;
        }
        mainCamera.transform.Translate(movePos);
    }

    void Awake()
    {
        Screen.SetResolution(Screen.height * 16 / 9, Screen.height, false);

    }
    //XML읽어 TARGET위치 설정 함수
   /* public void makeXmlFormat()
    {

        XmlWriterSettings settings = new XmlWriterSettings();
        const int ZERO = 0;
        settings.Indent = true;
        settings.IndentChars = ("\t");

        string path = Path.Combine(Application.persistentDataPath, "target.xml");

        using (XmlWriter writer = XmlWriter.Create(path, settings))
        {
            writer.WriteStartDocument();

            //시작
            writer.WriteStartElement("content");
            

            //TargetPosition
            writer.WriteStartElement("targetPosition");
            writer.WriteAttributeString("x", ZERO.ToString());
            writer.WriteAttributeString("y", ZERO.ToString());
            writer.WriteEndElement(); //TargetPosition


            //끝
            writer.WriteEndElement();   //content 끝

            writer.WriteEndDocument();
            writer.Close();
        }
        Debug.Log("Successfulliy Saved. : " + Path.Combine(Application.persistentDataPath, "target.xml"));
    }*/
    public Vector2 readTargetPos(string filepath)
    {
        double x = 0;
        double y = 0;
        Debug.Log(filepath);
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);
        
        XmlElement Todaymenu = Document["content"];
        foreach (XmlElement Dishes in Todaymenu.ChildNodes)
        {

            x = (Dishes.HasAttribute("x")) ? System.Convert.ToDouble(Dishes.GetAttribute("x")) : 0;
            y = (Dishes.HasAttribute("y")) ? System.Convert.ToDouble(Dishes.GetAttribute("y")) : 0;
            Debug.Log("x = " + x + ", y = " + y);
        }
        Vector2 result = new Vector2((float)x, (float)y);

        return result;

    }
    //정 중앙을 터치해(UPDATE함수에서 처리) TARGET을 얼마나 포함하는가 처리
    public float targetFind()
    {
        float result = 0.0f;

        return result;
    }
    //PLAYER에 점수 RETURN 함수+기사 출력
    public void endPaparazzi()
    {
        Vector2 cameraPos = mainCamera.transform.position;
        int score,index;
        float distance = (float)Math.Sqrt(Math.Pow((target.x - cameraPos.x), 2) + Math.Pow((target.y - cameraPos.y), 2));
        index = OtherEnding();
        if (index == 0)
        {
            Player.getPlayer().setCat(true);
        }
        else if (index == 1)
        {
            SpecialEndingController.setSPEnding("v");
            SceneManager.LoadScene("SpEndingScene");
        }
        else
        {

            if (distance < 0)
                score = -1;
            else if (distance < 100)
                score = 25;
            else
                score = 5;
            if (score == -1)
                Debug.Log("Distance Error");
            Debug.Log("c.x = " + cameraPos.x + "c.y" + cameraPos.y);
            Debug.Log("Score : " + score);
            Player.getPlayer().saveScore(score);
            Player.getPlayer().SaveGameState();
            SceneManager.LoadScene("ResultScene"); //결과 출력 씬으로
        }

    }
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
    public static void MakePath(InterviewList.Category category, int number)
    {
        PATH = Path.Combine(Path.Combine("XML", category.ToString()), number.ToString());       //XML주소를 설정하고
        Player.getPlayer().addLastArticle(category, number);        //최근에 플레이한 인터뷰 종류와 인덱스를 Player에 기록
    }
    public int OtherEnding()
    {
        Vector2 cameraPos = mainCamera.transform.position;
        int index = 0;
        List<Vector2> target = new List<Vector2>();
        target.Add(new Vector2(-336.25f, -40.75f));
        target.Add(new Vector2(199.0f, 88.75f));
        for (index = 0; index < MAX_S_ENDING; index++)
        {
            float distance = (float)Math.Sqrt(Math.Pow((target[index].x - cameraPos.x), 2) + Math.Pow((target[index].y - cameraPos.y), 2));

            if (distance<66.6f)
            {
                return index;
            }
        }
        return -1;//특수 Ending 없음
    }
}
