using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public partial class UIGuild : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.G;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI masterText;
    public TextMeshProUGUI currentCapacityText;
    public TextMeshProUGUI maximumCapacityText;
    public InputField noticeInput;
    public Button noticeEditButton;
    public Button noticeSetButton;
    public UIGuildMemberSlot slotPrefab;
    public Transform memberContent;
    //public Color onlineColor = Color.cyan;
    //public Color offlineColor = Color.gray;
    public Button leaveButton;
    public Button leaveAlly;
    private Guild guild;
    public bool notMyGroup;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if(UIGroup.singleton)
            {
                guild = UIGroup.singleton.selectedGuild;
            }
                int memberCount = guild.members != null ? guild.members.Length : 0;

                // guild properties
                nameText.text = guild.name;
                masterText.text = guild.master;
                currentCapacityText.text = memberCount.ToString();
                maximumCapacityText.text = GuildSystem.Capacity.ToString();

                // notice edit button
                noticeEditButton.interactable = guild.CanNotify(player.name) &&
                                                !noticeInput.interactable;
                noticeEditButton.onClick.SetListener(() => {
                    noticeInput.interactable = true;
                });

                // notice set button
                noticeSetButton.interactable = guild.CanNotify(player.name) &&
                                               noticeInput.interactable &&
                                               NetworkTime.time >= player.nextRiskyActionTime;
                noticeSetButton.onClick.SetListener(() => {
                    noticeInput.interactable = false;
                    if (noticeInput.text.Length > 0 &&
                        !string.IsNullOrWhiteSpace(noticeInput.text) &&
                        noticeInput.text != guild.notice) {
                        player.CmdSetGuildNotice(noticeInput.text);
                    }
                });

                // notice input: copies notice while not editing it
                if (!noticeInput.interactable) noticeInput.text = guild.notice ?? "";
                noticeInput.characterLimit = GuildSystem.NoticeMaxLength;

                // leave
                leaveButton.gameObject.SetActive(guild.name == player.guild.name && (guild.CanLeave(player.name) || player.guild.CanTerminate(player.name)));
                //leaveButton.interactable = guild.CanLeave(player.name) || player.guild.CanTerminate(player.name);
                leaveButton.onClick.SetListener(() => {
                    if (player.InGuild() && player.guild.CanTerminate(player.name))
                    {
                        UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                        player.CmdTerminateGuild();
                    }
                    else if (player.InGuild() && !player.guild.CanTerminate(player.name))
                    {
                        player.CmdLeaveGuild();
                        UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                    }

                });

                leaveAlly.gameObject.SetActive (notMyGroup == true);
                leaveAlly.onClick.SetListener(() => {
                    player.playerAlliance.CmdRemoveAllyGuild(guild.name, player.guild.name);
                    UIGroup.singleton.selectedGroup = string.Empty;
                    UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                });

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, guild.members.Length, memberContent);

                // refresh all members
                for (int i = 0; i < guild.members.Length; i++)
                {
                    UIGuildMemberSlot slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();
                    GuildMember member = guild.members[i];

                    slot.onlineStatusImage.color = member.online ? GeneralManager.singleton.onlineColor : GeneralManager.singleton.offlineColor;
                    slot.nameText.text = member.name;
                    slot.levelText.text = member.level.ToString();
                    slot.rankText.text = member.rank.ToString();
                    slot.promoteButton.interactable = guild.CanPromote(player.name, member.name);
                    slot.promoteButton.onClick.SetListener(() => {
                        player.CmdGuildPromote(member.name);
                    });
                    slot.demoteButton.interactable = guild.CanDemote(player.name, member.name);
                    slot.demoteButton.onClick.SetListener(() => {
                        player.CmdGuildDemote(member.name);
                    });
                    slot.kickButton.interactable = guild.CanKick(player.name, member.name);
                    slot.kickButton.onClick.SetListener(() => {
                        player.CmdGuildKick(member.name);
                    });
                }
        }
    }
}
