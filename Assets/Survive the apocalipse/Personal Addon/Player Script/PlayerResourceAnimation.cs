using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResourceAnimation : MonoBehaviour
{
    public void ShakeCamera()
    {
        if (Player.localPlayer.target && (Player.localPlayer.target is Tree || Player.localPlayer.target is Rock || Player.localPlayer.target is Building))
        {
            if (Player.localPlayer.playerMonsterGrab.customCameraShake)
            {
                Player.localPlayer.playerMonsterGrab.customCameraShake.animator.SetBool("SHAKERESOURCE", true);
            }
            else
            {
                Player.localPlayer.playerMonsterGrab.customCameraShake = FindObjectOfType<CustomCameraShake>();
                Player.localPlayer.playerMonsterGrab.customCameraShake.animator.SetBool("SHAKERESOURCE", true);
            }
        }
    }

    public void StopShakeCamera()
    {
        if (Player.localPlayer.playerMonsterGrab.customCameraShake)
        {
            Player.localPlayer.playerMonsterGrab.customCameraShake.animator.SetBool("SHAKERESOURCE", false);
        }
        else
        {
            Player.localPlayer.playerMonsterGrab.customCameraShake = FindObjectOfType<CustomCameraShake>();
            Player.localPlayer.playerMonsterGrab.customCameraShake.animator.SetBool("SHAKERESOURCE", false);
        }
    }
}
