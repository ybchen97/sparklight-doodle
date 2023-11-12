using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner manager;

    public int spawnWaitInterval = 2;

    public GameObject[] monsterPrefabs;
    float[] monsterProbs;
    public int maxMonsterCount = 10;
    public int monsterCount;
    float xRange = 5f;
    // float yRange = 0.1f;
    float zRange = 5f;

    private void Awake()
    {
        manager = this;
    }

    public void DecrementMonsterCount()
    {
        AudioSource sound = GetComponent<AudioSource>();
        if (sound != null)
        {
            sound.Play();
        }
        monsterCount -= 1;
    }

    int GetRandomMonster(float[] probs)
    {
        float total = 0;
        float randomChance = Random.Range(0f, 1f);
        for (int a = 0; a < probs.Length; a++)
        {
            total += probs[a];
            if (randomChance <= total)
            {
                return a;
            }
        }
        return -1;
    }

    public Vector3 GetRandomLocation()
    {
        Vector3 perturbation = new(
            Random.Range(-xRange, xRange),
            0.5f,
            Random.Range(-zRange, zRange));
        return perturbation;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Compute spawn probability
        float monsterTotal = 0;
        for (int i = 0; i < monsterPrefabs.Length; i++)
        {
            monsterTotal += monsterPrefabs[0].GetComponent<Monster>().scale;
        }
        monsterProbs = new float[monsterPrefabs.Length];
        for (int i = 0; i < monsterProbs.Length; i++)
        {
            monsterProbs[i] = monsterPrefabs[0].GetComponent<Monster>().scale / monsterTotal;
        }

        // Spawn initial wave of monsters
        while (monsterCount < maxMonsterCount)
        {
            int idx = GetRandomMonster(monsterProbs);
            createMonsterObject(idx);
            monsterCount += 1;
        }
        StartCoroutine(spawnRandomMonster());
    }

    private IEnumerator spawnRandomMonster()
    {
        yield return new WaitForSeconds(spawnWaitInterval);
        int idx = GetRandomMonster(monsterProbs);

        if (monsterCount < maxMonsterCount)
        {
            createMonsterObject(idx);
            monsterCount += 1;

        }
        StartCoroutine(spawnRandomMonster());
    }

    private void createMonsterObject(int idx)
    {
        GameObject newMonster = Instantiate(monsterPrefabs[idx], transform);
        newMonster.transform.position = GetRandomLocation();
        newMonster.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
        // Vector3 monsterForward = newMonster.GetComponent<Monster>().forward;
        // newMonster.GetComponent<Monster>().forward = newMonster.transform.rotation * monsterForward;
    }
}
