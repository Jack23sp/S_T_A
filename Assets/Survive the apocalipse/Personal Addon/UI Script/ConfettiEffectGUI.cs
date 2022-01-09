using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiEffectGUI : MonoBehaviour
{
    //Total number of confetti
    public int confettiCount = 20;

    //The canvas of the generated confetti
    public Transform canvasGUI;

    //confetti Template prefab
    public GameObject confettiTemplate;

    //Reference object coordinates for generating confetti
    public Transform referenceObject;

    //Is confetti generated after mouse click
    public bool isMouseClick = false;

    //The gravity of falling confetti
    public float gravityConfetti = 0.3f;

    //Value of dragging confetti horizontally
    public float dragConfetti = 0.075f;

    //Minimum terminal velocity
    public float terminalVelocity = 3f;

    //Generated pieces list
    List<ConfettiPiece> pieces = new List<ConfettiPiece>();

    //The pieces to be deleted at the next frame
    List<ConfettiPiece> removePieces = new List<ConfettiPiece>();

    public Vector3 positionSpawn;

    public Transform positionToSpawn;


    //Random pieces color
    public ConfettiColor[] colors = new ConfettiColor[]
    {
        new ConfettiColor(new Color(0.48f, 0.36f,1f),new Color(0.38f,0.27f,0.87f)),
        new ConfettiColor(new Color(0.7f,0.78f,1f),new Color(0.56f,0.64f,0.89f)),
        new ConfettiColor(new Color(0.36f,0.52f,1f),new Color(0.2f,0.36f,0.81f))
    };

    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 60;
        canvasGUI = GeneralManager.singleton.canvas;
        positionSpawn = positionToSpawn.transform.position;

        StartConfetti(positionSpawn);

        removePieces.Clear();

        foreach (ConfettiPiece piece in pieces)
        {
            //If the position of the piece is outside the screen range
            //Put the piece in the delete list
            if (piece.transform.position.y < 0)
            {
                removePieces.Add(piece);
            }
        }

        //Deletes the piece object in the delete list
        foreach (ConfettiPiece piece in removePieces)
        {
            Destroy(piece.gameObject);
            pieces.Remove(piece);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Generate pieces at reference position
    /// </summary>
    public void StartConfetti()
    {
        //Generate pieces at reference position
        StartConfetti(referenceObject.position);
    }

    /// <summary>
    /// Generate pieces at position
    /// </summary>
    /// <param name="pos"></param>
    void StartConfetti(Vector3 pos)
    {

        for (int i = 0; i < confettiCount; i++)
        {
            //Instantiate confetti piece
            GameObject go = Instantiate(confettiTemplate);

            go.transform.SetParent(positionToSpawn);

            ConfettiPiece piece = go.GetComponent<ConfettiPiece>();
            if (piece != null)
            {
                //Initialization piece and set random color.
                piece.Setup(this, colors[Random.Range(0, colors.Length - 1)], pos);
                //Add piece to pieces list.
                pieces.Add(piece);
            }

        }
    }
}
