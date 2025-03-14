using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour 
{
    public UnityEvent playerEatFishSFX;
    public UnityEvent fishEatPlayerSFX;


    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.IsOver == true) return;
        int playerLevel = GameManager.Instance.PlayerLevel;

        if (other.CompareTag("Fish"))
        {
            if (other.GetComponent<Fish>().IsBumped == true) return;
            other.GetComponent<Fish>().IsBumped = true;
            Debug.Log($"충돌한 물고기 이름 : {other.gameObject.name}");
            int level = other.GetComponent<Fish>().Level;
            if (playerLevel > level) //플레이어의 레벨이 부딪힌 물고기의 레벨보다 높을 때
            {
                playerEatFishSFX?.Invoke();
                //경험치 1 증가
                GameManager.Instance.PlusExp(level+1);
                other.gameObject.SetActive(false);
                StartCoroutine(PoolingManager.Instance.FishRespawn(other.GetComponent<Fish>().Area, other.GetComponent<Fish>().Level));
            }
            else if (playerLevel == level) //플레이어의 레벨과 부딪힌 물고기의 레벨이 같을 때
            {
                //경험치 1 감소
                GameManager.Instance.MinusExp(level+1);
            }
            else //플레이어의 레벨이 부딪힌 물고기의 레벨보다 작을 때
            {
                fishEatPlayerSFX?.Invoke();
                //게임 오버
                GameManager.Instance.GameOver();
            }
        }
    }
}
