using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    public IEnumerator BlinkRedImg()
    {
        Debug.Log("위험해요");
        for(int i = 0; i < blinkCount; i++)
        {
            WarningImg.SetActive(true);
            yield return blinkTime;
            WarningImg.SetActive(false);
            yield return blinkTime;
        }
    }
}
