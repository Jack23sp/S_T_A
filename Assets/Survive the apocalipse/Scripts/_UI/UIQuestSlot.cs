// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestSlot : MonoBehaviour
{
    public Button nameButton;
    public GameObject childObject;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI goldText;
    public Transform itemContent;    
    public Transform itemContentDetails;    
    public UIInventorySlot toSpawn;
    public Button claimButton;

}
