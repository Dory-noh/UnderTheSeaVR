using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    int totalLevel = 5;
    private static PoolingManager instance;
    public static PoolingManager Instance
    {
        get { if (instance == null) FindObjectOfType<PoolingManager>(); return instance; }
    }
    [SerializeField] private GameObject[] fishPrefabs;
    [SerializeField] private List<List<GameObject>> fishList = new List<List<GameObject>> { }; 
    [SerializeField] Transform[] area = new Transform[3]; //layer별 위치 받아옴
    [SerializeField] int[] areaCounts = new int[3] { 15, 5, 3 }; //area 크기별 물고기 생성 마릿수
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
        for (int i = 0; i < totalLevel; i++)
        {
            List<GameObject> fishes = new List<GameObject>();
            for (int j = 0; j < 25; j++)
            {
                GameObject Fish = Instantiate(fishPrefabs[Random.Range(i * 3, i * 3 + 3)], fishGroup.transform);
                Fish.name = $"물고기 {i + 1}호 - {j + 1}번째";
                Fish.GetComponent<Fish>().Level = i; //물고기 레벨 설정
                Fish.SetActive(false);
                fishes.Add(Fish);
                if (i == totalLevel - 1 && j == areaCounts[areaCounts.Length - 1] - 1) break;
            }
            fishList.Add(fishes);
        }
    }

    //물고기 배치 메서드
    IEnumerator SetFish() 
    {
        //포문을 두 번 돌려서 각 위치마다 10 5 5 배치를 세번 진행하게,
        //그리고 안에 들어가는 포문에서는 3종류의 물고기가 15 5 3 로 SetActive되게 
        for (int i = 0; i < area.Length; i++) //각 위치마다 15 5 3로 배치 
        {
            for (int j = 0; j < areaCounts.Length; j++) //물고기를 15 5 3로 배치 
            {
                for(int k = 0; k < areaCounts[j]/(j == 0 ? i+1 : 1); k++) //층별 수 조절
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
                fish.GetComponent<Fish>().Area = i;
                Transform RespawnPoint = area[i].transform;
                fish.transform.position = new Vector3(Random.Range(RespawnPoint.position.x - 70, RespawnPoint.position.x + 70), Random.Range(RespawnPoint.position.y - 30, RespawnPoint.position.y + 30), Random.Range(RespawnPoint.position.z - 70, RespawnPoint.position.z + 70));
                fish.transform.rotation = Quaternion.identity;
                fish.gameObject.SetActive(true);
                break;
            }
        }
    }

    public IEnumerator FishRespawn(int AreaNum, int level)
    {
        //if (GameManager.Instance.IsOver == true) yield return null;
        yield return new WaitForSeconds(1f);
        Debug.Log($"{AreaNum}위치에 {level}레벨 물고기 리스폰 되었습니다.");
        foreach (var fish in fishList[level])
        {
            //if (GameManager.instance.isGameover) break;
            if (fish.activeSelf == false)
            {
                Transform RespawnPoint = area[AreaNum].transform;
                fish.transform.position = new Vector3(Random.Range(RespawnPoint.position.x - 30, RespawnPoint.position.x + 30), Random.Range(RespawnPoint.position.y - 30, RespawnPoint.position.y + 30), Random.Range(RespawnPoint.position.z - 30, RespawnPoint.position.z + 30));
                fish.transform.rotation = Quaternion.identity;
                fish.gameObject.SetActive(true);
                break;
            }
        }
    }
}
