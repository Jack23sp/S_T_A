using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisibleHealthArmor : MonoBehaviour
{
    public Image backgroundImage;
    public Image healthSlider;
    public Image armorSlider;
    public Image manaSlider;

    public Entity entity;
    public Entity otherEntity;

    public float prevHealthPerc;
    public float prevArmorPerc;
    public float prevManarPerc;

    public Canvas canvas;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if(entity == Player.localPlayer)
            InvokeRepeating(nameof(CheckStatUnderPlayer), 0.0f, 0.3f);
        canvas.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        healthSlider.gameObject.SetActive(false);
    }

    void CheckStatUnderPlayer()
    {
        for (int i = 0; i < Player.localPlayer.playerMove.sorted.Count; i++)
        {
            otherEntity = Player.localPlayer.playerMove.sorted[i].GetComponent<Entity>();
                if (otherEntity is Monster || otherEntity is Rock || otherEntity is Tree)
                {
                    if (otherEntity.HealthPercent() != prevHealthPerc)
                    {
                        healthSlider.fillAmount = otherEntity.HealthPercent();
                        prevHealthPerc = otherEntity.HealthPercent();
                    }
                }
                if (otherEntity is Player && ((Player)otherEntity) != entity)
                {
                        ((Player)otherEntity).UIVisibleHealthArmor.canvas.gameObject.SetActive(true);
                        ((Player)otherEntity).UIVisibleHealthArmor.backgroundImage.gameObject.SetActive(true);
                        ((Player)otherEntity).UIVisibleHealthArmor.healthSlider.gameObject.SetActive(true);
                        ((Player)otherEntity).UIVisibleHealthArmor.healthSlider.fillAmount = otherEntity.HealthPercent();
                        ((Player)otherEntity).UIVisibleHealthArmor.armorSlider.fillAmount = ((Player)otherEntity).playerArmor.ArmorPercent();
                        ((Player)otherEntity).UIVisibleHealthArmor.manaSlider.fillAmount = ((Player)otherEntity).ManaPercent();
                }
            }
        }
    }
