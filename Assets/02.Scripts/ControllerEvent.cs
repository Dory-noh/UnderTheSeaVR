using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerEvent : MonoBehaviour
{
    public InputActionProperty leftHandTriggerAction;
    public InputActionProperty rightHandTriggerAction;
    public UnityEvent ToggleRay;
    public UnityEvent ToggleRayOff;
    [SerializeField] private GameObject colorBall;

    private void Start()
    {
        ToggleRayOff.Invoke();
    }

    //fish를 ray로 hover할 때 작동하는 메서드
    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        GameObject fish = args.interactableObject.transform.gameObject;
        if(fish != null)
        {
            Debug.Log($"물고기 감지 : {fish.name}");
            int level = fish.GetComponent<Fish>().Level;
            int playerLevel = GameManager.Instance.PlayerLevel;
            if (playerLevel > level) //플레이어의 레벨이 Ray에 부딪힌 물고기의 레벨보다 높을 때
            {
                colorBall.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            else if (playerLevel == level) //플레이어의 레벨과 Ray에 부딪힌 물고기의 레벨이 같을 때
            {
                colorBall.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else //플레이어의 레벨이 Ray에 부딪힌 물고기의 레벨보다 작을 때
            {
                colorBall.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            Invoke("resetColorBall", 5f);
        }
        else
        {
            Debug.Log("현재 Ray에 감지된 Fish가 없습니다. ColorBall 초기화");
            resetColorBall();
        }
    }

    private void resetColorBall()
    {
        colorBall.GetComponent<MeshRenderer>().material.color = Color.white;
    }

    private void OnEnable()
    {
        leftHandTriggerAction.action.performed += OnTriggerPressed;
        rightHandTriggerAction.action.performed += OnTriggerPressed;
        leftHandTriggerAction.action.canceled += OnTriggerReleased;
        rightHandTriggerAction.action.canceled += OnTriggerReleased;
    }

    private void OnDisable()
    {
        leftHandTriggerAction.action.performed -= OnTriggerPressed;
        rightHandTriggerAction.action.performed -= OnTriggerPressed;
        leftHandTriggerAction.action.canceled -= OnTriggerReleased;
        rightHandTriggerAction.action.canceled -= OnTriggerReleased;
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {

        ToggleRay.Invoke();
    }

    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        ToggleRayOff.Invoke();
    }
}
