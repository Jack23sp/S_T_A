using EasyMobile;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;



public class IAPManager : MonoBehaviour
{
    public static IAPManager singleton;

    public IAPProduct[] products;
    public List<IAPProduct> goldProducts = new List<IAPProduct>();
    public List<IAPProduct> gemsProducts = new List<IAPProduct>();

    public CustomType.Product[] itemImage;

    public void Awake()
    {
        if (!RuntimeManager.IsInitialized())
            RuntimeManager.Init();
    }

    public void Start()
    {
        if (!singleton) singleton = this;

        if (!InAppPurchasing.IsInitialized())
        {
            InAppPurchasing.InitializePurchasing();
        }

        products = InAppPurchasing.GetAllIAPProducts();
        //	Print	all	product	names 
        foreach (IAPProduct prod in products)
        {
            if (prod.Name.Contains("gold"))
                if (!goldProducts.Contains(prod)) goldProducts.Add(prod);
            if (prod.Name.Contains("gems"))
                if (!gemsProducts.Contains(prod)) gemsProducts.Add(prod);

        }

    }

    void OnEnable() { InAppPurchasing.PurchaseCompleted += PurchaseCompletedHandler; InAppPurchasing.PurchaseFailed += PurchaseFailedHandler; }

    void OnDisable() { InAppPurchasing.PurchaseCompleted -= PurchaseCompletedHandler; InAppPurchasing.PurchaseFailed -= PurchaseFailedHandler; }

    public void PurchaseFailedHandler(IAPProduct product , string itemName)
    {

    }

        public void Purchase(string itemName)
    {
        switch (itemName)
        {
            case "gold_500":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_500_gold);
                break;
            case "gold_1000":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_1000_gold);
                break;
            case "gold_2000":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_2000_gold);
                break;
            case "gold_5000":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_5000_gold);
                break;
            case "gold_100000":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_10000_gold);
                break;
            case "gems_50":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_50_gems);
                break;
            case "gems_100":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_100_gems);
                break;
            case "gems_250":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_250_gems);
                break;
            case "gems_500":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_500_gems);
                break;
            case "gems_1000":
                InAppPurchasing.Purchase(EM_IAPConstants.Product_1000_gems);
                break;
        }
    }

    void PurchaseCompletedHandler(IAPProduct product)
    {
        switch (product.Name)
        {
            case EM_IAPConstants.Product_500_gold:
                Player.localPlayer.CmdAddGold(500);
                GoldThanks();
                break;
            case EM_IAPConstants.Product_1000_gold:
                Player.localPlayer.CmdAddGold(1000);
                GoldThanks();
                break;
            case EM_IAPConstants.Product_2000_gold:
                Player.localPlayer.CmdAddGold(2000);
                GoldThanks();
                break;
            case EM_IAPConstants.Product_5000_gold:
                Player.localPlayer.CmdAddGold(5000);
                GoldThanks();
                break;
            case EM_IAPConstants.Product_10000_gold:
                Player.localPlayer.CmdAddGold(10000);
                GoldThanks();
                break;
            case EM_IAPConstants.Product_50_gems:
                Player.localPlayer.CmdAddCoins(50);
                CoinThanks();
                break;
            case EM_IAPConstants.Product_100_gems:
                Player.localPlayer.CmdAddCoins(100);
                CoinThanks();
                break;
            case EM_IAPConstants.Product_250_gems:
                Player.localPlayer.CmdAddCoins(250);
                CoinThanks();
                break;
            case EM_IAPConstants.Product_500_gems:
                Player.localPlayer.CmdAddCoins(500);
                CoinThanks();
                break;
            case EM_IAPConstants.Product_1000_gems:
                Player.localPlayer.CmdAddCoins(1000);
                CoinThanks();
                break;
        }
    }

    public void GoldThanks()
    {
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            ScreenNotificationManager.singleton.SpawnNotification("Grazie per l'acquisto!", "Gold");
        else
            ScreenNotificationManager.singleton.SpawnNotification("Thanks for the purchase!", "Gold");
    }

    public void CoinThanks()
    {
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            ScreenNotificationManager.singleton.SpawnNotification("Grazie per l'acquisto!", "Coin");
        else
            ScreenNotificationManager.singleton.SpawnNotification("Thanks for the purchase!", "Coin");
    }

    public Sprite GetPremiumItemImage(string itemName)
    {
        for(int i = 0; i < itemImage.Length; i++)
        {
            if(itemImage[i].name == itemName)
            {
                return itemImage[i].itemImage;
            }
        }

        Debug.LogError("You probably missed to assign sprite for item whit name : " + itemName);
        return null;
    }

    //	Failed	purchase	handler 
    //void PurchaseFailedHandler(IAPProduct product) { Debug.Log("The	purchase	of	product	" + product.Name + "	has	failed."); }
}