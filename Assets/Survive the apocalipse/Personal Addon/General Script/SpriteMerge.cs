using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMerge : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;// assumes you've dragged a reference into this
    public Transform mergeInput;// a transform with a bunch of SpriteRenderers you want to merge

    public enum ROT90
    {
        NONE, ONE, TWO, THREE, NONE_FLIP, ONE_FLIP, TWO_FLIP, THREE_FLIP
    }

    void Start()
    {
        spriteRenderer.sprite = Create(new Vector2Int(2048, 2048), mergeInput);
    }

    public static Sprite Create(Vector2Int size, Transform input)
    {
        return Create(size, input, new Vector2Int());
    }

    /* Takes a transform holding many sprites as input and creates one flattened sprite out of them */
    public static Sprite Create(Vector2Int size, Transform input, Vector2Int offset)
    {
        var spriteRenderers = input.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderers.Length == 0)
        {
            Debug.Log("No SpriteRenderers found in " + input.name + " for SpriteMerge");
            return null;
        }

        var targetTexture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false, false);
        targetTexture.filterMode = FilterMode.Point;
        var targetPixels = targetTexture.GetPixels();
        for (int i = 0; i < targetPixels.Length; i++) targetPixels[i] = Color.clear;// default pixels are not set
        var targetWidth = targetTexture.width;
        int tempX, tempY;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            // Check for rotation:
            // not only does rotation do funny things with x/y, but by doing so you're jumping into the next tile
            // this means that the drawing ends up 1 pixel off, because you're not at the bottom of the current tile
            // but at the top of the next tile. Only 90 degree turns and sprite.flipX implemented so far.
            var rot = ROT90.NONE;
            var r = sr.transform.localRotation.eulerAngles.z;
            var pivot = sr.sprite.pivot;
            if (r == 90)
            {
                if (sr.flipX)
                {
                    rot = ROT90.ONE_FLIP;
                    pivot = new Vector2(-pivot.y + 1, -pivot.x + 1);
                }
                else
                {
                    rot = ROT90.ONE;
                    pivot = new Vector2(-pivot.y + 1, pivot.x);
                }
            }
            else if (r == 180)
            {
                if (sr.flipX)
                {
                    rot = ROT90.TWO_FLIP;
                    pivot = new Vector2(pivot.x, -pivot.y + 1);
                }
                else
                {
                    rot = ROT90.TWO;
                    pivot = new Vector2(pivot.x + 1, -pivot.y + 1);
                }
            }
            else if (r == 270)
            {// fun fact: if you enter -90 into the inspector it gets picked up as 270 in here
                if (sr.flipX)
                {
                    rot = ROT90.THREE_FLIP;
                    pivot = new Vector2(pivot.y, pivot.x);
                }
                else
                {
                    rot = ROT90.THREE;
                    pivot = new Vector2(pivot.y, -pivot.x + 1);
                }
            }
            else if (sr.flipX)
            {
                rot = ROT90.NONE_FLIP;
                pivot.x++;
            }
            var position = (Vector2)sr.transform.localPosition - pivot;
            Debug.Log(position);
            var p = new Vector2Int((int)position.x, (int)position.y) + offset;
            var sourceWidth = sr.sprite.texture.width;
            // if read/write is not enabled on texture (under Advanced) then this next bit throws an error
            // no way to check this without Try/Catch :(
            Color[] sourcePixels = null;
            try
            {
                sourcePixels = sr.sprite.texture.GetPixels();
            }
            catch (UnityException e)
            {
                if (e.Message.StartsWith("Texture '" + sr.sprite.texture.name + "' is not readable"))
                {
                    Debug.LogError("Please enable read/write on texture [" + sr.sprite.texture.name + "]");
                }
            }
            for (int j = 0; j < sourcePixels.Length; j++)
            {
                var source = sourcePixels[j];
                var x = j % sourceWidth;
                var y = j / sourceWidth;

                if (rot != 0)
                {
                    tempX = x;
                    tempY = y;
                    switch (rot)
                    {
                        case ROT90.NONE_FLIP:
                            x = -tempX;
                            y = tempY;
                            break;
                        case ROT90.ONE:
                            x = -tempY;
                            y = tempX;
                            break;
                        case ROT90.ONE_FLIP:
                            x = -tempY;
                            y = -tempX;
                            break;
                        case ROT90.TWO:
                            x = -tempX;
                            y = -tempY;
                            break;
                        case ROT90.TWO_FLIP:
                            x = tempX;
                            y = -tempY;
                            break;
                        case ROT90.THREE:
                            x = tempY;
                            y = -tempX;
                            break;
                        case ROT90.THREE_FLIP:
                            x = tempY;
                            y = tempX;
                            break;
                    }
                }

                var index = (x + p.x) + (y + p.y) * targetWidth;
                if (index > 0 && index < targetPixels.Length)
                {
                    var target = targetPixels[index];
                    if (target.a > 0)
                    {
                        // alpha blend when we've already written to the target
                        float sourceAlpha = source.a;
                        float invSourceAlpha = 1f - source.a;
                        float alpha = sourceAlpha + invSourceAlpha * target.a;
                        Color result = (source * sourceAlpha + target * target.a * invSourceAlpha) / alpha;
                        result.a = alpha;
                        source = result;
                    }
                    targetPixels[index] = source;
                }
            }
        }

        targetTexture.SetPixels(targetPixels);
        targetTexture.Apply(false, true);// read/write is disabled in 2nd param to free up memory
        return Sprite.Create(targetTexture, new Rect(new Vector2(), size), new Vector2(), 1, 1, SpriteMeshType.FullRect, new Vector4(2, 2, 2, 2));
    }
}