// simple script to always set y position to order in layer for visiblity
using UnityEngine;

public class SortByDepth : MonoBehaviour
{
#pragma warning disable CS0109 // member does not hide accessible member
    new public SpriteRenderer renderer;
#pragma warning restore CS0109 // member does not hide accessible member

    // precision is useful for cases where two players stand at
    //   y=0 and y=0.1, which would both be sortingOrder=0 otherwise
    public float precision = 100;

    // offset in case it's needed (e.g. for mounts that should be behind the
    // player, even if the player is above it in .y)
    public int offset = 0;

    public bool relatedToPlayer = false;
    public int amountRelatedToPlayer = 1;

    private void Start()
    {
        if (!renderer) renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (relatedToPlayer)
        {
            if(Player.localPlayer)
                renderer.sortingOrder = Player.localPlayer.spriteRenderer.sortingOrder - amountRelatedToPlayer;
        }
        else
        {
            // we negate it because that's how Unity's sorting order works
            renderer.sortingOrder = -Mathf.RoundToInt((transform.position.y + offset) * precision);
        }
    }

    public void SetOrder()
    {
        if (relatedToPlayer)
        {
            if (Player.localPlayer)
                renderer.sortingOrder = Player.localPlayer.spriteRenderer.sortingOrder - amountRelatedToPlayer;
        }
        else
        {
            // we negate it because that's how Unity's sorting order works
            renderer.sortingOrder = -Mathf.RoundToInt((transform.position.y + offset) * precision);
        }
    }
}
