using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool isOver;
    
    public bool IsOver
    {
        get { return isOver; }
        set { isOver = value; }
    }

    [SerializeField] private int moveMode;

    public int MoveMode
    {
        get { return moveMode; }
        set { moveMode = value; }
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

    [SerializeField] private int playerLevel;

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
        else if(instance != this) Destroy(gameObject);
    }

    void Start()
    {
        playerLevel = 1;
        exp = 0;
    }

    public void PlusExp(int point) //플레이어보다 레벨이 낮은 물고기와 충돌시 1점 획득
    {
        Debug.Log($"하위 레벨 공격 성공 : {point}점 획득");
        exp+=point;
        SetLevel();
    }

    public void MinusExp(int point) //플레이어와 레벨이 같은 물고기와 충돌시 1점 차감
    {
        Debug.Log($"동일 레벨 물고기와 충돌 : {point}점 감점");
        exp-=point;
        SetLevel();
    }

    public void GameOver()
    {
        Debug.Log("게임 종료");
        isOver = true;
    }

    public void SetLevel()
    {
        if (exp / (10 * playerLevel) == 1)
        {
            Debug.Log($"플레이어 레벨 : {playerLevel.ToString()}");
            FindObjectOfType<PlayerMove>().acceleration -= GameManager.Instance.PlayerLevel * 0.5f;
            playerLevel++;
        }
        

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
