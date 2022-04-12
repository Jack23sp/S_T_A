// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemMallSlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public Image image;
    public UIDragAndDropable dragAndDropable;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;
    public Button unlockButton;

    public void Start()
    {
        image.preserveAspect = true;
    }
}
