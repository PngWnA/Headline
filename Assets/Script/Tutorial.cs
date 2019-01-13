using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;


public class Dialog
{
    public string Content;
    public int Id;

    public Dialog(string Content, int Id = 0)
    {
        this.Content = Content;
        this.Id = Id;
    }
}



public class Tutorial : MonoBehaviour
{
    public Button messageBox;
    public Button skipButton;

    private string PATH;

    private string wholeText = "";
    private string currentText = "";

    private float timer = 0.0f;
    private float WAITING_TIME = 0.03f;

    private int currentIndex = 0;
    private int index = 0;
    
    private bool endOfStory;

    public List<Dialog> Stream = new List<Dialog>();


    void Start()
    {
        Stream = ReadContent(Path.Combine(Path.Combine("XML", "Tutorial"), "Tutorial"));
        wholeText = Stream[index].Content;

        index = 0;
        endOfStory = false;
    }

    void Update()
    {
        if (!endOfStory)
        {
            timer += Time.deltaTime;
            messageBox.GetComponentInChildren<Text>().text = currentText;

            if (timer > WAITING_TIME && currentIndex < wholeText.Length)
            {
                currentText += wholeText[currentIndex]; //한글자씩
                timer = 0.0f; //출력을
                ++currentIndex; //합니다
            }
        }
        else
        {
            timer += Time.deltaTime;

            if (timer > 1.0f)
            {
                SceneManager.LoadScene("MainScene");
            }
        }
    }

    private List<Dialog> ReadContent(string filepath)
    {
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);

        List<Dialog> result = new List<Dialog>();
        XmlElement Todaymenu = Document["Tutorial"];

        foreach (XmlElement Dishes in Todaymenu.ChildNodes)
        {
            int tmpID = (Dishes.HasAttribute("Id")) ? System.Convert.ToInt32(Dishes.GetAttribute("Id")) : 0;
            Dialog answer = new Dialog(Dishes.GetAttribute("Content"), tmpID);
            result.Add(answer);

        }
        return result;
    }

    //버튼을 누르면, 모든 문자를 다 출력시키고, 이미 다 출력시켰으면 다음 문장.
    public void CallNext()
    {
        if (currentIndex < wholeText.Length) // 다 출력 안했으면
        {
            currentText = wholeText; // 다 출력시켜버립니다.
            currentIndex = wholeText.Length; // Index도 수정하고요
        }

        else
        {
            index++;

            if(index >= Stream.Count)   //모든 문장이 다 끝나면 메인씬으로 넘어감
            {
                Skip();
            }

            else
            {
                wholeText = Stream[index].Content;
                currentText = "";
                currentIndex = 0;
                timer = 0.0f;
            }

        }
    }

    public void Skip()
    {
        messageBox.interactable = false;
        messageBox.GetComponent<Image>().FadeOut(0.5f);
        messageBox.GetComponentInChildren<Text>().FadeOut(0.5f);
        skipButton.interactable = false;
        skipButton.GetComponent<Image>().FadeOut(0.5f);
        skipButton.GetComponentInChildren<Text>().FadeOut(0.5f);
        endOfStory = true;
        timer = 0.0f;
    }
}