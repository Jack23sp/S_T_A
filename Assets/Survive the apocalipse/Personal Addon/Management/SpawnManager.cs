using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public struct CustomSpawnObject
{
    public int health;
    public Vector3 position;
    public Entity entity;
    public int resourceType;
}

public class SpawnManager : NetworkBehaviour
{
    public List<GameObject> Trees = new List<GameObject>();
    public List<GameObject> Rocks = new List<GameObject>();
    public List<GameObject> Monster = new List<GameObject>();
    public List<GameObject> Bosses = new List<GameObject>();


    [Header("Total")]
    public int treeCount;
    public int rockCount;
    public int monsterCount;
    public int bossCount;

    [Header("Actual")]
    public List<CustomSpawnObject> actualTrees = new List<CustomSpawnObject>();
    public List<CustomSpawnObject> actualRock = new List<CustomSpawnObject>();
    public List<CustomSpawnObject> actualMonster = new List<CustomSpawnObject>();
    public List<CustomSpawnObject> actualBoss = new List<CustomSpawnObject>();


    public float x = 1.7f;
    public float y = 1.9f;

    [HideInInspector] public int spawnedAlready;
    [HideInInspector] public int spawnManagerIndex = 0;
    [HideInInspector] public GameObject instantiatedObject;

    [HideInInspector] public int spawnedIndex = 0;

    public List<Player> playerInside = new List<Player>();

    public GameObject decorationObject;

    public bool forceSpawn;

    public void Start()
    {
        if (isServer)
            InvokeRepeating(nameof(CheckAndSpawnObject), 60.0f, 60.0f);
        if (isServer)
            InvokeRepeating(nameof(ForceSpawn), 60.0f, 60.0f);

        decorationObject = transform.GetChild(0).gameObject;
    }

    [Command]
    public void CmdCheckCreation(string username)
    {
        bool existsPlayer = Database.singleton.CharacterExists(username);
        TargetCheckExist(existsPlayer);
    }

    [TargetRpc]
    public void TargetCheckExist(bool exist)
    {
        UICharacterCreationCustom uICharacterCreationCustom = FindObjectOfType<UICharacterCreationCustom>();
        uICharacterCreationCustom.existsPlayer = exist;
    }



    public override void OnStartServer()
    {

    }

    public void InstantiateResourceObjectOnEnter(GameObject SpawnItem, Transform fatherTransform, int type, int resourceType, int addToCustomSpawnObject)
    {
        if (!isServer)
            return;

        if (type == 0)
        {
            if (addToCustomSpawnObject == -1)
            {
                float randomPosX = Random.Range(fatherTransform.position.x - fatherTransform.localScale.x / x, fatherTransform.position.x + fatherTransform.localScale.x / x);
                float randomPosY = Random.Range(fatherTransform.position.y - fatherTransform.localScale.y / y, fatherTransform.position.y + fatherTransform.localScale.y / y);
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
            }
            else
            {
                float randomPosX = actualTrees[addToCustomSpawnObject].position.x;
                float randomPosY = actualTrees[addToCustomSpawnObject].position.y;
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
                instantiatedObject.GetComponent<Tree>()._health = actualTrees[addToCustomSpawnObject].health;
            }
            LayerColliderManager layerColliderManager = instantiatedObject.GetComponent<LayerColliderManager>();
            Entity entity = instantiatedObject.GetComponent<Entity>();
            if (entity) entity.spawnManager = spawnManagerIndex;

            instantiatedObject.gameObject.layer = 13;

            if (layerColliderManager)
            {
                layerColliderManager.CheckObstacle();
            }

            if (layerColliderManager.colliders.Length == 0)
            {
                if (addToCustomSpawnObject == -1)
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject.entity = entity;
                    spawnObject.position = entity.transform.position;
                    spawnObject.health = entity.healthMax;
                    spawnObject.resourceType = resourceType;
                    if (!actualTrees.Contains(spawnObject)) actualTrees.Add(spawnObject);
                    NetworkServer.Spawn(instantiatedObject);
                }
                else
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject = actualTrees[addToCustomSpawnObject];
                    spawnObject.health = entity._health;
                    spawnObject.entity = entity;
                    spawnObject.position = actualTrees[addToCustomSpawnObject].position;
                    actualTrees[addToCustomSpawnObject] = spawnObject;

                    NetworkServer.Spawn(instantiatedObject);
                    Entity entit = actualTrees[addToCustomSpawnObject].entity;
                    entit.health = actualTrees[addToCustomSpawnObject].health;
                }
            }
            else
            {
                Destroy(instantiatedObject);
            }
        }
        else if (type == 1)
        {
            if (addToCustomSpawnObject == -1)
            {
                float randomPosX = Random.Range(fatherTransform.position.x - fatherTransform.localScale.x / x, fatherTransform.position.x + fatherTransform.localScale.x / x);
                float randomPosY = Random.Range(fatherTransform.position.y - fatherTransform.localScale.y / y, fatherTransform.position.y + fatherTransform.localScale.y / y);
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
            }
            else
            {
                float randomPosX = actualRock[addToCustomSpawnObject].position.x;
                float randomPosY = actualRock[addToCustomSpawnObject].position.y;
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
                instantiatedObject.GetComponent<Rock>()._health = actualRock[addToCustomSpawnObject].health;
            }
            LayerColliderManager layerColliderManager = instantiatedObject.GetComponent<LayerColliderManager>();
            Entity entity = instantiatedObject.GetComponent<Entity>();
            if (entity) entity.spawnManager = spawnManagerIndex;

            instantiatedObject.gameObject.layer = 13;

            if (layerColliderManager)
            {
                layerColliderManager.CheckObstacle();
            }

            if (layerColliderManager.colliders.Length == 0)
            {
                if (addToCustomSpawnObject == -1)
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject.entity = entity;
                    spawnObject.position = entity.transform.position;
                    spawnObject.health = entity.healthMax;
                    spawnObject.resourceType = resourceType;
                    if (!actualRock.Contains(spawnObject)) actualRock.Add(spawnObject);
                    NetworkServer.Spawn(instantiatedObject);
                }
                else
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject = actualRock[addToCustomSpawnObject];
                    spawnObject.health = entity._health;
                    spawnObject.entity = entity;
                    spawnObject.position = actualRock[addToCustomSpawnObject].position;
                    actualRock[addToCustomSpawnObject] = spawnObject;

                    NetworkServer.Spawn(instantiatedObject);
                    Entity entit = actualRock[addToCustomSpawnObject].entity;
                    entit.health = actualRock[addToCustomSpawnObject].health;
                }
            }
            else
            {
                Destroy(instantiatedObject);
            }
        }
        else if (type == 2)
        {
            if (addToCustomSpawnObject == -1)
            {
                float randomPosX = Random.Range(fatherTransform.position.x - fatherTransform.localScale.x / x, fatherTransform.position.x + fatherTransform.localScale.x / x);
                float randomPosY = Random.Range(fatherTransform.position.y - fatherTransform.localScale.y / y, fatherTransform.position.y + fatherTransform.localScale.y / y);
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
            }
            else
            {
                float randomPosX = actualMonster[addToCustomSpawnObject].position.x;
                float randomPosY = actualMonster[addToCustomSpawnObject].position.y;
                instantiatedObject = Instantiate(SpawnItem, new Vector3(randomPosX, randomPosY), Quaternion.identity);
                instantiatedObject.GetComponent<Monster>()._health = actualMonster[addToCustomSpawnObject].health;
            }
            LayerColliderManager layerColliderManager = instantiatedObject.GetComponent<LayerColliderManager>();
            Entity entity = instantiatedObject.GetComponent<Entity>();
            if (entity) entity.spawnManager = spawnManagerIndex;

            instantiatedObject.gameObject.layer = 10;

            if (layerColliderManager)
            {
                layerColliderManager.CheckObstacle();
            }

            if (layerColliderManager.colliders.Length == 0)
            {
                if (addToCustomSpawnObject == -1)
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject.entity = entity;
                    spawnObject.position = entity.transform.position;
                    spawnObject.health = entity.healthMax;
                    spawnObject.resourceType = resourceType;
                    if (!actualMonster.Contains(spawnObject)) actualMonster.Add(spawnObject);
                    NetworkServer.Spawn(instantiatedObject);
                }
                else
                {
                    CustomSpawnObject spawnObject = new CustomSpawnObject();
                    spawnObject = actualMonster[addToCustomSpawnObject];
                    spawnObject.health = entity._health;
                    spawnObject.entity = entity;
                    spawnObject.position = actualMonster[addToCustomSpawnObject].position;
                    actualMonster[addToCustomSpawnObject] = spawnObject;

                    NetworkServer.Spawn(instantiatedObject);
                    Entity entit = actualMonster[addToCustomSpawnObject].entity;
                    entit.health = actualMonster[addToCustomSpawnObject].health;

                }
            }
            else
            {
                Destroy(instantiatedObject);
            }
        }
    }

    public void CheckAndSpawnObject()
    {
        if (playerInside.Count > 0)
        {
            if (actualMonster.Count > 0 && actualMonster.Count < monsterCount)
            {
                for (int i = actualMonster.Count; i < monsterCount; i++)
                {
                    int index = Random.Range(0, Monster.Count);
                    InstantiateResourceObjectOnEnter(Monster[index], transform, 2, index, -1);
                }
            }
            if (actualRock.Count > 0 && actualRock.Count < rockCount)
            {
                for (int i = actualRock.Count; i < rockCount; i++)
                {
                    int index = Random.Range(0, Rocks.Count);
                    InstantiateResourceObjectOnEnter(Rocks[index], transform, 1, index, -1);
                }
            }
            if (actualTrees.Count > 0 && actualTrees.Count < treeCount)
            {
                for (int i = actualTrees.Count; i < treeCount; i++)
                {
                    int index = Random.Range(0, Trees.Count);
                    InstantiateResourceObjectOnEnter(Trees[index], transform, 0, index, -1);
                }
            }
        }
    }

    public void ForceSpawn()
    {
        if (forceSpawn)
        {
            for (int i = actualMonster.Count; i < monsterCount; i++)
            {
                int index = Random.Range(0, Monster.Count);
                InstantiateResourceObjectOnEnter(Monster[index], transform, 2, index, -1);
            }
            for (int i = actualRock.Count; i < rockCount; i++)
            {
                int index = Random.Range(0, Rocks.Count);
                InstantiateResourceObjectOnEnter(Rocks[index], transform, 1, index, -1);
            }
            for (int i = actualTrees.Count; i < treeCount; i++)
            {
                int index = Random.Range(0, Trees.Count);
                InstantiateResourceObjectOnEnter(Trees[index], transform, 0, index, -1);
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        for (int i = 0; i < playerInside.Count; i++)
        {
            if (playerInside[i] == null)
                playerInside.RemoveAt(i);
        }

        if (!playerInside.Contains(collision.GetComponent<Player>()))
        {
            playerInside.Add(collision.GetComponent<Player>());
            decorationObject.SetActive(true);
        }
    }


    public void OnTriggerExit2D(Collider2D collision)
    {
        decorationObject.SetActive(false);

        if (!isServer) return;

        for (int i = 0; i < playerInside.Count; i++)
        {
            if (playerInside[i] == null)
                playerInside.RemoveAt(i);
        }

        if (playerInside.Contains(collision.GetComponentInParent<Player>()))
        {
            playerInside.Remove(collision.GetComponentInParent<Player>());
        }

        if (collision.gameObject.layer == 14 && playerInside.Count == 0)
        {
            int index = 0;
            for (int i = 0; i < actualRock.Count; i++)
            {
                index = i;
                CustomSpawnObject customSpawnObject = new CustomSpawnObject();
                Rock rock = new Rock();
                rock = ((Rock)actualRock[index].entity);
                if (rock)
                {
                    customSpawnObject.position = new Vector3(rock.transform.position.x, rock.transform.position.y, rock.transform.position.z);
                    customSpawnObject.health = rock._health;
                    customSpawnObject.entity = rock;
                    actualRock[index] = customSpawnObject;
                    NetworkServer.Destroy(actualRock[index].entity.gameObject);
                }
            }
            for (int i = 0; i < actualTrees.Count; i++)
            {
                index = i;
                CustomSpawnObject customSpawnObject = new CustomSpawnObject();
                Tree tree = new Tree();
                tree = ((Tree)actualTrees[index].entity);
                if (tree)
                {
                    customSpawnObject.position = new Vector3(tree.transform.position.x, tree.transform.position.y, tree.transform.position.z);
                    customSpawnObject.health = tree._health;
                    customSpawnObject.entity = tree;
                    actualTrees[index] = customSpawnObject;
                    NetworkServer.Destroy(actualTrees[index].entity.gameObject);
                }
            }
            for (int i = 0; i < actualMonster.Count; i++)
            {
                index = i;
                CustomSpawnObject customSpawnObject = new CustomSpawnObject();
                Monster monster = new Monster();
                monster = ((Monster)actualMonster[index].entity);
                if (monster)
                {
                    customSpawnObject.position = new Vector3(monster.transform.position.x, monster.transform.position.y, monster.transform.position.z);
                    customSpawnObject.health = monster._health;
                    customSpawnObject.entity = monster;
                    actualMonster[index] = customSpawnObject;
                    NetworkServer.Destroy(actualMonster[index].entity.gameObject);
                }
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        decorationObject.SetActive(true);

        if (!isServer) return;

        for (int i = 0; i < playerInside.Count; i++)
        {
            if (playerInside[i] == null)
                playerInside.RemoveAt(i);
        }

        if (!playerInside.Contains(collision.GetComponentInParent<Player>()))
        {
            playerInside.Add(collision.GetComponentInParent<Player>());
        }

        if (collision.gameObject.layer == 14 && playerInside.Count == 1)
        {
            if (actualTrees.Count == 0)
            {
                for (int i = 0; i < treeCount; i++)
                {
                    int index = Random.Range(0, Trees.Count);
                    InstantiateResourceObjectOnEnter(Trees[index], transform, 0, index, -1);
                }
            }
            else
            {
                for (int i = 0; i < actualTrees.Count; i++)
                {
                    int index = i;
                    InstantiateResourceObjectOnEnter(Trees[actualTrees[index].resourceType], transform, 0, i, i);
                }
            }

            if (actualRock.Count == 0)
            {
                for (int i = 0; i < rockCount; i++)
                {
                    int index = Random.Range(0, Rocks.Count);
                    InstantiateResourceObjectOnEnter(Rocks[index], transform, 1, index, -1);
                }
            }
            else
            {
                for (int i = 0; i < actualRock.Count; i++)
                {
                    int index = i;
                    InstantiateResourceObjectOnEnter(Rocks[actualRock[index].resourceType], transform, 1, i, i);
                }
            }

            if (actualMonster.Count == 0)
            {
                for (int i = 0; i < monsterCount; i++)
                {
                    int index = Random.Range(0, Monster.Count);
                    InstantiateResourceObjectOnEnter(Monster[index], transform, 2, index, -1);
                }
            }
            else
            {
                for (int i = 0; i < actualMonster.Count; i++)
                {
                    int index = i;
                    InstantiateResourceObjectOnEnter(Monster[actualMonster[index].resourceType], transform, 2, i, i);
                }
            }
        }
    }

}
