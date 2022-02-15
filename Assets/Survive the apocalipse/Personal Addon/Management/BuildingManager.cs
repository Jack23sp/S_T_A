using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager singleton;
    public List<Building> buildings = new List<Building>();//
    public List<BeeKeeper> beeKeeper = new List<BeeKeeper>();//
    public List<Breeding> breedings = new List<Breeding>();
    public List<BuildingCraft> buildingCrafts = new List<BuildingCraft>();//
    public List<Campfire> campfires = new List<Campfire>();//
    public List<CultivableField> cultivableFields = new List<CultivableField>();
    public List<Dynamite> dynamites = new List<Dynamite>();//
    public List<GasStation> gasStations = new List<GasStation>();//
    public List<Warehouse> personalWarehouses = new List<Warehouse>();//
    public List<Warehouse> groupWarehouses = new List<Warehouse>();//
    public List<Mine> mines = new List<Mine>();//
    public List<WoodWall> woodWalls = new List<WoodWall>();//
    public List<Barbwire> barbWires = new List<Barbwire>();//
    public List<Tesla> teslas = new List<Tesla>();//
    public List<Totem> totems = new List<Totem>();//
    public List<BuildingUpgradeRepair> upgradeRepair = new List<BuildingUpgradeRepair>();//
    public List<BuildingWaterWell> waterWells = new List<BuildingWaterWell>();//
    public List<PetTrainer> petTrainers = new List<PetTrainer>();
    public List<WorldHouse> worldHouses = new List<WorldHouse>();
    public List<StreetLamp> streetLamps = new List<StreetLamp>();
    public List<Flag> flags = new List<Flag>();
    public List<Gate> gates = new List<Gate>();
    public List<ModularPiece> modularPieces = new List<ModularPiece>();
    public List<ModularObject> modularObjects = new List<ModularObject>();


    public GameObject beeKeeperObject;
    public GameObject breedingsObject;
    public List<GameObject> buildingCraftsObject = new List<GameObject>();
    public GameObject campfiresObject;
    public GameObject cultivableFieldsObject;
    public GameObject dynamitesObject;
    public GameObject gasStationsObject;
    public GameObject personalWarehousesObject;
    public GameObject groupWarehousesObject;
    public GameObject minesObject;
    public List<GameObject> woodWallsObject = new List<GameObject>();
    public List<GameObject> barbwiresObject = new List<GameObject>();
    public GameObject teslasObject;
    public GameObject totemsObject;
    public GameObject upgradeRepairObject;
    public GameObject waterWellsObject;
    public GameObject petTrainersObject;
    public GameObject streetLampsObject;
    public GameObject flagObject;
    public GameObject gateObject;
    public GameObject modularPiece;

    public float saveInterval = 50.0f;

    public bool isLoaded;


    void Start()
    {
        if (!singleton) singleton = this;
        if (isServer)
        {
            InvokeRepeating("SaveBuilding", saveInterval, saveInterval);
        }
    }

    public override void OnStartServer()
    {
        if (isServer)
        {
            Database.singleton.LoadBuilding(SceneManager.GetActiveScene().name);
        }
    }

    void Update()
    {
    }

    public void SaveBuilding()
    {
        Database.singleton.SaveBuilding(SceneManager.GetActiveScene().name);
    }

    public void AddToList(GameObject building)
    {
        if (building.GetComponent<Building>())
        {
            if (!buildings.Contains(building.GetComponent<Building>()))
            {
                buildings.Add(building.GetComponent<Building>());
            }
        }

        if (building.GetComponent<BeeKeeper>())
        {
            if (!beeKeeper.Contains(building.GetComponent<BeeKeeper>()))
            {
                beeKeeper.Add(building.GetComponent<BeeKeeper>());
            }
        }
        if (building.GetComponent<Breeding>())
        {
            if (!breedings.Contains(building.GetComponent<Breeding>()))
            {
                breedings.Add(building.GetComponent<Breeding>());
            }
        }
        if (building.GetComponent<BuildingCraft>())
        {
            if (!buildingCrafts.Contains(building.GetComponent<BuildingCraft>()))
            {
                buildingCrafts.Add(building.GetComponent<BuildingCraft>());
            }
        }
        if (building.GetComponent<Campfire>())
        {
            if (!campfires.Contains(building.GetComponent<Campfire>()))
            {
                campfires.Add(building.GetComponent<Campfire>());
            }
        }
        if (building.GetComponent<CultivableField>())
        {
            if (!cultivableFields.Contains(building.GetComponent<CultivableField>()))
            {
                cultivableFields.Add(building.GetComponent<CultivableField>());
            }
        }
        if (building.GetComponent<Dynamite>())
        {
            if (!dynamites.Contains(building.GetComponent<Dynamite>()))
            {
                dynamites.Add(building.GetComponent<Dynamite>());
            }
        }
        if (building.GetComponent<GasStation>())
        {
            if (!gasStations.Contains(building.GetComponent<GasStation>()))
            {
                gasStations.Add(building.GetComponent<GasStation>());
            }
        }
        if (building.GetComponent<Warehouse>())
        {
            if (!personalWarehouses.Contains(building.GetComponent<Warehouse>()) && building.GetComponent<Warehouse>().personal)
            {
                personalWarehouses.Add(building.GetComponent<Warehouse>());
            }
        }
        if (building.GetComponent<Warehouse>())
        {
            if (!groupWarehouses.Contains(building.GetComponent<Warehouse>()) && !building.GetComponent<Warehouse>().personal)
            {
                groupWarehouses.Add(building.GetComponent<Warehouse>());
            }
        }
        if (building.GetComponent<Mine>())
        {
            if (!mines.Contains(building.GetComponent<Mine>()))
            {
                mines.Add(building.GetComponent<Mine>());
            }
        }
        if (building.GetComponent<WoodWall>())
        {
            if (!woodWalls.Contains(building.GetComponent<WoodWall>()))
            {
                woodWalls.Add(building.GetComponent<WoodWall>());
            }
        }
        if (building.GetComponent<Barbwire>())
        {
            if (!barbWires.Contains(building.GetComponent<Barbwire>()))
            {
                barbWires.Add(building.GetComponent<Barbwire>());
            }
        }
        if (building.GetComponent<Tesla>())
        {
            if (!teslas.Contains(building.GetComponent<Tesla>()))
            {
                teslas.Add(building.GetComponent<Tesla>());
            }
        }
        if (building.GetComponent<Totem>())
        {
            if (!totems.Contains(building.GetComponent<Totem>()))
            {
                totems.Add(building.GetComponent<Totem>());
            }
        }
        if (building.GetComponent<BuildingUpgradeRepair>())
        {
            if (!upgradeRepair.Contains(building.GetComponent<BuildingUpgradeRepair>()))
            {
                upgradeRepair.Add(building.GetComponent<BuildingUpgradeRepair>());
            }
        }
        if (building.GetComponent<BuildingWaterWell>())
        {
            if (!waterWells.Contains(building.GetComponent<BuildingWaterWell>()))
            {
                waterWells.Add(building.GetComponent<BuildingWaterWell>());
            }
        }
        if (building.GetComponent<PetTrainer>())
        {
            if (!petTrainers.Contains(building.GetComponent<PetTrainer>()))
            {
                petTrainers.Add(building.GetComponent<PetTrainer>());
            }
        }
        if (building.GetComponent<StreetLamp>())
        {
            if (!streetLamps.Contains(building.GetComponent<StreetLamp>()))
            {
                streetLamps.Add(building.GetComponent<StreetLamp>());
            }
        }
        if (building.GetComponent<Flag>())
        {
            if (!flags.Contains(building.GetComponent<Flag>()))
            {
                flags.Add(building.GetComponent<Flag>());
            }
        }
        if (building.GetComponent<Gate>())
        {
            if (!gates.Contains(building.GetComponent<Gate>()))
            {
                gates.Add(building.GetComponent<Gate>());
            }
        }
        if (building.GetComponent<ModularPiece>())
        {
            if (!modularPieces.Contains(building.GetComponent<ModularPiece>()))
            {
                modularPieces.Add(building.GetComponent<ModularPiece>());
            }
        }
        if (building.GetComponent<ModularObject>())
        {
            if (!modularObjects.Contains(building.GetComponent<ModularObject>()))
            {
                modularObjects.Add(building.GetComponent<ModularObject>());
            }
        }
    }

    public void RemoveFromList(GameObject building)
    {
        if (building.GetComponent<Building>())
        {
            if (buildings.Contains(building.GetComponent<Building>()))
            {
                buildings.Remove(building.GetComponent<Building>());
            }
        }

        if (building.GetComponent<BeeKeeper>())
        {
            if (beeKeeper.Contains(building.GetComponent<BeeKeeper>()))
            {
                beeKeeper.Remove(building.GetComponent<BeeKeeper>());
            }
        }
        if (building.GetComponent<Breeding>())
        {
            if (breedings.Contains(building.GetComponent<Breeding>()))
            {
                breedings.Remove(building.GetComponent<Breeding>());
            }
        }
        if (building.GetComponent<BuildingCraft>())
        {
            if (buildingCrafts.Contains(building.GetComponent<BuildingCraft>()))
            {
                buildingCrafts.Remove(building.GetComponent<BuildingCraft>());
            }
        }
        if (building.GetComponent<Campfire>())
        {
            if (campfires.Contains(building.GetComponent<Campfire>()))
            {
                campfires.Remove(building.GetComponent<Campfire>());
            }
        }
        if (building.GetComponent<CultivableField>())
        {
            if (cultivableFields.Contains(building.GetComponent<CultivableField>()))
            {
                cultivableFields.Remove(building.GetComponent<CultivableField>());
            }
        }
        if (building.GetComponent<Dynamite>())
        {
            if (dynamites.Contains(building.GetComponent<Dynamite>()))
            {
                dynamites.Remove(building.GetComponent<Dynamite>());
            }
        }
        if (building.GetComponent<GasStation>())
        {
            if (gasStations.Contains(building.GetComponent<GasStation>()))
            {
                gasStations.Remove(building.GetComponent<GasStation>());
            }
        }
        if (building.GetComponent<Warehouse>())
        {
            if (personalWarehouses.Contains(building.GetComponent<Warehouse>()) && building.GetComponent<Warehouse>().personal)
            {
                personalWarehouses.Remove(building.GetComponent<Warehouse>());
            }
        }
        if (building.GetComponent<Warehouse>())
        {
            if (groupWarehouses.Contains(building.GetComponent<Warehouse>()) && !building.GetComponent<Warehouse>().personal)
            {
                groupWarehouses.Remove(building.GetComponent<Warehouse>());
            }
        }
        if (building.GetComponent<Mine>())
        {
            if (mines.Contains(building.GetComponent<Mine>()))
            {
                mines.Remove(building.GetComponent<Mine>());
            }
        }
        if (building.GetComponent<WoodWall>())
        {
            if (woodWalls.Contains(building.GetComponent<WoodWall>()))
            {
                woodWalls.Remove(building.GetComponent<WoodWall>());
            }
        }
        if (building.GetComponent<Barbwire>())
        {
            if (barbWires.Contains(building.GetComponent<Barbwire>()))
            {
                barbWires.Remove(building.GetComponent<Barbwire>());
            }
        }
        if (building.GetComponent<Tesla>())
        {
            if (teslas.Contains(building.GetComponent<Tesla>()))
            {
                teslas.Remove(building.GetComponent<Tesla>());
            }
        }
        if (building.GetComponent<Totem>())
        {
            if (totems.Contains(building.GetComponent<Totem>()))
            {
                totems.Remove(building.GetComponent<Totem>());
            }
        }
        if (building.GetComponent<BuildingUpgradeRepair>())
        {
            if (upgradeRepair.Contains(building.GetComponent<BuildingUpgradeRepair>()))
            {
                upgradeRepair.Remove(building.GetComponent<BuildingUpgradeRepair>());
            }
        }
        if (building.GetComponent<BuildingWaterWell>())
        {
            if (waterWells.Contains(building.GetComponent<BuildingWaterWell>()))
            {
                waterWells.Remove(building.GetComponent<BuildingWaterWell>());
            }
        }
        if (building.GetComponent<PetTrainer>())
        {
            if (petTrainers.Contains(building.GetComponent<PetTrainer>()))
            {
                petTrainers.Remove(building.GetComponent<PetTrainer>());
            }
        }
        if (building.GetComponent<StreetLamp>())
        {
            if (streetLamps.Contains(building.GetComponent<StreetLamp>()))
            {
                streetLamps.Remove(building.GetComponent<StreetLamp>());
            }
        }
        if (building.GetComponent<Flag>())
        {
            if (flags.Contains(building.GetComponent<Flag>()))
            {
                flags.Remove(building.GetComponent<Flag>());
            }
        }
        if (building.GetComponent<Gate>())
        {
            if (gates.Contains(building.GetComponent<Gate>()))
            {
                gates.Remove(building.GetComponent<Gate>());
            }
        }

        if (building.GetComponent<ModularPiece>())
        {
            if (modularPieces.Contains(building.GetComponent<ModularPiece>()))
            {
                modularPieces.Remove(building.GetComponent<ModularPiece>());
            }
        }

        if (building.GetComponent<ModularObject>())
        {
            if (modularObjects.Contains(building.GetComponent<ModularObject>()))
            {
                modularObjects.Remove(building.GetComponent<ModularObject>());
            }
        }
    }


    public void RemoveToList()
    {
        buildings = buildings.Where(item => item != null).ToList();

        beeKeeper = beeKeeper.Where(item => item != null).ToList();

        breedings = breedings.Where(item => item != null).ToList();

        buildingCrafts = buildingCrafts.Where(item => item != null).ToList();

        campfires = campfires.Where(item => item != null).ToList();

        cultivableFields = cultivableFields.Where(item => item != null).ToList();

        dynamites = dynamites.Where(item => item != null).ToList();

        gasStations = gasStations.Where(item => item != null).ToList();

        personalWarehouses = personalWarehouses.Where(item => item != null).ToList();

        groupWarehouses = groupWarehouses.Where(item => item != null).ToList();

        mines = mines.Where(item => item != null).ToList();

        woodWalls = woodWalls.Where(item => item != null).ToList();

        barbWires = barbWires.Where(item => item != null).ToList();

        teslas = teslas.Where(item => item != null).ToList();

        totems = totems.Where(item => item != null).ToList();

        upgradeRepair = upgradeRepair.Where(item => item != null).ToList();

        waterWells = waterWells.Where(item => item != null).ToList();

        petTrainers = petTrainers.Where(item => item != null).ToList();

        streetLamps = streetLamps.Where(item => item != null).ToList();

        flags = flags.Where(item => item != null).ToList();

        gates = gates.Where(item => item != null).ToList();

        modularPieces = modularPieces.Where(item => item != null).ToList();

        modularObjects = modularObjects.Where(item => item != null).ToList();

    }
}
