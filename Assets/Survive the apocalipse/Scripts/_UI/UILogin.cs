// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;

public partial class UILogin : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerMMO manager; // singleton=null in Start/Awake
    public NetworkAuthenticatorMMO auth;
    public GameObject panel;
    public InputField accountInput;
    public InputField passwordInput;
    public Button loginButton;
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Transform content;
    public GameObject textServer;
    public GameObject serverObject;
    private Color tempColor;
    private string ms;
    public static UILogin singleton;

    public int selectedHost = -1;

    public Image loginImageBackground;
    public Image loginImageInsideButton;
    public TextMeshProUGUI loginText;

    public Material loginBackgroundImageToApply;
    public Material cancelBackgroundImageToApply;
    public Sprite cancelimageInsideButtonToApply;
    public Sprite loginImageInsideButtonToApply;

    public GameObject backgroundObject;

    void Start()
    {
        if (!singleton) singleton = this;

        UIUtils.BalancePrefabs(serverObject, manager.serverList.Count, content);
        for (int i = 0; i < manager.serverList.Count; i++)
        {
            int index = i;
            UIServerSlot slot = content.GetChild(index).GetComponent<UIServerSlot>();
            CheckPing(manager.serverList[index].ip, index);
            slot.serverText.text = manager.serverList[index].name;
            if (slot.msText.text != string.Empty)
            {
                if (Convert.ToInt32(slot.msText.text) <= GeneralManager.singleton.goodServerPick)
                {
                    tempColor = slot.slotOn.color;
                    tempColor = Color.green;
                    slot.slotOn.color = tempColor;
                }
                else if (Convert.ToInt32(slot.msText.text) > GeneralManager.singleton.goodServerPick && Convert.ToInt32(slot.msText.text) <= GeneralManager.singleton.mediumServicePick)
                {
                    tempColor = slot.slotOn.color;
                    tempColor = Color.yellow;
                    slot.slotOn.color = tempColor;
                }
                else if (Convert.ToInt32(slot.msText.text) > GeneralManager.singleton.mediumServicePick)
                {
                    tempColor = slot.slotOn.color;
                    tempColor = Color.red;
                    slot.slotOn.color = tempColor;
                }
            }

            slot.serverButton.onClick.SetListener(() =>
            {
                foreach (Transform t in content)
                {
                    t.GetComponent<Outline>().enabled = false;
                }
                selectedHost = index;
                slot.GetComponent<Outline>().enabled = true;
                manager.networkAddress = manager.serverList[index].ip;
            });
        }
        if (PlayerPrefs.HasKey("LastServer"))
        {
            selectedHost = PlayerPrefs.GetInt("LastServer");
            content.GetChild(selectedHost).GetComponent<UIServerSlot>().serverButton.onClick.Invoke();
        }

    }


    void Update()
    {
        accountInput.gameObject.SetActive(true);
        hostButton.gameObject.SetActive(true);
        dedicatedButton.gameObject.SetActive(true);

        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);

            loginButton.interactable = auth.IsAllowedAccountName(accountInput.text) && selectedHost > -1;
            hostButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            hostButton.onClick.SetListener(() => { manager.StartHost(); });
            dedicatedButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive;
            dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });

            if (manager.IsConnecting())
            {
                loginImageBackground.material = cancelBackgroundImageToApply;
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.SetListener(() => { manager.StopClient(); });
            }
            else
            {
                loginImageBackground.material = loginBackgroundImageToApply;
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.SetListener(() => { PlayerPrefs.SetInt("LastServer", selectedHost); manager.StartClient();  });
            }

            for (int i = 0; i < manager.serverList.Count; i++)
            {
                int index = i;
                CheckPing(manager.serverList[index].ip, index);
            }

                // inputs
                auth.loginAccount = accountInput.text;
                auth.loginPassword = passwordInput.text;
        }
        else
        {
            if (backgroundObject)  Destroy(backgroundObject);
            panel.SetActive(false);
        }
    }

    public void CheckPing(string ip, int index)
    {
        StartCoroutine(StartPing(ip, index));
    }

    IEnumerator StartPing(string ip, int index)
    {
        WaitForSeconds f = new WaitForSeconds(2.0f);
        Ping p = new Ping(ip);
        while (p.isDone == false)
        {
            yield return f;
        }
        PingFinished(p , index);
    }


    public void PingFinished(Ping p , int index)
    {
        content.GetChild(index).GetComponent<UIServerSlot>().msText.text = (p.time).ToString();
    }
}
