using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassObject : MonoBehaviour
{
    public SortByDepth sortByDepth;

    public void DisableDepth()
    {
        if(sortByDepth) sortByDepth.enabled = true;
        sortByDepth.SetOrder();
        Destroy(sortByDepth);
        Destroy(this);
    }

}
