using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerValueCatcher : MonoBehaviour
{
    public static PlayerValueCatcher singleton;
    public Player player;

    public float armorPercent;
    [HideInInspector] public float prevArmor;

    public float healthPercent;
    [HideInInspector] public int prevHealth;

    public float manaPercent;
    [HideInInspector] public int prevMana;

    public int hungry;
    private int prevHungry;

    public int thirsty;
    private int prevThirsty;



    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.playerArmor.currentArmor != prevArmor)
        {
            armorPercent = player.playerArmor.ArmorPercent();
            prevArmor = player.playerArmor.currentArmor;
        }

        if (player.health != prevHealth)
        {
            healthPercent = player.HealthPercent();
            prevHealth = player.health;
        }

        if (player.mana != prevMana)
        {
            manaPercent = player.ManaPercent();
            prevMana = player.mana;
        }

        if (player.playerThirsty.currentThirsty != prevThirsty)
        {
            thirsty = player.playerThirsty.currentThirsty;
            prevThirsty = player.playerThirsty.currentThirsty;
        }

        if (player.playerHungry.currentHungry != prevHungry)
        {
            hungry = player.playerHungry.currentHungry;
            prevHungry = player.playerHungry.currentHungry;
        }

    }

}

public partial class Player
{
    public bool running;

    public bool moving;

    public bool casting;

    public bool stunned;

    public bool dead;

    public float movingX;

    public float movingY;

    public float joystickAdapter;
    public float prevJoystickAdapter;

    public float prevJoystickeMultiplierNormal;
    public float prevJoystickeMultiplierSneak;
    public float prevJoystickeMultiplierRun;

    private float multiplier;

    public AnimatorControllerParameter[] parameters;

    public List<Animator> animators = new List<Animator>();

    public void AnimationManagerMOVING(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("isMoving", player.IsMoving() && player.state != "CASTING" && !player.IsMounted());
        }
    }
    public void AnimationManagerRUNNING(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("isRunning", player.playerMove.run);
        }
    }

    public void AnimationManagerCASTING(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("CASTING", player.state == "CASTING");
        }
    }

    public void AnimationManagerSTUNNED(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("STUNNED", player.state == "STUNNED");
        }
    }

    public void AnimationManagerDEAD(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("DEAD", player.state == "DEAD");
        }
    }

    public void AnimationManagerMOVEX(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetFloat("moveX", player.playerMove.x);
        }
    }

    public void AnimationManagerMOVEY(Player player)
    {
        foreach (Animator anim in animators)
        {
            anim.SetFloat("LookY", player.playerMove.y);
        }
    }

    public void JoystickManager(Player player)
    {
        parameters = player.animator.parameters;
        multiplier = parameters[6].defaultFloat;
        joystickAdapter = parameters[7].defaultFloat;

        if (joystickAdapter != player.playerMove.major)
        {
            foreach (Animator anim in animators)
            {
                anim.SetFloat("JoystickAdapter", player.playerMove.major);
            }
        }
    }
    public void JoystickMultiplierNormal(Player player)
    {
        if (multiplier != prevJoystickeMultiplierNormal)
        {

            foreach (Animator anim in animators)
            {
                anim.SetFloat("MoveMultiplier", GeneralManager.singleton.normalMultiplier * player.playerMove.major);
            }
            prevJoystickeMultiplierNormal = multiplier;
        }
    }

    public void JoystickMultiplierRun(Player player)
    {
        if (multiplier != prevJoystickeMultiplierRun)
        {

            foreach (Animator anim in animators)
            {
                anim.SetFloat("MoveMultiplier", GeneralManager.singleton.runMultiplier * player.playerMove.major);
            }
            prevJoystickeMultiplierRun = multiplier;
        }
    }

    public void JoystickMultiplierSneak(Player player)
    {
        if (multiplier != prevJoystickeMultiplierSneak)
        {
            foreach (Animator anim in animators)
            {
                anim.SetFloat("MoveMultiplier", GeneralManager.singleton.sneakMultiplier * player.playerMove.major);
            }
            prevJoystickeMultiplierSneak = multiplier;
        }
    }
}


public partial class Monster
{
    public bool moving;

    public bool casting;

    public bool stunned;

    public bool dead;

    public float movingX;

    public float movingY;


    public AnimatorControllerParameter[] parameters;

    public void AnimationManagerMonsterMOVING(Monster monster)
    {
        if (moving != (monster.state == "MOVING" && monster.agent.velocity != Vector2.zero))
        {
            moving = (monster.state == "MOVING" && monster.agent.velocity != Vector2.zero);
            monster.animator.SetBool("MOVING", moving);
        }
    }

    public void AnimationManagerMonsterCASTING(Monster monster)
    {
        if (casting != (monster.state == "CASTING"))
        {
            casting = (monster.state == "CASTING");
            monster.animator.SetBool("CASTING", casting);
        }
    }

    public void AnimationManagerMonsterSTUNNED(Monster monster)
    {
        if (stunned != (monster.state == "STUNNED"))
        {
            stunned = (monster.state == "STUNNED");
            monster.animator.SetBool("STUNNED", stunned);
        }
    }

    public void AnimationManagerMonsterDEAD(Monster monster)
    {
        if (dead != (monster.state == "DEAD"))
        {
            dead = (monster.state == "DEAD");
            monster.animator.SetBool("DEAD", dead);
        }
    }

    public void AnimationManagerMonsterMOVEX(Monster monster)
    {
        if (movingX != (monster.lookDirection.x))
        {
            movingX = monster.lookDirection.x;
            monster.animator.SetFloat("LookX", movingX);
        }
    }

    public void AnimationManagerMonsterMOVEY(Monster monster)
    {
        if (movingY != (monster.lookDirection.x))
        {
            movingY = monster.lookDirection.y;
            monster.animator.SetFloat("LookY", movingY);
        }
    }
}