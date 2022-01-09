using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class SpawnedEmoji : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    [SyncVar] public string emojiName;
    [SyncVar] public string playerName;

    public void Start()
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(playerName, out onlinePlayer))
        {
            if (GeneralManager.singleton.FindNetworkEmoji(emojiName, playerName) >= 0)
            {
                for (int i = 0; i < GeneralManager.singleton.listCompleteOfEmoji.Count; i++)
                {
                    if (GeneralManager.singleton.listCompleteOfEmoji[i].name.Contains(emojiName))
                    {
                        spriteRenderer.sprite = GeneralManager.singleton.listCompleteOfEmoji[i].emojiImg;
                    }
                }
            }
            transform.SetParent(onlinePlayer.transform);
            transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
        }
    }
}
