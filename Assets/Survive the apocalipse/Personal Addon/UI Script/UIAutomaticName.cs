using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAutomaticName : MonoBehaviour
{
    public UICharacterCreationCustom uICharacterCreationCustom;
    public Button pickName;
    public Button closeButton;
    public TextMeshProUGUI message;

    void Update()
    {
        pickName.onClick.SetListener(() =>
        {
            uICharacterCreationCustom = FindObjectOfType<UICharacterCreationCustom>();
            uICharacterCreationCustom.nameInput.text = "Player" + UnityEngine.Random.Range(0, 10000).ToString();
            closeButton.onClick.Invoke();
        });

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
    }

}
