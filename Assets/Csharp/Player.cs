using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Linq;



public class Player
{
    private static Player player = null;
    private int playerNum = -1;

    //연계기사 확인용
    public bool isCombo;
    public bool isComboStarted;
    public int comboIndex = 1;
    public InterviewList.Category comboCategory;
    public int comboDate = 0;
    public string PATH;
    public bool cat;
    //이전의 신묻들을 불러오기 위해 저장하는 구조체
    private class Article
    {
        public InterviewList.Category category;
        public int number;

        public Article(InterviewList.Category category, int number)
        {
            this.category = category;
            this.number = number;
        }
    }
    private List<Article> lastArticleList = new List<Article>();
    
    private string playerFileName;

    //각 분야별의 모든 인터뷰들에 대한 점수를 저장. -1인 경우 플레이를 하지 않은 경우를 의미
    private Dictionary<InterviewList.Category, int[]> score = new Dictionary<InterviewList.Category, int[]>();
    
    private int totalScore;//총 점수
    private InterviewList interviewList;

    //각 분야별의 다음 인터뷰 인덱스를 저장. 매번 불러올 때마다 랜덤으로 달라진다. -1인 경우 모든 인터뷰를 플레이함을 의미
    private Dictionary<InterviewList.Category, int> nextInterviewIndex = new Dictionary<InterviewList.Category, int>();
    

    //private 기본 생성자, 최초 생성시에만 호출
    private Player()
    {
        Debug.Log("Player class is generated");
        interviewList = InterviewList.getInterviewList();
        this.totalScore = 0;

        //score 내의 모든 인자를 -1로 대입. -1은 아직 플레이하지않음을 의미
        score.Add(InterviewList.Category.Economy, Enumerable.Repeat(-1, interviewList.numOfEconomy).ToArray());
        score.Add(InterviewList.Category.Entertainment, Enumerable.Repeat(-1, interviewList.numOfEntertainment).ToArray());
        score.Add(InterviewList.Category.Politics, Enumerable.Repeat(-1, interviewList.numOfPolitics).ToArray());
        score.Add(InterviewList.Category.Science, Enumerable.Repeat(-1, interviewList.numOfScience).ToArray());
        score.Add(InterviewList.Category.Society, Enumerable.Repeat(-1, interviewList.numOfSociety).ToArray());
        score.Add(InterviewList.Category.Combo, Enumerable.Repeat(-1, interviewList.numOfCombo).ToArray());

        //nextInterviewIndex 내의 모든 인자를 0을 대입
        nextInterviewIndex.Add(InterviewList.Category.Economy, 0);
        nextInterviewIndex.Add(InterviewList.Category.Entertainment, 0);
        nextInterviewIndex.Add(InterviewList.Category.Politics, 0);
        nextInterviewIndex.Add(InterviewList.Category.Science, 0);
        nextInterviewIndex.Add(InterviewList.Category.Society, 0);
        nextInterviewIndex.Add(InterviewList.Category.Combo, 0);
    }

    //싱글톤
    public static Player getPlayer()
    {
        if (player == null)
        {
            player = new Player();
        }
        return player;
    }
    
    //ResultController에서 기사 면에 넣을 이미지 파일들의 주소를 부른다.
    public string getLastArticle(int daysAgo)
    {
        int index = lastArticleList.Count - 1 - daysAgo;
        index = Mathf.Max(0, Mathf.Min(lastArticleList.Count - 1, index));

        return Path.Combine(Path.Combine("XML", lastArticleList[index].category.ToString()), lastArticleList[index].number.ToString());
    }

    //인터뷰가 끝난 뒤 최근 인터뷰를 리스트에 추가하는 함수
    public void addLastArticle(InterviewList.Category curIA, int curIN)
    {
        lastArticleList.Add(new Article(curIA, curIN));
    }

    public InterviewList.Category lastCategory()
    {
        return lastArticleList.Last().category;
    }

    //ResultController에서, 기사 몇면에 쓰일지 판단. 처음에는(0일때는) -1 반환
    public int getResultNumber(int daysAgo) 
    {
        int index = lastArticleList.Count - 1 - daysAgo;

        if (index < 0 || index >= lastArticleList.Count)
            return -1;
        else if (lastArticleList[index].number == 0)
            return -1;
        else
            return 6 - score[lastArticleList[index].category][lastArticleList[index].number - 1] / 5;
    }

    //다음 인터뷰가 뭔지 랜덤으로 설정한다. (게임 시작 및 로드, 인터뷰 종료시 호출된다). -1인 경우 모든 인터뷰를 다함. 0인 경우 해금이 아직 안 됨 (0은 아직 구현 안 됨)
    public void setNextInterview()
    {
        comboDate++; //2턴이 지났는지 확인하는 변수

        if (lastArticleList.Last().category == InterviewList.Category.Combo && comboIndex<6)
        {
            comboIndex++; //이전 플레이가 연계기사였으면 인덱스를 다음으로 옮겨준다.
            setComboInterview(); //연계기사 설정
            comboDate = 0; //연계기사를 플레이했으니 2턴 카운터를 초기화
        }
        else if (comboDate < 2&&comboIndex<6) //이전 플레이는 연계기사가 아니지만 2턴이 안지남
        {
            nextInterviewIndex[lastArticleList.Last().category] = nextInterview(lastArticleList.Last().category);       //방금 인터뷰한 분야는 다음걸 찾는다.
            setComboInterview();
        }
        else //2턴이 지나서 연계기사는 빠이빠이했어요 ^^
        {
            nextInterviewIndex[lastArticleList.Last().category] = nextInterview(lastArticleList.Last().category);       //방금 인터뷰한 분야는 다음걸 찾는다.
            isCombo = false;
        }
    }

    public void setComboInterview()
    {

        if (comboIndex == 0) 
        {
            comboIndex = 1;
        }
        int nextIndex = comboIndex;
        PATH = Path.Combine("XML", "Combo");
        PATH = Path.Combine(PATH, nextIndex.ToString());

        string filepath = Path.Combine(PATH, "Intro".ToString());
        Debug.Log("filepath는 이것이다 ! : " + filepath);
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);
        XmlElement Todaymenu = Document["Intro".ToString()];

        if (comboDate > 2) //2턴이 지나서 연계기사는 빠이빠이했어요 ^^
        {
            //다음 인터뷰는 연계기사 Intro의 Combo attribute에 따라 설정됩니다.
            nextInterviewIndex[InterviewList.StringToCategory(Todaymenu.GetAttribute("Combo"))] = nextInterview(InterviewList.StringToCategory(Todaymenu.GetAttribute("Combo")));
            isCombo = false;

        }
        else
        {
            nextInterviewIndex[InterviewList.Category.Combo] = nextInterview(InterviewList.Category.Combo);
        }
    }

    //연계기사의 카테고리를 찾아줍니다.
    public InterviewList.Category findComboCategory()
    {
        int nextIndex = Player.getPlayer().comboIndex;
        PATH = Path.Combine("XML", "Combo");
        PATH = Path.Combine(PATH, nextIndex.ToString());

        string filepath = Path.Combine(PATH, "Intro".ToString());
        Debug.Log("filepath는 이것이다 ! : " + filepath);
        TextAsset RestaurantMenu = (TextAsset)Resources.Load(filepath, typeof(TextAsset));
        XmlDocument Document = new XmlDocument();
        Document.LoadXml(RestaurantMenu.text);
        XmlElement Todaymenu = Document["Intro".ToString()];

        return InterviewList.StringToCategory(Todaymenu.GetAttribute("Combo"));
    }

    //다음 인터뷰의 인덱스를 가져온다. 랜덤 함수가 포함됨
    private int nextInterview(InterviewList.Category category)
    {
        if (category == InterviewList.Category.Combo)
            return comboIndex;
        int[] scoreOfCategory = score[category];    //선택된 category의 인터뷰의 점수들을 담은 배열들

        System.Random rnd = new System.Random();
        int[] randomIndex = Enumerable.Range(0, scoreOfCategory.Length).ToArray().OrderBy(x => rnd.Next()).ToArray();    //0부터 n-1까지 랜덤하게 들어있는 배열

        foreach (int index in randomIndex)
        {
            if (scoreOfCategory[index] < 0)
                return index + 1;   //하지 않은 인덱스를 반환. 인덱스 0이 1번째니까, 인덱스 1가 2번째니까 1을 더한다.
        }

        if (findComboCategory()==category)
        {
            return 0;
        }
        else
            return -1;      //모든 인터뷰를 다한 경우는 -1을 반환
    }

    public int getNextInterview(InterviewList.Category category)
    {
        return nextInterviewIndex[category];
    }

    //플레이어 1인지 2인지 3인지 판단
    public int getPlayerNum()
    {
        if(playerNum == -1)
        {
            Debug.Log("PlayerNum is not initiallized");
        }
        return playerNum;
    }

    //Ending이 활성화되는가 되지않는가를 나타내는함수
    public bool CheckEnding()
    {
        int totalNumOfInterview = interviewList.numOfEconomy + interviewList.numOfEntertainment + interviewList.numOfPolitics + interviewList.numOfScience + interviewList.numOfSociety;
    
        return CheckDate() - 1 == totalNumOfInterview;
    }

    //날짜계산
    public int CheckDate()
    {
        int check = 1;

        foreach(InterviewList.Category category in score.Keys)
        {
            foreach(int i in score[category])
            {
                if (i >= 0)
                    check++;
            }
        }

        return check;
    }
    //Ending 번호
    public int EndingNumber()
    {
        float res = (float)player.totalScore / (float)Allscore();
        if (res > 1.0f)
        {
            Debug.Log("Error...Over Totalscore");
            return -1;
        }
        else if (res > 0.8f)
        {
            return 1;//1번 엔딩
        }
        else if (res > 0.6f)
        {
            return 2;//2번 엔딩
        }
        else if (res > 0.4f)
        {
            return 3;//3번 엔딩
        }
        else if (res > 0.2f)
        {
            return 4;//4번 엔딩
        }
        else if (res >= 0.0f) //0점은 가능한 점수겠죠...?
        {
            return 5;//5번 엔딩
        }
        else
        {
            Debug.Log("Error...Under Totalscore 0");
            return -1;
        }

    }
    //만점
    public int Allscore()
    {
        int sum = 0;
        sum += interviewList.numOfEconomy;
        sum += interviewList.numOfEntertainment;
        sum += interviewList.numOfPolitics;
        sum += interviewList.numOfScience;
        sum += interviewList.numOfSociety;
        sum += interviewList.numOfCombo;
        return sum * 25;
    }
    public void selectGameData(int num)
    {
        playerNum = num;
        playerFileName = "player_" + num.ToString() + ".xml";
        LoadGameState();

        //다음 인터뷰 설정
        foreach (InterviewList.Category c in score.Keys)
        {
            nextInterviewIndex[c] = nextInterview(c);
        }
    }
    public void makeGameData(int num)
    {
        playerNum = 1;
        playerFileName = "player_" + num.ToString() + ".xml";
        SaveGameState();

        //다음 인터뷰 설정
        foreach (InterviewList.Category c in score.Keys)
        {
            nextInterviewIndex[c] = nextInterview(c);
        }
    }

    public void addTotalScore(int score)
    {
        player.totalScore += score;
    }
    public int getTotalScore()
    {
        return player.totalScore;
    }

    public void saveScore(int score)
    {
        this.score[lastArticleList.Last().category][lastArticleList.Last().number - 1] = score;
        addTotalScore(score);
    }



    //게임 상태 저장
    public void SaveGameState()
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.IndentChars = ("\t");

        string path = Path.Combine(Application.persistentDataPath, this.playerFileName);

        using (XmlWriter writer = XmlWriter.Create(path, settings))
        {
            writer.WriteStartDocument();
            writer.WriteComment(" Saved game data of the game HeadLine");

            //시작
            writer.WriteStartElement("content");


            //이름 기록
            writer.WriteStartElement("user");
            //writer.WriteAttributeString("Version", Version.Number.ToString())
            writer.WriteAttributeString("playerFileName", this.playerFileName);
            writer.WriteEndElement();

            //totalScore 기록
            writer.WriteStartElement("asset");
            writer.WriteAttributeString("totalScore", this.totalScore.ToString());
            writer.WriteEndElement(); //Asset끝

            //연계기사 진행여부 기록
            writer.WriteStartElement("combo");
            writer.WriteAttributeString("isCombo", this.isCombo.ToString());
            writer.WriteAttributeString("isComboStarted", this.isComboStarted.ToString());
            writer.WriteAttributeString("comboIndex", this.comboIndex.ToString());
            writer.WriteEndElement();


            writer.WriteStartElement("Special");
            writer.WriteAttributeString("cat", this.isCombo.ToString());
            writer.WriteEndElement();

            foreach (InterviewList.Category category in score.Keys)
            {
                writer.WriteStartElement(category.ToString());        //Economy 등의 카테고리 기록
                int counter = 0;
                foreach (int i in score[category])
                {
                    writer.WriteAttributeString("num" + counter, i.ToString());
                    counter++;
                }
                writer.WriteEndElement();
            }
            
            writer.WriteStartElement("LastInterviewList");
            foreach(Article i in lastArticleList)
            {
                writer.WriteStartElement("Interviews");
                writer.WriteAttributeString("category", i.category.ToString());
                writer.WriteAttributeString("number", i.number.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();


            //끝
            writer.WriteEndElement();   //content 끝

            writer.WriteEndDocument();
            writer.Close();
        }
        Debug.Log("Successfulliy Saved. : " + Path.Combine(Application.persistentDataPath, playerFileName));

    }

    //게임 상태 로드
    private void LoadGameState()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(Path.Combine(Application.persistentDataPath, playerFileName));

        XmlElement content = doc["content"];

        this.playerFileName = content["user"].GetAttribute("playerFileName");
        this.totalScore = System.Convert.ToInt32(content["asset"].GetAttribute("totalScore"));
        this.isCombo = System.Convert.ToBoolean(content["combo"].GetAttribute("isCombo"));
        this.isComboStarted = System.Convert.ToBoolean(content["combo"].GetAttribute("isComboStarted"));
        this.comboIndex = System.Convert.ToInt32(content["combo"].GetAttribute("comboIndex"));
        this.cat = System.Convert.ToBoolean(content["Special"].GetAttribute("cat"));

        foreach (InterviewList.Category category in score.Keys)
        {
            int counter = 0;
            foreach (int i in score[category])
            {
                this.score[category][counter] = System.Convert.ToInt32(content[category.ToString()].GetAttribute("num" + counter));
                counter++;
            }
        }
        
        foreach(XmlElement e in content["LastInterviewList"])
        {
            lastArticleList.Add(new Article(InterviewList.StringToCategory(e.GetAttribute("category")), System.Convert.ToInt32(e.GetAttribute("number"))));
        }
        Debug.Log("Successfulliy Loaded. : " + Path.Combine(Application.persistentDataPath, playerFileName));
        Debug.Log("LastInterview#: " + lastArticleList.Count);
    }

    public InterviewList.Category getMost()
    {
        int[] scorelist = new int[5];
        int most = 0;

        scorelist[0] = getScore(InterviewList.Category.Economy);
        scorelist[1] = getScore(InterviewList.Category.Entertainment);
        scorelist[2] = getScore(InterviewList.Category.Politics);
        scorelist[3] = getScore(InterviewList.Category.Science);
        scorelist[4] = getScore(InterviewList.Category.Society);

        for (int i = 0; i < scorelist.Length - 1; i++)
        {
            if (scorelist[i] <= scorelist[i + 1])
                most = i + 1;
        }

        switch (most)
        {
            case 0:
                return InterviewList.Category.Economy;
            case 1:
                return InterviewList.Category.Entertainment;
            case 2:
                return InterviewList.Category.Politics;
            case 3:
                return InterviewList.Category.Science;
            case 4:
                return InterviewList.Category.Society;
            default:
                Debug.Log("Error Deteched In Ending Scene");
                return InterviewList.Category.Economy;
        }

    }
    
    public int getScore(InterviewList.Category category)
    {
        int cScore = 0;
        foreach (int i in score[category])
        {
            cScore += score[category][i];
        }
        return cScore;
    }
    public void setCat(bool cat)
    {
        this.cat = cat;
    }
    public void setEndingCat()
    {
        if(cat)
        {
            EndingController.isCat();
        }
    }
}
