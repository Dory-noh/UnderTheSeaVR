using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour 
{ 

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.IsOver == true) return;
        int playerLevel = GameManager.Instance.PlayerLevel;
        if (other.CompareTag("Fish"))
        {
            int level = other.GetComponent<Fish>().Level;
            if (playerLevel > level) //플레이어의 레벨이 부딪힌 물고기의 레벨보다 높을 때
            {
                GameManager.Instance.PlusExp();
            }
            else if (playerLevel == level) //플레이어의 레벨과 부딪힌 물고기의 레벨이 같을 때
            {
                GameManager.Instance.MinusExp();
            }
            else //플레이어의 레벨이 부딪힌 물고기의 레벨보다 작을 때
            {
                GameManager.Instance.GameOver();
            }
        }
    }

}
