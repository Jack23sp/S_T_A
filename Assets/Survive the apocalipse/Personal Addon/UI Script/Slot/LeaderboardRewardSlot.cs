using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRewardSlot : MonoBehaviour
{
    public Image image;
    public Text rewardName;
    public Text rewardAmount;
    public Button rewardButton;
    public Image imageToColor;

    public int index;
    public int category;

    public void Start()
    {
        image.preserveAspect = true;
        imageToColor.preserveAspect = true;
    }
}
