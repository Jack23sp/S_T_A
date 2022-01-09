using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuctionHouseSlot : MonoBehaviour
{
    public Sprite defaultImage;

    public Image rarityImage;
    public Image itemImage;

    public Text itemName;
    public Text itemAmount;

    public Button UpgradeButton;
    
    public Text bidText;
    public Text bidoutText;
    public Text sellerText;
    public Text lastBidder;
    public Text remainingTime;

    public InputField inputField;

    public Button bidObject;
    public Button bidoutObject;
}
