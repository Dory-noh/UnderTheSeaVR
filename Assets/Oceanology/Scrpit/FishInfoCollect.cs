using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FishInfoCollect : MonoBehaviour
{
    public FishData fishData;  // ScriptableObject에서 물고기 정보 참조
    private bool isRegistered = false;  // 중복 등록 방지

    private void OnEnable()
    {
        var Interactable = GetComponent<XRSimpleInteractable>();
        if (Interactable != null)
        {
            Interactable.hoverEntered.AddListener(RegisterFishInfo);
        }
    }

    private void OnDisable()
    {
        var Interactable = GetComponent<XRSimpleInteractable>();
        if (Interactable != null)
        {
            Interactable.hoverEntered.RemoveListener(RegisterFishInfo);
        }
    }

    public void RegisterFishInfo(HoverEnterEventArgs Args)
    {
        if (!isRegistered)
        {
            OceanManager.Instance.AddFish(fishData);
            isRegistered = true;
        }
    }
}
