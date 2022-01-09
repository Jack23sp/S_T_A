using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuButtonManager : MonoBehaviour
{
    public Button abilitiesButton;
    public GameObject abilityPanel;
    public Button guildButton;
    public GameObject guildPanel;
    public Button boostButton;
    public GameObject boostPanel;
    public Button friendButton;
    public GameObject friendPanel;
    public Button questButton;
    public GameObject questPanel;
    public Button optionsButton;
    public GameObject optionsPanel;

    public UIOrderManager uIOrderManager;

    void Start()
    {
        uIOrderManager = FindObjectOfType<UIOrderManager>();


        abilitiesButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(abilityPanel);
            abilitiesButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
        guildButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(guildPanel);
            guildButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
        boostButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(boostPanel);
            boostButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
        friendButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(friendPanel);
            friendButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
        questButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(questPanel);
            questButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
        optionsButton.onClick.AddListener(() =>
        {
            uIOrderManager.InstantiatePanel(optionsPanel);
            questButton.GetComponent<ButtonAudioPlayer>().PlaySound();
        });
    }
}
