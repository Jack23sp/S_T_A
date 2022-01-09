using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfettiPiece : MonoBehaviour
{
    //Random modifier value
    float randomModifier;

    //Random piece size
    Vector2 dimensions;

    //piece front and back color.
    ConfettiColor color;

    //Current piece velocity
    Vector2 velocity;

    //Current piece rotation value
    float rotation = 0;

    //Parent confettiEffectGUI class
    ConfettiEffectGUI effect;

    //Draw front or back color GUI control
    Image img;

    // Use this for initialization
    void Start()
    {

    }

    public void Setup(ConfettiEffectGUI effect, ConfettiColor color, Vector3 pos)
    {
        //Set parent confettiEffectGUI class 
        this.effect = effect;
        //set front and back color.
        this.color = color;

        //Set starting position
        transform.position = pos;

        //Random piece size
        //dimensions = new Vector2(Random.Range(5, 9), Random.Range(8, 15));
        dimensions = new Vector2(Random.Range(0.1f, 1f), Random.Range(0.5f, 1f));

        //Random modifier value
        randomModifier = Random.Range(0, 99);

        //Random starting rotation
        rotation = Random.Range(0f, 2f * Mathf.PI) * Mathf.Rad2Deg;

        //Random confetti velocity
        velocity = initConfettiVelocity(-9, 9, -6, -11);

        //Find and set Image to front color.
        img = GetComponent<Image>();
        if (img != null)
        {
            img.color = color.frontColor;
        }

        //scale = new Vector2(1, 1);

    }

    // Update is called once per frame
    void Update()
    {
        //Calculation velocity vakye
        velocity.x -= velocity.x * effect.dragConfetti;
        velocity.y = Mathf.Min(velocity.y + effect.gravityConfetti, effect.terminalVelocity);
        velocity.x += (Random.value > 0.5f) ? Random.value : -Random.value;

        //Get current position
        Vector3 pos = transform.position;

        //Calculation new position
        pos.x += velocity.x;
        pos.y -= velocity.y;

        //Calculate size based on Y-axis position
        float sy = Mathf.Cos((pos.y + randomModifier) * 0.09f);

        //Set new position,scale and rotation
        transform.position = pos;
        transform.localScale = new Vector3(dimensions.x * sy, dimensions.y * sy, 1);

        transform.rotation = Quaternion.Euler(rotation, rotation, 1);

        //whether to display the front color or the back color?
        img.color = (sy > 0) ? color.frontColor : color.backColor;


    }

    /// <summary>
    /// Random confetti velocity
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="y1"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    Vector2 initConfettiVelocity(float x1, float x2, float y1, float y2)
    {
        //Random x value
        float x = Random.Range(x1, x2);
        float range = y2 - y1 + 1;
        float y = y2 - Mathf.Abs(Random.Range(0, range) + Random.Range(0, range) - range);
        if (y >= y2 - 1)
        {
            //Random y value between 0 - 3
            y += (Random.value < 0.25f) ? Random.Range(1, 3) : 0;
        }

        return new Vector2(x, y);
    }
}
