using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    private SpawnData spawnData;
    private bool spawnFlag;
    public bool IsSpawnning
    {
        get { return spawnFlag; }
    }

    private int spawnCount;
    private Vector3 pos;
    private WaitForSeconds waitForSeconds;

	// Use this for initialization
	void Start ()
    {
        spawnFlag = false;
	}
	
    public void SpawnningStart(SpawnData spawn)
    {
        spawnFlag = true;
        spawnData = spawn;
        spawnCount = spawnData.count;
 
        pos = transform.position;
        pos.y += spawnData.spawnOffset;

        waitForSeconds = new WaitForSeconds(spawnData.interval);
        StartCoroutine(Spawnning());

    }

    IEnumerator Spawnning()
    {
        while(spawnCount > 0)
        {
            GameObject newObj = ObjectPool.instance.GetPooledObject(spawnData.plane);
            newObj.transform.position = pos;
            newObj.transform.rotation = this.transform.rotation;

            Plane enemy = newObj.GetComponent<Plane>();
            enemy.speed = spawnData.speed;
            enemy.energy = spawnData.energy;

            enemy.Init();

            --spawnCount;
            yield return waitForSeconds;
        }

        spawnFlag = false;
    }
}
