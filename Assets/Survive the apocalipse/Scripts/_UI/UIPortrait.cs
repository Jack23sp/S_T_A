using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIPortrait : MonoBehaviour
{
    public static UIPortrait singleton;
    public GameObject panel;
    public TextMeshProUGUI levelText;

    public void Start()
    {
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(SetInitialLevel), 0.0f, 0.2f);
    }

    public void SetInitialLevel()
    {
        Player player = Player.localPlayer;

        if (player)
        {
            panel.SetActive(true);
            levelText.text = player.level.ToString();
            CancelInvoke(nameof(SetInitialLevel));
        }
        else panel.SetActive(false);
    }

    public void RefreshLevel()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            levelText.text = player.level.ToString();
        }
    }

    void Update()
    {
        Player player = Player.localPlayer;

        if (player)
        {
            panel.SetActive(true);
            levelText.text = player.level.ToString();
        }
        else panel.SetActive(false);
    }
}
