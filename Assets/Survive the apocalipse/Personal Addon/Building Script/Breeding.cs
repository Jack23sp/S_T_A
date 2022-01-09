using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Breeding : NetworkBehaviour
{
    //[SyncVar]
    public int water;
    //[SyncVar]
    public int food;
    public SyncListAnimal cow = new SyncListAnimal();
    public SyncListAnimal horse = new SyncListAnimal();
    public SyncListAnimal goat = new SyncListAnimal();
    public SyncListAnimal chicken = new SyncListAnimal();
    public SyncListAnimal sheep = new SyncListAnimal();
    public LinearInt animalEachLevel;
    public LinearInt maxWater;
    public LinearInt maxFood;
    [HideInInspector] public int currentMaxWater;
    [HideInInspector] public int currentMaxFood;
    public int sum;


    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            InvokeRepeating("GrowYear", GeneralManager.singleton.intervalYear, GeneralManager.singleton.intervalYear);
            //InvokeRepeating("CheckTakeProduct", GeneralManager.singleton.intervalTake, GeneralManager.singleton.intervalTake);
            InvokeRepeating("RemoveAutomaticallyAfterDeath", GeneralManager.singleton.intervalCheckAfterDeath, GeneralManager.singleton.intervalCheckAfterDeath);
            InvokeRepeating("CheckNewBorn", GeneralManager.singleton.intervalDistanceBetweenSex, GeneralManager.singleton.intervalDistanceBetweenSex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        sum = cow.Count + horse.Count + goat.Count + chicken.Count + sheep.Count;
        //currentMaxWater = maxWater.Get(GetComponent<Building>().level);
        //currentMaxFood = maxFood.Get(GetComponent<Building>().level);
    }

    [Server]
    public void GrowYear()
    {
        foreach (Animal anim in cow)
        {
            if (anim.age < GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                anim.age++;
                if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
                {
                    anim.health = 0;
                }
            }
        }
        foreach (Animal anim in horse)
        {
            if (anim.age < GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                anim.age++;
                if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
                {
                    anim.health = 0;
                }
            }
        }
        foreach (Animal anim in goat)
        {
            if (anim.age < GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                anim.age++;
                if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
                {
                    anim.health = 0;
                }
            }
        }
        foreach (Animal anim in chicken)
        {
            if (anim.age < GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                anim.age++;
                if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
                {
                    anim.health = 0;
                }
            }
        }
        foreach (Animal anim in sheep)
        {
            if (anim.age < GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                anim.age++;
                if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
                {
                    anim.health = 0;
                }
            }
        }
    }
    [Server]
    public void ConsumeWater()
    {
        foreach (Animal anim in cow)
        {
            if (anim.health > 0 && water >= anim.amountWaterNeeded)
            {
                water -= anim.amountWaterNeeded;
                if (water < 0) water = 0;
            }
            else if (water < anim.amountWaterNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in horse)
        {
            if (anim.health > 0 && water >= anim.amountWaterNeeded)
            {
                water -= anim.amountWaterNeeded;
                if (water < 0) water = 0;
            }
            else if (water < anim.amountWaterNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in goat)
        {
            if (anim.health > 0 && water >= anim.amountWaterNeeded)
            {
                water -= anim.amountWaterNeeded;
                if (water < 0) water = 0;
            }
            else if (water < anim.amountWaterNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in chicken)
        {
            if (anim.health > 0 && water >= anim.amountWaterNeeded)
            {
                water -= anim.amountWaterNeeded;
                if (water < 0) water = 0;
            }
            else if (water < anim.amountWaterNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in sheep)
        {
            if (anim.health > 0 && water >= anim.amountWaterNeeded)
            {
                water -= anim.amountWaterNeeded;
                if (water < 0) water = 0;
            }
            else if (water < anim.amountWaterNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
    }
    [Server]
    public void ConsumeFood()
    {
        foreach (Animal anim in cow)
        {
            if (anim.health > 0 && food >= anim.amountFoodNeeded)
            {
                food -= anim.amountFoodNeeded;
                if (food < 0) food = 0;
            }
            else if (food < anim.amountFoodNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in horse)
        {
            if (anim.health > 0 && food >= anim.amountFoodNeeded)
            {
                food -= anim.amountFoodNeeded;
                if (water < 0) food = 0;
            }
            else if (food < anim.amountFoodNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in goat)
        {
            if (anim.health > 0 && food >= anim.amountFoodNeeded)
            {
                food -= anim.amountFoodNeeded;
                if (water < 0) food = 0;
            }
            else if (food < anim.amountFoodNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in chicken)
        {
            if (anim.health > 0 && food >= anim.amountFoodNeeded)
            {
                food -= anim.amountFoodNeeded;
                if (food < 0) food = 0;
            }
            else if (food < anim.amountFoodNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
        foreach (Animal anim in sheep)
        {
            if (anim.health > 0 && food >= anim.amountFoodNeeded)
            {
                food -= anim.amountFoodNeeded;
                if (food < 0) food = 0;
            }
            else if (food < anim.amountFoodNeeded)
            {
                anim.health -= 5;
                if (anim.health < 0) anim.health = 0;
            }
        }
    }
    [Server]
    public void PregnantGrown()
    {
        List<Animal> temp = new List<Animal>();
        foreach (Animal anim in cow)
        {
            if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn > 0 && anim.lastBorn < anim.distanceBeetweenBorn)
            {
                anim.lastBorn++;
            }
            else if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn == anim.distanceBeetweenBorn)
            {
                Animal child = new Animal();
                int random = UnityEngine.Random.Range(0, 2);
                if (random == 1)
                {
                    child.name = anim.otherSexName;
                    child.sex = AnimalSex.male.ToString();
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = 0;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.otherSexName).maxAge;
                    child.animalCategory = GeneralManager.singleton.FindAnimal(anim.otherSexName).animalCategory.ToString().ToUpper();
                    child.timeToTake = GeneralManager.singleton.FindAnimal(anim.otherSexName).timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(anim.otherSexName).distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountWaterNeeded;
                    child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountFoodNeeded;
                    child.health = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                    child.maxHealth = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                }
                if (random == 0)
                {
                    child.name = anim.name;
                    child.sex = anim.sex;
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.age = 0;
                    child.maxAge = anim.maxAge;
                    child.animalCategory = anim.animalCategory;
                    child.timeToTake = anim.timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = anim.amountWaterNeeded;
                    child.amountFoodNeeded = anim.amountFoodNeeded;
                    child.health = anim.health;
                    child.maxHealth = anim.health;
                }
                anim.lastBorn = 0;
                temp.Add(child);
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                cow.Add(tAnimal);
        }
        temp.Clear();

        foreach (Animal anim in horse)
        {
            if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn > 0 && anim.lastBorn < anim.distanceBeetweenBorn)
            {
                anim.lastBorn++;
            }
            else if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn == anim.distanceBeetweenBorn)
            {
                Animal child = new Animal();
                int random = UnityEngine.Random.Range(0, 2);

                if (random == 1)
                {
                    child.name = anim.otherSexName;
                    child.sex = GeneralManager.singleton.FindAnimal(anim.otherSexName).sex.ToString().ToUpper();
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = 0;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.otherSexName).maxAge;
                    child.animalCategory = GeneralManager.singleton.FindAnimal(anim.otherSexName).animalCategory.ToString().ToUpper();
                    child.timeToTake = GeneralManager.singleton.FindAnimal(anim.otherSexName).timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(anim.otherSexName).distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountWaterNeeded;
                    child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountFoodNeeded;
                    child.health = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                    child.maxHealth = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                }
                if (random == 0)
                {
                    child.name = anim.name;
                    child.sex = anim.sex;
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.name).maxAge;
                    child.animalCategory = anim.animalCategory;
                    child.timeToTake = anim.timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = anim.amountWaterNeeded;
                    child.amountFoodNeeded = anim.amountFoodNeeded;
                    child.health = anim.health;
                    child.maxHealth = anim.health;
                }
                anim.lastBorn = 0;
                temp.Add(child);
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                horse.Add(tAnimal);
        }
        temp.Clear();

        foreach (Animal anim in goat)
        {
            if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn > 0 && anim.lastBorn < anim.distanceBeetweenBorn)
            {
                anim.lastBorn++;
            }
            else if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn == anim.distanceBeetweenBorn)
            {
                Animal child = new Animal();
                int random = UnityEngine.Random.Range(0, 2);

                if (random == 1)
                {
                    child.name = anim.otherSexName;
                    child.sex = GeneralManager.singleton.FindAnimal(anim.otherSexName).sex.ToString().ToUpper();
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = 0;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.otherSexName).maxAge;
                    child.animalCategory = GeneralManager.singleton.FindAnimal(anim.otherSexName).animalCategory.ToString().ToUpper();
                    child.timeToTake = GeneralManager.singleton.FindAnimal(anim.otherSexName).timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(anim.otherSexName).distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountWaterNeeded;
                    child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountFoodNeeded;
                    child.health = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                    child.maxHealth = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                }
                if (random == 0)
                {
                    child.name = anim.name;
                    child.sex = anim.sex;
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.name).maxAge;
                    child.animalCategory = anim.animalCategory;
                    child.timeToTake = anim.timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = anim.amountWaterNeeded;
                    child.amountFoodNeeded = anim.amountFoodNeeded;
                    child.health = anim.health;
                    child.maxHealth = anim.health;
                }
                anim.lastBorn = 0;
                temp.Add(child);
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                goat.Add(tAnimal);
        }
        temp.Clear();

        foreach (Animal anim in chicken)
        {
            if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn > 0 && anim.lastBorn < anim.distanceBeetweenBorn)
            {
                anim.lastBorn++;
            }
            else if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn == anim.distanceBeetweenBorn)
            {
                Animal child = new Animal();
                int random = UnityEngine.Random.Range(0, 2);

                if (random == 1)
                {
                    child.name = anim.otherSexName;
                    child.sex = GeneralManager.singleton.FindAnimal(anim.otherSexName).sex.ToString().ToUpper();
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = 0;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.otherSexName).maxAge;
                    child.animalCategory = GeneralManager.singleton.FindAnimal(anim.otherSexName).animalCategory.ToString().ToUpper();
                    child.timeToTake = GeneralManager.singleton.FindAnimal(anim.otherSexName).timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(anim.otherSexName).distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountWaterNeeded;
                    child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountFoodNeeded;
                    child.health = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                    child.maxHealth = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                }
                if (random == 0)
                {
                    child.name = anim.name;
                    child.sex = anim.sex;
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.name).maxAge;
                    child.animalCategory = anim.animalCategory;
                    child.timeToTake = anim.timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = anim.amountWaterNeeded;
                    child.amountFoodNeeded = anim.amountFoodNeeded;
                    child.health = anim.health;
                    child.maxHealth = anim.health;
                }
                anim.lastBorn = 0;
                temp.Add(child);
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                chicken.Add(tAnimal);
        }
        temp.Clear();

        foreach (Animal anim in sheep)
        {
            if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn > 0 && anim.lastBorn < anim.distanceBeetweenBorn)
            {
                anim.lastBorn++;
            }
            else if (anim.health > 0 && anim.sex == AnimalSex.female.ToString() && anim.lastBorn == anim.distanceBeetweenBorn)
            {
                Animal child = new Animal();
                int random = UnityEngine.Random.Range(0, 2);

                if (random == 1)
                {
                    child.name = anim.otherSexName;
                    child.sex = GeneralManager.singleton.FindAnimal(anim.otherSexName).sex.ToString().ToUpper();
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = 0;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.otherSexName).maxAge;
                    child.animalCategory = GeneralManager.singleton.FindAnimal(anim.otherSexName).animalCategory.ToString().ToUpper();
                    child.timeToTake = GeneralManager.singleton.FindAnimal(anim.otherSexName).timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(anim.otherSexName).distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountWaterNeeded;
                    child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(anim.otherSexName).amountFoodNeeded;
                    child.health = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                    child.maxHealth = GeneralManager.singleton.FindAnimal(anim.otherSexName).health;
                }
                if (random == 0)
                {
                    child.name = anim.name;
                    child.sex = anim.sex;
                    child.otherSexName = GeneralManager.singleton.FindAnimal(anim.otherSexName).name;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.age = 0;
                    child.maxAge = GeneralManager.singleton.FindAnimal(anim.name).maxAge;
                    child.animalCategory = anim.animalCategory;
                    child.timeToTake = anim.timeToTake;
                    child.lastTake = 0;
                    child.distanceBeetweenBorn = anim.distanceBeetweenBorn;
                    child.lastBorn = 0;
                    child.amountWaterNeeded = anim.amountWaterNeeded;
                    child.amountFoodNeeded = anim.amountFoodNeeded;
                    child.health = anim.health;
                    child.maxHealth = anim.health;
                }
                anim.lastBorn = 0;
                temp.Add(child);
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                sheep.Add(tAnimal);
        }
        temp.Clear();

    }
    [Server]
    public void DistanceBeetweenSex()
    {
        int sum = cow.Count + horse.Count + goat.Count + chicken.Count + sheep.Count;
        if (animalEachLevel.Get(GetComponent<Building>().level) == sum) return;
        List<Animal> temp = new List<Animal>();

        int maleAnimal = 0;
        #region COW
        foreach (Animal allAnimal in cow)
        {
            if (allAnimal.sex == AnimalSex.female.ToString())
            {
                if (allAnimal.health > 0 && allAnimal.lastBorn == allAnimal.distanceBeetweenBorn)
                {
                    Animal child = new Animal();
                    int random = UnityEngine.Random.Range(0, 11);
                    if (random <= 5)
                    {
                        child.name = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.name;
                        child.otherSexName = allAnimal.sex;
                        child.sex = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.sex.ToString();
                        child.distanceBeetweenBorn = 0;
                        child.age = 0;
                        child.maxAge = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.maxAge;
                        child.animalCategory = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.animalCategory.ToString().ToUpper();
                        child.timeToTake = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountWaterNeeded;
                        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountFoodNeeded;
                        child.health = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                        child.maxHealth = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                    }
                    if (random > 5)
                    {
                        child.name = allAnimal.name;
                        child.sex = allAnimal.sex;
                        child.otherSexName = allAnimal.otherSexName;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.age = 0;
                        child.maxAge = allAnimal.maxAge;
                        child.animalCategory = allAnimal.animalCategory;
                        child.timeToTake = allAnimal.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = allAnimal.amountWaterNeeded;
                        child.amountFoodNeeded = allAnimal.amountFoodNeeded;
                        child.health = allAnimal.health;
                        child.maxHealth = allAnimal.health;
                    }
                    allAnimal.lastBorn = 0;
                    temp.Add(child);
                }
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                cow.Add(tAnimal);
        }
        temp.Clear();
        foreach (Animal allAnimal in cow)
        {
            if (allAnimal.sex == AnimalSex.male.ToString() && allAnimal.age < allAnimal.maxAge)
            {
                maleAnimal++;
            }
        }
        foreach (Animal allAnimal in cow)
        {
            if (allAnimal.sex == AnimalSex.female.ToString() && allAnimal.lastBorn < allAnimal.distanceBeetweenBorn && maleAnimal > 0)
            {
                allAnimal.lastBorn++;
                maleAnimal--;
            }
        }
        maleAnimal = 0;
        #endregion

        #region HORSE
        foreach (Animal allAnimal in horse)
        {
            if (allAnimal.sex == AnimalSex.female.ToString())
            {
                if (allAnimal.health > 0 && allAnimal.lastBorn == allAnimal.distanceBeetweenBorn)
                {
                    Animal child = new Animal();
                    int random = UnityEngine.Random.Range(0, 11);
                    if (random <= 5)
                    {
                        child.name = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.name;
                        child.otherSexName = allAnimal.sex;
                        child.sex = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.sex.ToString();
                        child.distanceBeetweenBorn = 0;
                        child.age = 0;
                        child.maxAge = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.maxAge;
                        child.animalCategory = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.animalCategory.ToString().ToUpper();
                        child.timeToTake = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountWaterNeeded;
                        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountFoodNeeded;
                        child.health = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                        child.maxHealth = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                    }
                    if (random > 5)
                    {
                        child.name = allAnimal.name;
                        child.sex = allAnimal.sex;
                        child.otherSexName = allAnimal.otherSexName;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.age = 0;
                        child.maxAge = allAnimal.maxAge;
                        child.animalCategory = allAnimal.animalCategory;
                        child.timeToTake = allAnimal.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = allAnimal.amountWaterNeeded;
                        child.amountFoodNeeded = allAnimal.amountFoodNeeded;
                        child.health = allAnimal.health;
                        child.maxHealth = allAnimal.health;
                    }
                    allAnimal.lastBorn = 0;
                    temp.Add(child);
                }
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                horse.Add(tAnimal);
        }
        temp.Clear();
        foreach (Animal allAnimal in horse)
        {
            if (allAnimal.sex == AnimalSex.male.ToString() && allAnimal.age < allAnimal.maxAge)
            {
                maleAnimal++;
            }
        }
        foreach (Animal allAnimal in horse)
        {
            if (allAnimal.sex == AnimalSex.female.ToString() && allAnimal.lastBorn < allAnimal.distanceBeetweenBorn && maleAnimal > 0)
            {
                allAnimal.lastBorn++;
                maleAnimal--;
            }
        }
        maleAnimal = 0;
        #endregion

        #region PIG
        foreach (Animal allAnimal in goat)
        {
            if (allAnimal.sex == AnimalSex.female.ToString())
            {
                if (allAnimal.health > 0 && allAnimal.lastBorn == allAnimal.distanceBeetweenBorn)
                {
                    Animal child = new Animal();
                    int random = UnityEngine.Random.Range(0, 11);
                    if (random <= 5)
                    {
                        child.name = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.name;
                        child.otherSexName = allAnimal.sex;
                        child.sex = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.sex.ToString();
                        child.distanceBeetweenBorn = 0;
                        child.age = 0;
                        child.maxAge = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.maxAge;
                        child.animalCategory = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.animalCategory.ToString().ToUpper();
                        child.timeToTake = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountWaterNeeded;
                        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountFoodNeeded;
                        child.health = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                        child.maxHealth = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                    }
                    if (random > 5)
                    {
                        child.name = allAnimal.name;
                        child.sex = allAnimal.sex;
                        child.otherSexName = allAnimal.otherSexName;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.age = 0;
                        child.maxAge = allAnimal.maxAge;
                        child.animalCategory = allAnimal.animalCategory;
                        child.timeToTake = allAnimal.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = allAnimal.amountWaterNeeded;
                        child.amountFoodNeeded = allAnimal.amountFoodNeeded;
                        child.health = allAnimal.health;
                        child.maxHealth = allAnimal.health;
                    }
                    allAnimal.lastBorn = 0;
                    temp.Add(child);
                }
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                goat.Add(tAnimal);
        }
        temp.Clear();
        foreach (Animal allAnimal in goat)
        {
            if (allAnimal.sex == AnimalSex.male.ToString() && allAnimal.age < allAnimal.maxAge)
            {
                maleAnimal++;
            }
        }
        foreach (Animal allAnimal in goat)
        {
            if (allAnimal.sex == AnimalSex.female.ToString() && allAnimal.lastBorn < allAnimal.distanceBeetweenBorn && maleAnimal > 0)
            {
                allAnimal.lastBorn++;
                maleAnimal--;
            }
        }
        maleAnimal = 0;
        #endregion

        #region CHICKEN
        foreach (Animal allAnimal in chicken)
        {
            if (allAnimal.sex == AnimalSex.female.ToString())
            {
                if (allAnimal.health > 0 && allAnimal.lastBorn == allAnimal.distanceBeetweenBorn)
                {
                    Animal child = new Animal();
                    int random = UnityEngine.Random.Range(0, 11);
                    if (random <= 5)
                    {
                        child.name = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.name;
                        child.otherSexName = allAnimal.sex;
                        child.sex = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.sex.ToString();
                        child.distanceBeetweenBorn = 0;
                        child.age = 0;
                        child.maxAge = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.maxAge;
                        child.animalCategory = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.animalCategory.ToString().ToUpper();
                        child.timeToTake = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountWaterNeeded;
                        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountFoodNeeded;
                        child.health = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                        child.maxHealth = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                    }
                    if (random > 5)
                    {
                        child.name = allAnimal.name;
                        child.sex = allAnimal.sex;
                        child.otherSexName = allAnimal.otherSexName;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.age = 0;
                        child.maxAge = allAnimal.maxAge;
                        child.animalCategory = allAnimal.animalCategory;
                        child.timeToTake = allAnimal.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = allAnimal.amountWaterNeeded;
                        child.amountFoodNeeded = allAnimal.amountFoodNeeded;
                        child.health = allAnimal.health;
                        child.maxHealth = allAnimal.health;
                    }
                    allAnimal.lastBorn = 0;
                    temp.Add(child);
                }
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                chicken.Add(tAnimal);
        }
        temp.Clear();
        foreach (Animal allAnimal in chicken)
        {
            if (allAnimal.sex == AnimalSex.male.ToString() && allAnimal.age < allAnimal.maxAge)
            {
                maleAnimal++;
            }
        }
        foreach (Animal allAnimal in chicken)
        {
            if (allAnimal.sex == AnimalSex.female.ToString() && allAnimal.lastBorn < allAnimal.distanceBeetweenBorn && maleAnimal > 0)
            {
                allAnimal.lastBorn++;
                maleAnimal--;
            }
        }
        maleAnimal = 0;
        #endregion

        #region SHEEP
        foreach (Animal allAnimal in sheep)
        {
            if (allAnimal.sex == AnimalSex.female.ToString())
            {
                if (allAnimal.health > 0 && allAnimal.lastBorn == allAnimal.distanceBeetweenBorn)
                {
                    Animal child = new Animal();
                    int random = UnityEngine.Random.Range(0, 11);
                    if (random <= 5)
                    {
                        child.name = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.name;
                        child.otherSexName = allAnimal.sex;
                        child.sex = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.sex.ToString();
                        child.distanceBeetweenBorn = 0;
                        child.age = 0;
                        child.maxAge = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.maxAge;
                        child.animalCategory = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.animalCategory.ToString().ToUpper();
                        child.timeToTake = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountWaterNeeded;
                        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.amountFoodNeeded;
                        child.health = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                        child.maxHealth = GeneralManager.singleton.FindAnimal(allAnimal.name).otherSex.health;
                    }
                    if (random > 5)
                    {
                        child.name = allAnimal.name;
                        child.sex = allAnimal.sex;
                        child.otherSexName = allAnimal.otherSexName;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.age = 0;
                        child.maxAge = allAnimal.maxAge;
                        child.animalCategory = allAnimal.animalCategory;
                        child.timeToTake = allAnimal.timeToTake;
                        child.lastTake = 0;
                        child.distanceBeetweenBorn = allAnimal.distanceBeetweenBorn;
                        child.lastBorn = 0;
                        child.amountWaterNeeded = allAnimal.amountWaterNeeded;
                        child.amountFoodNeeded = allAnimal.amountFoodNeeded;
                        child.health = allAnimal.health;
                        child.maxHealth = allAnimal.health;
                    }
                    allAnimal.lastBorn = 0;
                    temp.Add(child);
                }
            }
        }
        foreach (Animal tAnimal in temp)
        {
            if (sum < animalEachLevel.Get(GetComponent<Building>().level))
                sheep.Add(tAnimal);
        }
        temp.Clear();
        foreach (Animal allAnimal in sheep)
        {
            if (allAnimal.sex == AnimalSex.male.ToString() && allAnimal.age < allAnimal.maxAge)
            {
                maleAnimal++;
            }
        }
        foreach (Animal allAnimal in sheep)
        {
            if (allAnimal.sex == AnimalSex.female.ToString() && allAnimal.lastBorn < allAnimal.distanceBeetweenBorn && maleAnimal > 0)
            {
                allAnimal.lastBorn++;
                maleAnimal--;
            }
        }
        maleAnimal = 0;
        #endregion

    }
    [Server]
    public void CheckTakeProduct()
    {
        foreach (Animal anim in cow)
        {
            if (anim.health > 0 && anim.lastTake < GeneralManager.singleton.FindAnimal(anim.name).timeToTake)
            {
                anim.lastTake++;
            }
        }
        foreach (Animal anim in horse)
        {
            if (anim.health > 0 && anim.lastTake < GeneralManager.singleton.FindAnimal(anim.name).timeToTake)
            {
                anim.lastTake++;
            }
        }
        foreach (Animal anim in goat)
        {
            if (anim.health > 0 && anim.lastTake < GeneralManager.singleton.FindAnimal(anim.name).timeToTake)
            {
                anim.lastTake++;
            }
        }
        foreach (Animal anim in chicken)
        {
            if (anim.health > 0 && anim.lastTake < GeneralManager.singleton.FindAnimal(anim.name).timeToTake)
            {
                anim.lastTake++;
            }
        }
        foreach (Animal anim in sheep)
        {
            if (anim.health > 0 && anim.lastTake < GeneralManager.singleton.FindAnimal(anim.name).timeToTake)
            {
                anim.lastTake++;
            }
        }
    }
    [Server]
    public void RemoveAutomaticallyAfterDeath()
    {
        List<Animal> animalToRemove = new List<Animal>();
        foreach (Animal anim in cow)
        {
            if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                animalToRemove.Add(anim);
            }
        }
        foreach (Animal anim in animalToRemove)
        {
            cow.Remove(anim);
        }
        animalToRemove.Clear();

        foreach (Animal anim in horse)
        {
            if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                animalToRemove.Add(anim);
            }
        }
        foreach (Animal anim in animalToRemove)
        {
            horse.Remove(anim);
        }
        animalToRemove.Clear();

        foreach (Animal anim in goat)
        {
            if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                animalToRemove.Add(anim);
            }
        }
        foreach (Animal anim in animalToRemove)
        {
            goat.Remove(anim);
        }
        animalToRemove.Clear();

        foreach (Animal anim in chicken)
        {
            if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                animalToRemove.Add(anim);
            }
        }
        foreach (Animal anim in animalToRemove)
        {
            chicken.Remove(anim);
        }
        animalToRemove.Clear();

        foreach (Animal anim in sheep)
        {
            if (anim.age >= GeneralManager.singleton.FindAnimal(anim.name).maxAge)
            {
                animalToRemove.Add(anim);
            }
        }
        foreach (Animal anim in animalToRemove)
        {
            sheep.Remove(anim);
        }
        animalToRemove.Clear();
    }
    [Server]
    public void CheckNewBorn()
    {
        int sum = cow.Count + horse.Count + goat.Count + chicken.Count + sheep.Count;
        if (animalEachLevel.Get(GetComponent<Building>().level) == sum) return;
        int male = 0;
        int female = 0;
        int remaining = 0;

        #region COW
        for (int i = 0; i < cow.Count; i++)
        {
            if (cow[i].age == cow[i].maxAge) continue;
            if (cow[i].sex == AnimalSex.male.ToString())
            {
                male++;
            }
            else
            {
                female++;
            }
        }
        if (female >= male)
        {
            remaining = ActualFemale(female, male);
        }
        else
        {
            remaining = female;
        }

        if (remaining < 0) remaining = 0;

        for (int ii = 0; ii < remaining; ii++)
        {
            Animal animal = new Animal();
            animal.animalCategory = "COW";
            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                animal.sex = "MALE";
                animal.otherSexName = "Taurus";
            }
            else
            {
                animal.sex = "FEMALE";
                animal.otherSexName = "Cow";
            }
            AddToAnimalList(cow, animal);
        }
        male = female = remaining = 0;
        #endregion

        #region HORSE
        for (int i = 0; i < horse.Count; i++)
        {
            if (horse[i].age == horse[i].maxAge) continue;
            if (horse[i].sex == AnimalSex.male.ToString())
            {
                male++;
            }
            else
            {
                female++;
            }
        }
        if (female >= male)
        {
            remaining = ActualFemale(female, male);
        }
        else
        {
            remaining = female;
        }

        if (remaining < 0) remaining = 0;

        for (int ii = 0; ii < remaining; ii++)
        {
            Animal animal = new Animal();
            animal.animalCategory = "HORSE";

            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                animal.sex = "MALE";
                animal.otherSexName = "Stallion";
            }
            else
            {
                animal.sex = "FEMALE";
                animal.otherSexName = "Mare";
            }
            AddToAnimalList(horse, animal);
        }
        male = female = remaining = 0;
        #endregion

        #region GOAT
        for (int i = 0; i < goat.Count; i++)
        {
            if (goat[i].age == goat[i].maxAge) continue;
            if (goat[i].sex == AnimalSex.male.ToString())
            {
                male++;
            }
            else
            {
                female++;
            }
        }
        if (female >= male)
        {
            remaining = ActualFemale(female, male);
        }
        else
        {
            remaining = female;
        }

        if (remaining < 0) remaining = 0;

        for (int ii = 0; ii < remaining; ii++)
        {
            Animal animal = new Animal();
            animal.animalCategory = "GOAT";

            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                animal.sex = "MALE";
                animal.otherSexName = "Goat";
            }
            else
            {
                animal.sex = "FEMALE";
                animal.otherSexName = "Billygoat";
            }

            AddToAnimalList(goat, animal);
        }
        male = female = remaining = 0;
        #endregion

        #region CHICKEN
        for (int i = 0; i < chicken.Count; i++)
        {
            if (chicken[i].age == chicken[i].maxAge) continue;
            if (chicken[i].sex == AnimalSex.male.ToString())
            {
                male++;
            }
            else
            {
                female++;
            }
        }
        if (female >= male)
        {
            remaining = ActualFemale(female, male);
        }
        else
        {
            remaining = female;
        }

        if (remaining < 0) remaining = 0;

        for (int ii = 0; ii < remaining; ii++)
        {
            Animal animal = new Animal();
            animal.animalCategory = "CHICKEN";

            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                animal.sex = "MALE";
                animal.otherSexName = "Cock";
            }
            else
            {
                animal.sex = "FEMALE";
                animal.otherSexName = "Hen";
            }

            AddToAnimalList(chicken, animal);
        }
        male = female = remaining = 0;
        #endregion

        #region SHEEP
        for (int i = 0; i < sheep.Count; i++)
        {
            if (sheep[i].age == sheep[i].maxAge) continue;
            if (sheep[i].sex == AnimalSex.male.ToString())
            {
                male++;
            }
            else
            {
                female++;
            }
        }
        if (female >= male)
        {
            remaining = ActualFemale(female, male);
        }
        else
        {
            remaining = female;
        }

        if (remaining < 0) remaining = 0;

        for (int ii = 0; ii < remaining; ii++)
        {
            Animal animal = new Animal();
            animal.animalCategory = "SHEEP";

            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                animal.sex = "MALE";
                animal.otherSexName = "Mutton";
            }
            else
            {
                animal.sex = "FEMALE";
                animal.otherSexName = "Sheep";
            }

            AddToAnimalList(sheep, animal);
        }
        male = female = remaining = 0;
        #endregion
    }

    public int ActualFemale(int female, int male)
    {
        return female - (female - male);
    }


    public void AddToAnimalList(SyncListAnimal animalList, Animal animal)
    {
        Animal child = new Animal();

        child.name = animal.otherSexName;
        if(animal.sex == "MALE")
            child.sex = AnimalSex.male.ToString();
        else
            child.sex = AnimalSex.male.ToString();
        child.otherSexName = GeneralManager.singleton.FindAnimal(animal.otherSexName).name;
        child.distanceBeetweenBorn = 0;
        child.age = 0;
        child.maxAge = GeneralManager.singleton.FindAnimal(animal.otherSexName).maxAge;
        child.animalCategory = GeneralManager.singleton.FindAnimal(animal.otherSexName).animalCategory.ToString().ToUpper();
        child.timeToTake = GeneralManager.singleton.FindAnimal(animal.otherSexName).timeToTake;
        child.lastTake = 0;
        child.distanceBeetweenBorn = GeneralManager.singleton.FindAnimal(animal.otherSexName).distanceBeetweenBorn;
        child.lastBorn = 0;
        child.amountWaterNeeded = GeneralManager.singleton.FindAnimal(animal.otherSexName).amountWaterNeeded;
        child.amountFoodNeeded = GeneralManager.singleton.FindAnimal(animal.otherSexName).amountFoodNeeded;
        child.health = GeneralManager.singleton.FindAnimal(animal.otherSexName).health;
        child.maxHealth = GeneralManager.singleton.FindAnimal(animal.otherSexName).health;

        if (sum < animalEachLevel.Get(GetComponent<Building>().level))
            animalList.Add(child);
    }
}

public class SyncListAnimal : SyncList<Animal> { }

[System.Serializable]
public partial class Animal
{
    public string name;
    public string otherSexName;
    public int age;
    public int maxAge;
    public int toAdultAge;
    public string animalCategory;
    public string sex;
    public int timeToTake;
    public int lastTake;
    public int distanceBeetweenBorn;
    public int lastBorn;
    public int amountWaterNeeded;
    public int amountFoodNeeded;
    public int health;
    public int maxHealth;
}


public enum AnimalCategory { nothing, cow, horse, goat, chicken, sheep }

public enum AnimalSex { male, female }

public enum AnimalDeathAtTake { yes, no }


[System.Serializable]
public partial class AnimalItem
{
    public ScriptableItem item;
    public int amount;
}