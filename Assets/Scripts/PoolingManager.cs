using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager instance;
    [SerializeField] private GameObject[] fishPrefabs = new GameObject[5];
    [SerializeField] private List<List<GameObject>> fishList = new List<List<GameObject>> { }; 
    [SerializeField] Transform[] area = new Transform[3]; //layer별 위치 받아옴
    [SerializeField] int[] areaCounts = new int[3] { 10, 5, 5 }; //area 크기별 물고기 생성 마릿수
    WaitForSeconds respawnTime;
    WaitForSeconds ws;
    void Awake()
    {       
        ws = new WaitForSeconds(1f);
        respawnTime = new WaitForSeconds(5f);
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(gameObject); }
        CreateFish();
        StartCoroutine(SetFish());

    }
    void CreateFish() //초기 물고기 생성 메서드
    {
        var fishGroup = new GameObject("Fishes");
        for (int i = 0; i < fishPrefabs.Length; i++)
        {
            List<GameObject> fishes = new List<GameObject>();
            for (int j = 0; j < 20; j++)
            {
                GameObject Fish = Instantiate(fishPrefabs[i], fishGroup.transform);
                Fish.name = $"물고기 {i + 1}호 - {j + 1}번째";
                Fish.GetComponent<Fish>().Level = i; //물고기 레벨 설정
                Fish.SetActive(false);
                fishes.Add(Fish);
            }
            fishList.Add(fishes);
        }
    }

    //물고기 배치 메서드
    IEnumerator SetFish() 
    {
        //포문을 두 번 돌려서 각 위치마다 10 5 5 배치를 세번 진행하게,
        //그리고 안에 들어가는 포문에서는 3종류의 물고기가 10 5 5 로 SetActive되게 
        for (int i = 0; i < area.Length; i++) //각 위치마다 10 5 5로 배치 
        {
            for (int j = 0; j < areaCounts.Length; j++) //물고기를 10 5 5로 배치 
            {
                for(int k = 0; k < areaCounts[j]; k++)
                {
                    FishSpawn(i, j);
                }            
            }
        }
        yield return null;
    }

    void FishSpawn(int i, int j)
    {
        if (GameManager.Instance.IsOver == true) return;
        foreach (var fish in fishList[i+j])
        {
            //if (GameManager.instance.isGameover) break;
            if (fish.activeSelf == false)
            {
                fish.transform.position = area[i].transform.position;
                fish.transform.rotation = area[i].transform.rotation;
                fish.gameObject.SetActive(true);
                break;
            }
        }
    }

    void Update()
    {
        
    }
}
