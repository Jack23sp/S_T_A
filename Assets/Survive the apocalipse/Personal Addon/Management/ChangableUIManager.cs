using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangableUIManager : MonoBehaviour
{
    //public static ChangableUIManager singleton;
    public List<UIChangeable> change = new List<UIChangeable>();
    public Font fontStyle;
    //public void Start()
    //{
    //    if (!singleton) singleton = this;
    //}
}

[Serializable]
public partial class UIChangeable
{
    public string category;
    public bool active;
    public List<ChangeUIType> changeUITypes = new List<ChangeUIType>();

}


[Serializable]
public partial class ChangeUIType
{
    public UIType imageType;
    public Sprite spriteImg;
}
