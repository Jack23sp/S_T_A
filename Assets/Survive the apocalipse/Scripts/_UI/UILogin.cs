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
    public Text statusText;
    public Text nameText;
    public InputField accountInput;
    public InputField passwordInput;
    //public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;
    public Transform content;
    public GameObject textServer;
    public GameObject serverObject;
    private Color tempColor;
    private string ms;
    public static UILogin singleton;
    //public GameObject background;

    public int selectedHost = -1;

    public Image loginImageBackground;
    public Image loginImageInsideButton;
    public TextMeshProUGUI loginText;

    public Sprite loginBackgroundImageToApply;
    public Sprite cancelBackgroundImageToApply;
    public Sprite cancelimageInsideButtonToApply;
    public Sprite loginImageInsideButtonToApply;

    public GameObject backgroundObject;

    void Start()
    {
        if (!singleton) singleton = this;
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer"))
        {
            string last = PlayerPrefs.GetString("LastServer", "");
            //serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
        }
    }

    void OnDestroy()
    {
        // save last server by name in case order changes some day
        //PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
    }

    void Update()
    {
        //if (Player.localPlayer)
        //{
        //    Destroy(background.gameObject);
        //    Destroy(this);
        //}
        //else
        //{
        //    background.SetActive(manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake);
        //}

        //if (Application.isMobilePlatform)
        //{
        //    textServer.gameObject.SetActive(false);
        //    accountInput.gameObject.SetActive(false);
        //    hostButton.gameObject.SetActive(false);
        //    dedicatedButton.gameObject.SetActive(false);
        //    quitButton.gameObject.SetActive(false);
        //}
        //else
        //{
        //    textServer.gameObject.SetActive(true);
            accountInput.gameObject.SetActive(true);
        hostButton.gameObject.SetActive(true);
        dedicatedButton.gameObject.SetActive(true);
        //    quitButton.gameObject.SetActive(false);
        //}

        // only show while offline
        // AND while in handshake since we don't want to show nothing while
        // trying to login and waiting for the server's response
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);

            // status
            //if (manager.IsConnecting())
            //    statusText.text = "Connecting...";
            //else if (manager.state == NetworkState.Handshake)
            //    statusText.text = "Handshake...";
            //else
            //    statusText.text = "";
            // buttons. interactable while network is not active
            // (using IsConnecting is slightly delayed and would allow multiple clicks)
            //registerButton.interactable = !manager.isNetworkActive;
            //registerButton.onClick.SetListener(() => { uiPopup.Show(registerMessage); });
            //loginButton.interactable = !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text) && selectedHost > -1;
            loginButton.interactable = auth.IsAllowedAccountName(accountInput.text) && selectedHost > -1;
            //loginButton.onClick.SetListener(() => { manager.StartClient(); });
            hostButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            hostButton.onClick.SetListener(() => { manager.StartHost(); });
            //cancelButton.gameObject.SetActive(manager.IsConnecting());
            //cancelButton.onClick.SetListener(() => { manager.StopClient(); });
            dedicatedButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive;
            dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
            quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });

            if (manager.IsConnecting())
            {
                loginImageBackground.sprite = cancelBackgroundImageToApply;
                //loginImageInsideButton.sprite = cancelimageInsideButtonToApply;
                //loginText.text = "Cancel";
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.SetListener(() => { manager.StopClient(); });
            }
            else
            {
                loginImageBackground.sprite = loginBackgroundImageToApply;
                //loginImageInsideButton.sprite = loginImageInsideButtonToApply;
                //loginText.text = "Enter";
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.SetListener(() => { manager.StartClient(); });
            }

            // inputs
            auth.loginAccount = accountInput.text;
            auth.loginPassword = passwordInput.text;

            // copy servers to dropdown; copy selected one to networkmanager ip/port.
            //serverDropdown.interactable = !manager.isNetworkActive;
            //serverDropdown.options = manager.serverList.Select(
            //    sv => new Dropdown.OptionData(sv.name)
            //).ToList();

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
        // stuff when the Ping p has finshed....
        content.GetChild(index).GetComponent<UIServerSlot>().msText.text = (p.time).ToString();
    }
}
