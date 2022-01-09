using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGroup : MonoBehaviour
{
    public static UIGroup singleton;
    public Transform groupContent;
    public GameObject allyToSpawn;

    public Transform personalGroupContent;
    public GameObject personalGroupToSpawn;

    public Transform partyContent;
    public GameObject partToSpawn;

    public Guild selectedGuild;
    public Party party;

    public List<Guild> allyList = new List<Guild>();
    private Player player;

    public GameObject guildObject;
    public GameObject partyObject;

    public Button groupButton;

    public string selectedGroup;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        groupButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.createGroupPanel, GeneralManager.singleton.canvas);
        });


        if (player.health == 0) UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();

        groupButton.interactable = !player.InGuild();
        UIUtils.BalancePrefabs(personalGroupToSpawn, player.InGuild() ? 1 : 0, personalGroupContent);
        for (int i = 0; i < personalGroupContent.childCount; i++)
        {
            int index = i;
            GroupSlot slot = personalGroupContent.GetChild(index).GetComponent<GroupSlot>();
            slot.statName.text = player.guild.name;
            slot.statAmount.text = player.guild.members.Length + " / " + GuildSystem.Capacity;
            slot.statButton.onClick.SetListener(() =>
            {
                selectedGuild = player.guild;
                UIOrderManager.singleton.SingleInstantePanel(guildObject);
            });
        }

        UIUtils.BalancePrefabs(allyToSpawn, player.InParty() ? 1 : 0, partyContent);
        for (int i = 0; i < partyContent.childCount; i++)
        {
            int index = i;
            GroupSlot slot = partyContent.GetChild(index).GetComponent<GroupSlot>();
            slot.statAmount.text = player.party.members.Length + " / " + Party.Capacity;
            slot.statName.text = player.party.master + "'s party";
            slot.statButton.onClick.SetListener(() =>
            {
                UIOrderManager.singleton.SingleInstantePanel(partyObject);
            });
        }

        UIUtils.BalancePrefabs(allyToSpawn, player.playerAlliance.guildAlly.Count, groupContent);
        for (int i = 0; i < player.playerAlliance.guildAlly.Count; i++)
        {
            int index = i;
            GroupSlot slot = groupContent.GetChild(index).GetComponent<GroupSlot>();
            slot.statName.text = player.playerAlliance.guildAlly[index];
            slot.statAmount.text = "";
            slot.statButton.onClick.SetListener(() =>
            {
                player.playerAlliance.CmdLoadGuild(player.playerAlliance.guildAlly[index]);
                selectedGroup = player.playerAlliance.guildAlly[index];
                UIOrderManager.singleton.SingleInstantePanel(guildObject);
                UIOrderManager.singleton.SingleInstantePanel(guildObject).gameObject.GetComponent<UIGuild>().notMyGroup = true;
                //UIOrderManager.singleton.singleTimePanel[UIOrderManager.singleton.singleTimePanel.Count - 1].gameObject.GetComponent<UIGuild>().leaveButton.gameObject.SetActive(false);
            });
        }

    }
}
