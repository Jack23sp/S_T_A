using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIModularBuildingConfirm : MonoBehaviour
{
    public Button confirmButton;
    public Button cancelButton;

    void Update()
    {
        confirmButton.onClick.SetListener(() =>
        {
            if(UIModularBuildingManager.singleton.clickedCenter)
            {
                UIModularBuildingManager.singleton.CheckClickedButton();
                Destroy(UIModularBuildingManager.singleton.gameObject);
                Destroy(this.gameObject);
            }
            else
            {
                UIModularBuildingManager.singleton.CheckClickedButton();
                Destroy(this.gameObject);
            }
        });

        cancelButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
    }
}
