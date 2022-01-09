using Michsky.UI.ModernUIPack;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIConfirmation : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public static UIConfirmation singleton;
    public NetworkManagerMMO manager;
    public Button declineButton;
    public Button acceptButton;

    public UIConfirmation()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    public void Start()
    {
        nameText.text = "Do you want really delete the selected character?\nThis character will be not available anymore!";
        declineButton.onClick.SetListener(() =>
        {
            transform.gameObject.SetActive(false);
        });
        acceptButton.onClick.SetListener(() =>
        {
            DeletePlayer();
        });
    }

    public void DeletePlayer()
    {
        NetworkClient.Send(new CharacterDeleteMsg { index = manager.selection });
        declineButton.onClick.Invoke();
    }

}