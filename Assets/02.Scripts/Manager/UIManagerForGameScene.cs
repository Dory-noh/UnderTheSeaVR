using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class UIManagerForGameScene : MonoBehaviour
{
    private static UIManagerForGameScene instance;
    public static UIManagerForGameScene Instance
    {
        get{
            if (instance == null) instance = FindObjectOfType<UIManagerForGameScene>();
            return instance;
        }
    }
    [SerializeField] GameObject WarningImg;

    private readonly WaitForSeconds blinkTime = new WaitForSeconds(1f);
    [SerializeField] private int blinkCount = 3;
    public GameObject GameOverUI; //게임오버 창
    public GameObject OptionUI; //옵션 창
    public Slider BGMSlider; //BGM 줄이는 용도 
    public Slider EffSlider; //이펙트 줄이는 용도 
    public AudioSource BGMSource; //Map에 넣어놓을 audiosource
    public AudioSource[] EffSource; //Player에 들어갈 audiosource 
    public TMP_Text Level; //InGame Option창에 들어갈 레벨 표기
    public InputActionProperty showBtn;
    public TMP_Dropdown motionDropdown;
    public UnityEvent dangerAlertSFX;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this) Destroy(gameObject);
    }
    public void Start()
    {
        WarningImg.SetActive(false);
        GameOverUI.SetActive(false);
        OptionUI.SetActive(false);
        BGMSlider.value = 3;
        EffSlider.value = 3;
        AdjustBGM();
        AdjustEff();
        motionDropdown.value = GameManager.Instance.MoveMode;
    }
    public IEnumerator BlinkRedImg()
    {
        //Debug.Log("위험해요");
        for(int i = 0; i < blinkCount; i++)
        {
            WarningImg.SetActive(true);
            yield return blinkTime;
            WarningImg.SetActive(false);
            yield return blinkTime;
        }
    }

    private void Update()
    {
        Level.text = $" LEVEL : {GameManager.Instance.PlayerLevel}"; //레벨 표시 
        if (GameManager.Instance.IsOver == true)  //게임오버 시에는 게임오버창이 열림 
        {
            GameOverUI.SetActive(true);
            OptionUI.SetActive(false);
        }
        if (showBtn.action.WasPressedThisFrame()) //키를 입력 시에 옵션이 나옴 
        {
            OptionUI.SetActive(!OptionUI.activeSelf);
        }
    }

    public void MotionChange(int index)
    {
        GameManager.Instance.MoveMode = index;
    }

    public void Lobby() //로비로 돌아가는 기능 버튼(옵션 전용)
    {
        Debug.Log("로비로 돌아가기");
        if (GameOverUI != null && OptionUI != null)
        {
            SceneManager.LoadScene("Lobby");
            GameManager.Instance.IsOver = false;
            GameManager.Instance.PlayerLevel = 1;
        }

    }

    public void Quit() //게임을 완전 종료하기 위한 버튼(옵션 전용)
    {
        Debug.Log("종료 버튼 누름");
        if (GameOverUI != null && OptionUI != null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

#else
       Application.Quit();
#endif
        }
    }
    void AdjustVolume(AudioSource source, Slider slider) //볼륨조절 
    {
        if (source != null && slider != null)
        {
            source.volume = slider.value;
        }

    }
    public void AdjustBGM() //BGM 조절 
    {
        AdjustVolume(BGMSource, BGMSlider);
    }

    public void AdjustEff() //Effect 조절
    {
        foreach(var source in EffSource) AdjustVolume(source, EffSlider);
    }
}
