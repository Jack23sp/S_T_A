using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="uMMORPG item/ Bundle item", order =999)]
public class BundleItem : ScriptableObject
{
    public List<Bundleitems> bundleitems;

    public long coins;
}

[System.Serializable]
public struct Bundleitems
{
    public ScriptableItem item;
    public int quantity;
}