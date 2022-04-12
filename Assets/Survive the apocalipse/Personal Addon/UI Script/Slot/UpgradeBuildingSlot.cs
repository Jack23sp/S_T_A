using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBuildingSlot : MonoBehaviour
{
    //public Button buttonSelectAmmo;
    public Image upgradeImage;
    public Text upgradeName;
    public Text upgradeAmount;
    public Image principalImage;

    public void Start()
    {
        upgradeImage.preserveAspect = true;
        principalImage.preserveAspect = true;
    }
}
