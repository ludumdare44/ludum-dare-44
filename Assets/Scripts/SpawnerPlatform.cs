﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class SpawnerPlatform : MonoBehaviour
{
    private const int PlatformLength = 8;
    private const int InitialFlatPlatformCount = 2;
    private const int PlatformCountAhead = 8;
    private const int MinTimeBetweenQte = 10;
    private const double QteSpawnProbability = 0.4;

    public Object[] basicPlatform;
    public Object[] obstaclePlatform;
    public Object[] buildingPlatform;
    public Object basicBase;
    public Object obstacleBase;
    public Object buildingBase;
    public GameObject spotlightTemplate;
    
    private Object[][] platforms;
    private Object[] bases;
    private int[] platformWeights;

    public GameObject player;

    private int _nextSpawnLocation;
    private int _timeSinceLastQte;
    
    // Start is called before the first frame update
    void Start()
    {
        _nextSpawnLocation = -1;
        _timeSinceLastQte = 0;
        
        platforms = new[] {basicPlatform, basicPlatform};
        bases = new[] {basicBase, obstacleBase};
        platformWeights = new[] {1, 4};
        
        while (_nextSpawnLocation <= InitialFlatPlatformCount)
        {
            var selectedPlatform = RandomPlatformOf(basicPlatform);
            CreateNewPlatform(selectedPlatform, basicBase, _nextSpawnLocation);
        }
        while (ShouldSpawnNewPlatform())
        {
            SpawnNextPlatform();
        }
    }

    private void CreateNewPlatform(Object selectedPlatform, Object selectedBase, int location)
    {
        var newPlatform = Instantiate(selectedPlatform, new Vector3(location * PlatformLength, 0.5F, 0), Quaternion.Euler(0, 90, 0)) as GameObject;
        var newBase = Instantiate(selectedBase, new Vector3(location * PlatformLength, 0, 0), Quaternion.identity) as GameObject;
        newPlatform.transform.SetParent(newBase.transform);
        newBase.transform.SetParent(this.transform);

        foreach (Transform pplatform in newPlatform.transform)
        {
            foreach (Transform item in pplatform)
            {
                if ("SM_Light".Equals(item.name))
                {
                    var spotlight = Instantiate(
                        spotlightTemplate,
                        item.position + new Vector3(0, 4, -0.5F),
                        Quaternion.identity
                    ) as GameObject;
                    spotlight.transform.SetParent(newPlatform.transform);
                    spotlight.SetActive(true);
                }
            }
        }

        _nextSpawnLocation++;
    }

    private Object WeightedRandomPlatform(int randomWeightedIndex)
    {
        var currentPlatformType = platforms[randomWeightedIndex];
        var selectedPlatform = RandomPlatformOf(currentPlatformType);
        return selectedPlatform;
    }

    private int RandomWeightedIndex()
    {
        var platformType = RandomIntRange(0, platformWeights.Sum());
        var selectedIndex = IndexedWeight(platformWeights, platformType);
        return selectedIndex;
    }

    private static int IndexedWeight(int[] platformWeights, int platformType)
    {
        var sumWeights = 0;
        var i = 0;
        while (i < platformWeights.Length)
        {
            sumWeights += platformWeights[i];
            if (platformType < sumWeights)
            {
                return i;
            }
            i++;
        }

        throw new IndexOutOfRangeException();
    }

    private static Object RandomPlatformOf(Object[] currentPlatformType)
    {
        return currentPlatformType[RandomIntRange(0, currentPlatformType.Length)];
    }

    private static int RandomIntRange(int min, int max)
    {
        return (int)Math.Floor((double)Random.Range(min, max));
    }

    void Update()
    {
        if (ShouldSpawnNewPlatform())
        {
            SpawnNextPlatform();
        }
    }

    private bool ShouldSpawnNewPlatform()
    {
        return player.transform.position.x - ((_nextSpawnLocation - PlatformCountAhead) * PlatformLength) > 0;
    }
    
    private void SpawnNextPlatform()
    {
        var lastChild = transform.GetChild(transform.childCount - 1);

        if (lastChild.CompareTag("qte"))
        {
            CreateNewPlatform(RandomPlatformOf(basicPlatform), basicBase, _nextSpawnLocation);
            CreateNewPlatform(RandomPlatformOf(basicPlatform), basicBase, _nextSpawnLocation);
        }
        else if (_timeSinceLastQte >= MinTimeBetweenQte && Random.Range(0F, 1F) < QteSpawnProbability)
        {
            CreateNewPlatform(RandomPlatformOf(buildingPlatform), buildingBase, _nextSpawnLocation);
            
            _timeSinceLastQte = 0;
        }
        else
        {
            var index = RandomWeightedIndex();
            var selectedBase = bases[index];
            CreateNewPlatform(WeightedRandomPlatform(index), selectedBase, _nextSpawnLocation);
            
            _timeSinceLastQte++;
        }

    }

    public GameObject FindActivePlatform()
    {
        return FindPlatformUnder(player.transform.position.x);
    }

    public GameObject FindNextPlatform()
    {
        return FindPlatformUnder(player.transform.position.x + PlatformLength);
    }

    private GameObject FindPlatformUnder(float position)
    {
        foreach (Transform platform in transform)
        {
            var platformLeftmost = platform.position.x - PlatformLength / 2F;
            if (platformLeftmost < position && position < (platformLeftmost + PlatformLength))
            {
                return platform.gameObject;
            }
        }

        throw new IndexOutOfRangeException();
    }

    public float FindPlatformRelativePosition()
    {
        var playerLeftmost = (player.transform.position.x - PlatformLength / 2F);
        return (playerLeftmost % PlatformLength) / (PlatformLength * 1F);
    }
}
