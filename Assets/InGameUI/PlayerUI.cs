using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEditor;

public class PlayerUI : MonoBehaviour
{
    public GameObject GameOver; //게임오버 창
    public GameObject Option; //옵션 창
    public Slider BGMSlider; //BGM 줄이는 용도 
    public Slider EffSlider; //이펙트 줄이는 용도 
    public AudioSource BGMSource; //Map에 넣어놓을 audiosource
    public AudioSource EffSource; //Player에 들어갈 audiosource 
    public TMP_Text Level; //InGame Option창에 들어갈 레벨 표기 
    [SerializeField] GameManager Gm;
    public InputActionProperty showBtn;


    void Start()
    {
        Gm = gameObject.GetComponent<GameManager>();
    }


    void Update()
    {
        Level.text = $" LEVEL : {Gm.PlayerLevel}"; //레벨 표시 
        if (Gm.IsOver == true)  //게임오버 시에는 게임오버창이 열림 
        {
            GameOver.SetActive(true);
        }
        if (showBtn.action.WasPressedThisFrame()) //키를 입력 시에 옵션이 나옴 
        {
            Option.SetActive(!Option.activeSelf);
        }
    }



    public void Lobby() //로비로 돌아가는 기능 버튼(옵션 전용)
    {
        if (GameOver != null && Option != null) 
        SceneManager.LoadScene("Lobby");
    }
    public void Quit() //게임을 완전 종료하기 위한 버튼(옵션 전용)
    {
        if (GameOver != null && Option != null)
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
        slider.value = 1f;
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
        AdjustVolume(EffSource, EffSlider);
    }
}
