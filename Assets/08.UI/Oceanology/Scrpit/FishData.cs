using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Oceanology/FishData")]
public class FishData : ScriptableObject
{
    public string fishName;  // 물고기 이름
    [TextArea] public string description;  // 물고기 설명
    //public Sprite fishImage;  // 물고기 이미지 (UI에 표시할 용도)
}

