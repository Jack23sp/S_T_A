﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticSlot : MonoBehaviour
{
    public Player player;

    public TextMeshProUGUI statName;
    public TextMeshProUGUI statAmount;
    public Button statButton;
    public Image image;
    public Button button;

    public bool armor;
    public bool health;
    public bool damage;
    public bool adrenaline;
    public bool defense;
    public bool accuracy;
    public bool miss;
    public bool critPerc;

    public bool weight;

    public bool poisoned;
    public bool hungry;
    public bool thirsty;
    public bool blood;
    public bool marriage;
    public bool activeBoost;
    public bool healthBonusPerc;
    public bool manaBonusPerc;
    public bool defenseBonusPerc;


    public void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if(armor)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Armatura" : "Armor";
            statAmount.text = player.playerArmor.currentArmor + " / " + player.playerArmor.maxArmor;
            image.sprite = GeneralManager.singleton.armor;
            button.interactable = false;
        }
        if (health)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Vita" : "Health";
            statAmount.text = player.health + " / " + player.healthMax;
            image.sprite = GeneralManager.singleton.health;
            button.interactable = false;
        }
        if (adrenaline)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Adrenalina" : "Adrenaline";
            statAmount.text = player.mana + " / " + player.manaMax;
            image.sprite = GeneralManager.singleton.adrenaline;
            button.interactable = false;
        }
        if (damage)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Forza" : "Strenght";
            statAmount.text = player.damage.ToString();
            image.sprite = GeneralManager.singleton.damage;
            button.interactable = false;
        }
        if (defense)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Difesa" : "Defense";
            statAmount.text = player.defense.ToString();
            image.sprite = GeneralManager.singleton.defense;
            button.interactable = false;
        }
        if (accuracy)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Precisione" : "Accuracy";
            statAmount.text = player.playerAccuracy.accuracy + " / " + "100";
            image.sprite = GeneralManager.singleton.accuracy;
            button.interactable = false;
        }
        if (miss)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Evasione" : "Evasion";
            statAmount.text = player.playerMiss.maxMiss + " / " + "100";
            image.sprite = GeneralManager.singleton.miss;
            button.interactable = false;
        }
        if (critPerc)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Crit" : "Crit";
            statAmount.text = (player.criticalChance * 100) + " / " + "100";
            image.sprite = GeneralManager.singleton.critPerc;
            button.interactable = false;
        }
        if (weight)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Peso" : "Weight";
            statAmount.text = player.playerWeight.currentWeight + " / " + player.playerWeight.maxWeight;
            image.sprite = GeneralManager.singleton.weight;
            button.interactable = false;
        }
        if (poisoned)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Avvelenamento" : "Poisoned";
            statAmount.text = player.playerPoisoning.currentPoisoning + " / " + player.playerPoisoning.maxPoisoning;
            image.sprite = GeneralManager.singleton.poisoned;
            button.interactable = false;
        }
        if (hungry)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Fame" : "Hungry";
            statAmount.text = player.playerHungry.currentHungry + " / " + player.playerHungry.maxHungry;
            image.sprite = GeneralManager.singleton.hungry;
            button.interactable = false;
        }
        if (thirsty)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Sete" : "Thirsty";
            statAmount.text = player.playerThirsty.currentThirsty + " / " + player.playerThirsty.maxThirsty;
            image.sprite = GeneralManager.singleton.thirsty;
            button.interactable = false;
        }
        if (blood)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Sangue" : "Blood";
            statAmount.text = player.playerBlood.currentBlood + " / " + "100";
            image.sprite = GeneralManager.singleton.blood;
            button.interactable = false;
        }
        if (marriage)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Compagno" : "Partner";
            statAmount.text = player.playerMarriage.partnerName;
            image.sprite = GeneralManager.singleton.marriage;
            button.interactable = player.playerMarriage.partnerName != string.Empty;
            button.onClick.SetListener(() =>
            {
                Instantiate(GeneralManager.singleton.marriageRemovePanel, GeneralManager.singleton.canvas);
            });
        }
        if (activeBoost)
        {
            statName.text = "Boosts";
            statAmount.text = player.playerBoost.networkBoost.Count.ToString();
            image.sprite = GeneralManager.singleton.activeBoost;
            button.interactable = player.playerBoost.networkBoost.Count > 0;
            button.onClick.SetListener(() =>
            {

            });
        }
        if (defenseBonusPerc)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Bonus difesa partner" : "Partner defense bonus";
            statAmount.text = GeneralManager.singleton.marriageDefense + " % ";
            image.sprite = GeneralManager.singleton.defenseBonus;
            button.interactable = false;
        }
        if (manaBonusPerc)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Bonus adrealina partner" : "Partner Adrenaline bonus";
            statAmount.text = GeneralManager.singleton.marriageMana + " % ";
            image.sprite = GeneralManager.singleton.manaBonus;
            button.interactable = false;
        }
        if (healthBonusPerc)
        {
            statName.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Bonus vita partner" : "Partner health bonus";
            statAmount.text = GeneralManager.singleton.marriageHealth + " % ";
            image.sprite = GeneralManager.singleton.healthBonus;
            button.interactable = false;
        }
    }
}