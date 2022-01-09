using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationSlot : MonoBehaviour
{
    public Animator animator;
    public TextMeshProUGUI description;
    public Button closeButton;
    public GameObject itemParticle;
    public GameObject goldParticle;
    public GameObject coinParticle;

    public string purchaseType;

    void Start()
    {
        if(purchaseType == "Item")
        {
            if(itemParticle) itemParticle.SetActive(true);
        }
        else if (purchaseType == "Gold")
        {
            if (goldParticle) goldParticle.SetActive(true);
        }
        else if (purchaseType == "Coin")
        {
            if (coinParticle) coinParticle.SetActive(true);
        }

        Invoke(nameof(CloseNotification), 3.0f);
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
    }

    void CloseNotification()
    {
        animator.SetBool("EXIT", true);
    }
}
