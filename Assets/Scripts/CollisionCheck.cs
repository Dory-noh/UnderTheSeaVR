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
            if (playerLevel > level) //�÷��̾��� ������ �ε��� ������� �������� ���� ��
            {
                GameManager.Instance.PlusExp();
            }
            else if (playerLevel == level) //�÷��̾��� ������ �ε��� ������� ������ ���� ��
            {
                GameManager.Instance.MinusExp();
            }
            else //�÷��̾��� ������ �ε��� ������� �������� ���� ��
            {
                GameManager.Instance.GameOver();
            }
        }
    }

}
