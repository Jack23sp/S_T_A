// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILootSlot : MonoBehaviour
{
    public Button button;
    public Image backgroundImage;
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public GameObject amountOverlay;
    public TextMeshProUGUI amountText;

    public void Start()
    {
        backgroundImage.preserveAspect = true;
        itemImage.preserveAspect = true;
    }
}
