using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool isOver;

    public bool IsOver
    {
        get { return isOver; }
    }

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    private int playerLevel;

    public int PlayerLevel
    {
        get { return playerLevel; }
        set { playerLevel = value; }
    }

    private int exp;

    public int Exp
    {
        get { return exp; }
        set { exp = value; }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        playerLevel = 1;
        exp = 0;
    }

    public void PlusExp() //플레이어보다 레벨이 낮은 물고기와 충돌시 1점 획득
    {
        Debug.Log("하위 레벨 공격 성공 : 1점 획득");
        exp++;
        SetLevel();
    }

    public void MinusExp() //플레이어와 레벨이 같은 물고기와 충돌시 1점 차감
    {
        Debug.Log("동일 레벨 물고기와 충돌 : 1점 감점");
        exp--;
        SetLevel();
    }

    public void GameOver()
    {
        Debug.Log("게임 종료");
        isOver = true;
    }

    public void SetLevel()
    {
        if (exp / 10 == playerLevel) playerLevel++;
        Debug.Log($"플레이어 레벨 : {playerLevel.ToString()}");

        if(playerLevel > 5)
        {
            Debug.Log($"게임 클리어 : 게임을 종료합니다.");
            GameOver();
        }
    }

    void Update()
    {
        if (GameManager.Instance.IsOver == true) return;
    }
}
