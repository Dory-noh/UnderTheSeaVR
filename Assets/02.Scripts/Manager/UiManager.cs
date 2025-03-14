using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UiManager : MonoBehaviour
{
    public GameObject Player;
    public GameObject option;
    public GameObject title;
    public GameObject Oceanology;
    public bool Onoff = false;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown motionDropdown;
    public Slider brightnessSlider;
    public Slider volumeSlider;
    //public LocomotionProvider locoPr;
    //public ControllerMove conMov;
    //public PostProcessingData globalVolume;
    public AudioSource audioSource;
    public OceanManager Oceanmanager;
    void Start()
    {
        Oceanology = OceanManager.Instance.OceanUI;
        Oceanology.SetActive(false);
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        //brightnessSlider.onValueChanged.AddListener();
        volumeSlider.onValueChanged.AddListener(AdjustVolume);
        AdjustVolume();
        motionDropdown.value = GameManager.Instance.MoveMode;
    }
    private void OnEnable()
    {
        Oceanmanager = GameObject.Find("OceanManager").GetComponent<OceanManager>();
    }

    public void GameScene() 
    {
        SceneManager.LoadScene("GameScene");
    }
    public void OceanologyScene(bool Turn)
    {

        title.SetActive(!Turn);
        Oceanmanager.OceanUI.SetActive(Turn);
        option.SetActive(false);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
       Application.Quit();
#endif
    }
    public void Option() 
    {
        Onoff = !Onoff;
        option.SetActive(Onoff);
    }
    public void AdjustVolume(float value = 5f)
    {
        if (audioSource != null)
        {
            audioSource.volume = value;
            volumeSlider.value = value;
        }
    }
    public void ChangeResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                break;
            case 1:
                Screen.SetResolution(1600, 900, FullScreenMode.FullScreenWindow);
                break;
            case 2:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;
        }
    }
    public void MotionChange(int index)
    {
        GameManager.Instance.MoveMode = index;
        //switch (index)
        //{
        //    case 0:
                
        //        //locoPr.enabled = false;
        //        //conMov.enabled = true;
        //        break;
        //    case 1:
        //        //locoPr.enabled = true;
        //        //conMov.enabled = false;
        //        break;

        //}
    }
}

