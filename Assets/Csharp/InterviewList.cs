using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class InterviewList {

    public enum Category
    {
        Politics,     // 정치
        Society,      // 사회
        Economy,     // 경제
        Science,      // 과학
        Entertainment, // 연예
        Combo // 연계기사
    }

    public static InterviewList interviewList = null;
    public int numOfEconomy, numOfPolitics, numOfScience, numOfSociety, numOfEntertainment, numOfCombo;

    private InterviewList()
    {
        Debug.Log("InterviewList class is generated");

        XmlDocument doc = new XmlDocument();
        TextAsset textAst = (TextAsset)Resources.Load(Path.Combine("XML", "InterviewList"));
        doc.LoadXml(textAst.text);

        XmlElement content = doc["content"];

        numOfEconomy = System.Convert.ToInt32(content[Category.Economy.ToString()].GetAttribute("numbers"));
        numOfEntertainment = System.Convert.ToInt32(content[Category.Entertainment.ToString()].GetAttribute("numbers"));
        numOfPolitics = System.Convert.ToInt32(content[Category.Politics.ToString()].GetAttribute("numbers"));
        numOfScience = System.Convert.ToInt32(content[Category.Science.ToString()].GetAttribute("numbers"));
        numOfSociety = System.Convert.ToInt32(content[Category.Society.ToString()].GetAttribute("numbers"));
        numOfCombo = System.Convert.ToInt32(content[Category.Combo.ToString()].GetAttribute("numbers"));

    }

    public static InterviewList getInterviewList()
    {
        if(interviewList == null)
        {
            interviewList = new InterviewList();
        }

        return interviewList;
    }
    

    //category를 한글string으로 바꿀때 사용
    public static string CategoryToKorean(Category category)
    {
        Dictionary<Category, string> dic = new Dictionary<Category, string>()
        {
            { Category.Politics, "정치" },
            { Category.Society, "사회" },
            { Category.Economy, "경제" },
            { Category.Science, "과학" },
            { Category.Entertainment, "연예" },
            {Category.Combo, "연계" }
        };

        return dic[category];
    }

    //string을 category로 바꿀때 사용. 반대로 category를 string으로 바꾸는 경우는 그냥 ToString()하면 됨
    public static Category StringToCategory(string category)
    {
        Dictionary<string, Category> dic = new Dictionary<string, Category>()
        {
            { "Politics", Category.Politics},
            { "Society", Category.Society},
            { "Economy", Category.Economy},
            { "Science", Category.Science},
            { "Entertainment", Category.Entertainment},
            {"Combo",Category.Combo }
        };

        return dic[category];
    }
}
