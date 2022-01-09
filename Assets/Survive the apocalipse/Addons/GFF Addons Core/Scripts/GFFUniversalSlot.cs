using UnityEngine;
using UnityEngine.UI;

public partial class GFFUniversalSlot : MonoBehaviour {

    public UIShowToolTip tooltip;
    public Button button;
    public UIDragAndDropable dragAndDropable;
    public Image image;
    public Image cooldownCircle;
    public GameObject amountOverlay;
    public Text amountText;

    [Header("only for Equipment")]
    public GameObject categoryOverlay;
    public Text categoryText;

    [Header("only for Skillbar")]
    public Text hotkeyText;

    [Header("Rarity addon")]
    public Image rarityImage;

    [Header("only for skills")]
    public GameObject cooldownOverlay;
    public Text cooldownText;

    [Header("only for upgrade")]
    public Text upgradeText;

    [Header("only for loot")]
    public Text nameText;

    [Header("only for macro")]
    public float CooldownTime;
    public float CooldownRemainingTime;

    [Header("only for quest")]
    public Image border;
}
