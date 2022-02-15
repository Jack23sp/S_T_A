using CustomType;
using Mirror;

public class BuildingModularCrafting : ModularObject
{
    [SyncVar]
    public bool isPremium;

    public SyncListCraft craftItem = new SyncListCraft();
    public ScriptableItem building;
}