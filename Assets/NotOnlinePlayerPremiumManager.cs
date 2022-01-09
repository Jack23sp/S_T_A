using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotOnlinePlayerPremiumManager : MonoBehaviour
{
    public Player player;

    public bool inPremiumZone;
    public GameObject nameNormal;
    public GameObject nameSpecial;

    public TextMeshProUGUI textNormal;
    public TextMeshProUGUI textSpecial;

    public UICharacterCreationCustom characterCreationCustom;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Player.localPlayer) Destroy(this);
        if (!characterCreationCustom) characterCreationCustom = FindObjectOfType<UICharacterCreationCustom>();

        if (characterCreationCustom && characterCreationCustom.selection.creationPanel.activeInHierarchy)
        {
            player.nameOverlay = textNormal;
            nameNormal.SetActive(false);
            nameSpecial.SetActive(false);
        }
        else
        {
            if (!player.isClient && !player.isServer)
            {
                if (inPremiumZone)
                {
                    player.nameOverlay = textSpecial;
                    nameNormal.SetActive(false);
                    nameSpecial.SetActive(true);
                }
                else
                {
                    player.nameOverlay = textNormal;
                    nameNormal.SetActive(true);
                    nameSpecial.SetActive(false);
                    textNormal.color = Color.white;
                }
            }
        }
    }
}
