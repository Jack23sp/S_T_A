// Contains all the network messages that we need.
using System.Collections.Generic;
using System.Linq;
using Mirror;

// client to server ////////////////////////////////////////////////////////////
public partial class LoginMsg : NetworkMessage
{
    public string account;
    public string password;
    public string version;
}

public partial class CharacterSelectMsg : NetworkMessage
{
    public int index;
}

public partial class CharacterDeleteMsg : NetworkMessage
{
    public int index;
}

public partial class CharacterCreateMsg : NetworkMessage
{
    public string name;
    public int classIndex;

    public int sex;
    public string skinColor;
    public string hairColor;
    public int hairType;
    public int eyesType;
    public string eyesColor;
    public int beard;
    public string underwearColor;

    public float fat;
    public float thin;
    public float muscle;
    public float height;
    public float breast;

}

// server to client ////////////////////////////////////////////////////////////
// we need an error msg packet because we can't use TargetRpc with the Network-
// Manager, since it's not a MonoBehaviour.
public partial class ErrorMsg : NetworkMessage
{
    public string text;
    public bool causesDisconnect;
}

public partial class LoginSuccessMsg : NetworkMessage
{
}

public partial class CharactersAvailableMsg : NetworkMessage
{
    public partial struct CharacterPreview
    {
        public string name;
        public string className; // = the prefab name
        public ItemSlot[] equipment;
        public int sex;
        public string skinColor;
        public string hairColor;
        public int hairType;
        public int eyesType;
        public string eyesColor;
        public int beard;
        public int bag;
        public string underwearColor;

        public float fat;
        public float thin;
        public float muscle;
        public float height;
        public float breast;
        public bool premiumZone;

    }
    public CharacterPreview[] characters;

    // load method in this class so we can still modify the characters structs
    // in the addon hooks
    public void Load(List<Player> players)
    {
        // we only need name, class, equipment for our UI
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        characters = new CharacterPreview[players.Count];
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.name,
                className = player.className,
                equipment = player.equipment.ToArray(),
                sex = player.playerCreation.sex,
                hairType = player.playerCreation.hairType,
                beard = player.playerCreation.beard,
                hairColor = player.playerCreation.hairColor,
                underwearColor = player.playerCreation.underwearColor,
                eyesColor = player.playerCreation.eyesColor,
                skinColor = player.playerCreation.skinColor,
                fat = player.playerCreation.fat,
                thin = player.playerCreation.thin,
                muscle = player.playerCreation.muscle,
                height = player.playerCreation.height,
                breast = player.playerCreation.breast,
                bag = player.playerCreation.bag,
                premiumZone = player.GetComponent<NotOnlinePlayerPremiumManager>().inPremiumZone
            };
        }

        // addon system hooks (to initialize extra values like health if necessary)
        Utils.InvokeMany(typeof(CharactersAvailableMsg), this, "Load_", players);
    }
}