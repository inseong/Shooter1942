using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaneType
{
    Player,
    Fighter01,
    Boss01,
}

public class Plane : MonoBehaviour
{
    public PlaneType planeType;
    public float energy = 100;
    public float damage = 10;
    public float speed = 3.5f;
    public int score = 100;

    public LifeBar lifeBar;

    public BulletSpawn[] bulletSpawns;
    protected int activatedSpawnCount = 0;
    protected Animator animator;

    public bool autoFiring = false;
    private bool _autoFire;
    public bool autoFire
    {
        get { return _autoFire; }

        set
        {
            _autoFire = value;
            for (int i = 0; i < bulletSpawns.Length; i++)
            {
                bulletSpawns[i].autoFire = _autoFire;
            }
        }
    }

    public virtual void Init()
    {
        activatedSpawnCount = 1;

        if (bulletSpawns.Length < activatedSpawnCount)
            activatedSpawnCount = bulletSpawns.Length;

        DisableFire();

        autoFire = autoFiring;
        animator = GetComponent<Animator>();

        if (lifeBar != null)
        {
            lifeBar.lifeAmount = energy;
            lifeBar.UpdateLifeBar(energy);
        }
    }

    public void EnableFire()
    {
        for (int i = 0; i < bulletSpawns.Length; i++)
            bulletSpawns[i].enabled = true;
    }

    public void DisableFire()
    {
        for (int i = 0; i < bulletSpawns.Length; i++)
            bulletSpawns[i].enabled = false;
    }


    public virtual void ReadyMode() {}
    public virtual void MoveMode() {}
    public virtual void DeadMode() {}
}
