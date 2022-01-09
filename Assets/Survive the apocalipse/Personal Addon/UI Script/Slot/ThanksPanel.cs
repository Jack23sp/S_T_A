using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThanksPanel : MonoBehaviour
{
    public Animator animator;

    public Button closeButton;
    public TextMeshProUGUI pointText;
    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("OPEN", true);
        closeButton.onClick.SetListener(() =>
        {
            Close();
        });
        pointText.text = "Reach : " + Player.localPlayer.playerItemPoint.point + " / " + Player.localPlayer.playerItemPoint.maxPoint + " to take :\n10.000 gold!";
    }



    public void Close()
    {
        animator.SetBool("OPEN", false);
        Destroy(this.gameObject, 1.0f);
    }

}
