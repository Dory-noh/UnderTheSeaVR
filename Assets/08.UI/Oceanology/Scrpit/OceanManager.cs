using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OceanManager : MonoBehaviour
{
    public static OceanManager Instance;

    public GameObject fishPrefab;  // 해양사전 UI에 추가할 프리팹 (TMP)
    public Transform fishContainer;  // Grid Layout Group이 있는 UI 부모
    [SerializeField] private List<FishData> collectedFishes = new List<FishData>();  // 등록된 물고기 리스트
    public GameObject OceanUI;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        fishContainer = OceanUI.transform.GetChild(1).GetChild(0).GetChild(0);
    }
    public void AddFish(FishData newFish)
    {
        if (!collectedFishes.Contains(newFish))
        {
            collectedFishes.Add(newFish);
            CreateFishUI(newFish);
            Debug.Log($"도감에 {newFish.name}이 추가되었습니다.");
        }
    }

    public void OceanologyScene(bool Turn)
    {
        Turn = !Turn;
        UiManager uimanager = GameObject.Find("UIManager").GetComponent<UiManager>();
        if (uimanager == null) Debug.Log("못찾음");
        else Debug.Log("매니저 찾음");
        uimanager.title.SetActive(!Turn);
        uimanager.Oceanology.SetActive(Turn);
        uimanager.option.SetActive(false);
    }

    private void CreateFishUI(FishData fish)
    {
        GameObject newFishUI = Instantiate(fishPrefab, fishContainer);

        TMP_Text nameText = newFishUI.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text descriptionText = newFishUI.transform.Find("Description").GetComponent<TMP_Text>();
        //Image fishImage = newFishUI.transform.Find("Image").GetComponent<Image>();

        if (nameText != null) nameText.text = fish.fishName;
        if (descriptionText != null) descriptionText.text = fish.description;
        //if (fishImage != null) fishImage.sprite = fish.fishImage;
    }
}
