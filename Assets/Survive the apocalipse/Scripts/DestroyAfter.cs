// Destroys the GameObject after a certain time.
using UnityEngine;
using Mirror;

public class DestroyAfter : MonoBehaviour
{
    public float time = 1;
    public bool activeSound;
    public AudioSource audioSource;
    void Start()
    {
        if(Player.localPlayer && activeSound)
        {
            if(!Player.localPlayer.playerOptions.blockSound)
            {
                if (!audioSource)
                    audioSource = GetComponent<AudioSource>();
                audioSource.gameObject.SetActive(true);
            }
        }
        //if (GetComponent<Entity>() && GetComponent<Entity>().isServer)
        //{
        //    NetworkServer.Destroy(this.gameObject);
        //    return;
        //}
        //else
            Destroy(gameObject, time);
    }
}
