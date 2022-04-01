using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIHealthMana : MonoBehaviour
{
    public static UIHealthMana singleton;

    public GameObject panel;
    public Slider healthSlider;
    public TextMeshProUGUI healthStatus;
    public Slider manaSlider;
    public TextMeshProUGUI manaStatus;
    public Slider undergroundHealth;
    public Slider undergroundArmor;
    public Slider undergroundMana;

    public Slider armorSlider;
    public TextMeshProUGUI armorStatus;

    public Button wetImage;

    public Player player;

    public void Start()
    {
        if (!singleton) singleton = this;

        wetImage.onClick.SetListener(() =>
        {
            UINotificationManager.singleton.SpawnWetObjectIndication();
        });
        InvokeRepeating(nameof(UpdateStat), 0.0f, 0.4f);
    }

    public void Update()
    {
        if (player)
        {
            if (undergroundHealth.value > healthSlider.value)
            {
                undergroundHealth.value -= 0.2f * Time.deltaTime;
            }
            if (undergroundHealth.value < healthSlider.value)
            {
                undergroundHealth.value = healthSlider.value;
            }

            if (armorSlider)
            {
                if (undergroundArmor.value > armorSlider.value)
                {
                    undergroundArmor.value -= 0.2f * Time.deltaTime;
                }
                if (undergroundArmor.value < armorSlider.value)
                {
                    undergroundArmor.value = armorSlider.value;
                }
            }

            if (undergroundMana.value > manaSlider.value)
            {
                undergroundMana.value -= 0.2f * Time.deltaTime;
            }
            if (undergroundMana.value < manaSlider.value)
            {
                undergroundMana.value = manaSlider.value;
            }
        }
    }

    void UpdateStat()
    {
        if(!player) player = Player.localPlayer;
        if (player)
        {
            if (!armorSlider) armorSlider = player.playerArmor.instantiateObject.GetComponent<Slider>();

            panel.SetActive(true);

            if(player.playerWet)
            {
                if (player.playerWet.wetEquipmentNaked >= player.playerWet.avgWetEquipment)
                {
                    wetImage.image.enabled = true;
                }
                else if (player.playerWet.wetEquipment >= player.playerWet.avgWetEquipment)
                {
                    wetImage.image.enabled = true;
                }
                else
                {
                    wetImage.image.enabled = false;
                }
            }
            else
            {
                wetImage.image.enabled = false;
            }

            healthSlider.value = PlayerValueCatcher.singleton.healthPercent;
            healthStatus.text = PlayerValueCatcher.singleton.prevHealth + " / " + player.healthMax;

            manaSlider.value = PlayerValueCatcher.singleton.manaPercent;
            manaStatus.text = PlayerValueCatcher.singleton.prevMana + " / " + player.manaMax;           
        }
        else panel.SetActive(false);
    

    }
}
