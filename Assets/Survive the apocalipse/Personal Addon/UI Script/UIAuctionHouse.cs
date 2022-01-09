using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIAuctionHouse : MonoBehaviour
{
    public Text categoryText;

    public GameObject bidClaimPanel;
    public Transform actualClaimBidContent;

    public GameObject bidderPanel;

    public GameObject auctionHouseSlot;
    public GameObject auctionHouseBiddedSlot;

    public Button allObject;
    public Button claimObject;
    public Button bidderObject;

    public bool allObjectB;
    public bool claimObjectB;
    public bool bidderObjectB;


    public Player player;

    public AuctionHouse auctionHouse;

    // Start is called before the first frame update
    void Start()
    {
        categoryText.text = "Categories :";
        bidClaimPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (auctionHouse) auctionHouse = Player.localPlayer.target.GetComponent<AuctionHouse>();

        allObject.onClick.SetListener(() =>
        {
            bidClaimPanel.SetActive(true);
            bidderPanel.SetActive(false);
            allObjectB = true;
            claimObjectB = false;
            bidderObjectB = false;
        });

        claimObject.onClick.SetListener(() =>
        {
            bidClaimPanel.SetActive(true);
            bidderPanel.SetActive(false);
            allObjectB = false;
            claimObjectB = true;
            bidderObjectB = false;
        });

        bidderObject.onClick.SetListener(() =>
        {
            bidClaimPanel.SetActive(false);
            bidderPanel.SetActive(true);
            allObjectB = false;
            claimObjectB = false;
            bidderObjectB = true;
        });

        if(allObjectB)
        {
            UIUtils.BalancePrefabs(auctionHouseSlot, auctionHouse.actualInBid.Count, actualClaimBidContent); 
            for(int i = 0; i < auctionHouse.actualInBid.Count; i++)
            {
                int index = i;
                AuctionHouseSlot actual = actualClaimBidContent.transform.GetChild(0).GetComponent<AuctionHouseSlot>();
                actual.rarityImage.color = GffItemRarity.singleton.rarityColor(true, auctionHouse.actualInBid[index].itemSlot.item);
                actual.itemImage.sprite = auctionHouse.actualInBid[index].itemSlot.item.image;
                actual.itemName.text = auctionHouse.actualInBid[index].itemSlot.item.name;
                actual.itemAmount.text = "Amount : " + auctionHouse.actualInBid[index].itemSlot.amount;
                actual.UpgradeButton.gameObject.SetActive(auctionHouse.actualInBid[index].upgrade);
                actual.bidText.text = auctionHouse.actualInBid[index].userThatBidThisObject.Length > 0 ? "Actual : \n" + auctionHouse.actualInBid[index].userThatBidThisObject.Last().userToReturn : "Actual \n: " + string.Empty;
                actual.bidoutText.text = "Bidout : " + auctionHouse.actualInBid[index].buyNowBid;
                actual.sellerText.text = "Seller : \n" + auctionHouse.actualInBid[index].sellerName;
                actual.lastBidder.text = auctionHouse.actualInBid[index].userThatBidThisObject.Length > 0 ? "Bidder : \n" + auctionHouse.actualInBid[index].userThatBidThisObject.Last().userToReturn : "Bidder \n: " + string.Empty;
                actual.remainingTime.text = GeneralManager.singleton.ConvertToTimer(auctionHouse.actualInBid[index].remainingEstimateTime);

                actual.bidObject.onClick.SetListener(() =>
                {
                    player.CmdBidActualItem(index, Convert.ToInt32(actual.inputField.text));
                });
                actual.bidoutObject.onClick.SetListener(() =>
                {
                    player.CmdBidoutItem(index);
                });
            }
        }

        if(claimObjectB)
        {
            UIUtils.BalancePrefabs(auctionHouseSlot, auctionHouse.FinshedBid.Count, actualClaimBidContent);
            for (int i = 0; i < auctionHouse.FinshedBid.Count; i++)
            {
                int index = i;
                ClaimAuctionHouseSlot claim = actualClaimBidContent.transform.GetChild(0).GetComponent<ClaimAuctionHouseSlot>();
                claim.rarityImage.color = GffItemRarity.singleton.rarityColor(true, auctionHouse.FinshedBid[index].itemSlot.item);
                claim.itemImage.sprite = auctionHouse.FinshedBid[index].itemSlot.item.image;
                claim.itemName.text = auctionHouse.FinshedBid[index].itemSlot.item.name;
                claim.itemAmount.text = "Amount : " + auctionHouse.FinshedBid[index].itemSlot.amount.ToString();
                claim.bidText.text = auctionHouse.FinshedBid[index].userThatBidThisObject.Length > 0 ? "Bidder : \n" + auctionHouse.FinshedBid[index].userThatBidThisObject.Last().userToReturn : "Bidder \n: " + string.Empty;
                claim.UpgradeButton.gameObject.SetActive(auctionHouse.FinshedBid[index].upgrade);
                claim.ClaimButton.onClick.SetListener(() =>
                {

                });
            }
        }

        if(bidderObjectB)
        {

        }
    }
}
