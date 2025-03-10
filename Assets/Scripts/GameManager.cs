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

    public void PlusExp() //�÷��̾�� ������ ���� ������ �浹�� 1�� ȹ��
    {
        Debug.Log("���� ���� ���� ���� : 1�� ȹ��");
        exp++;
        SetLevel();
    }

    public void MinusExp() //�÷��̾�� ������ ���� ������ �浹�� 1�� ����
    {
        Debug.Log("���� ���� ������ �浹 : 1�� ����");
        exp--;
        SetLevel();
    }

    public void GameOver()
    {
        Debug.Log("���� ����");
        isOver = true;
    }

    public void SetLevel()
    {
        if (exp / 10 == playerLevel) playerLevel++;
        Debug.Log($"�÷��̾� ���� : {playerLevel.ToString()}");

        if(playerLevel > 5)
        {
            Debug.Log($"���� Ŭ���� : ������ �����մϴ�.");
            GameOver();
        }
    }

    void Update()
    {
        if (GameManager.Instance.IsOver == true) return;
    }
}
