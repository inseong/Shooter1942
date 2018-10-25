using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletSpawn : MonoBehaviour
{
    public Bullet bullet;
    public float spawnInterval = 0.25f;
    public float speed = 220;
    public float duration = 0;
    public bool autoFiring = false;
    public bool autoRotating = false;
    public bool autoTargeting = false;

    public delegate void UpdateMethod();
    public UpdateMethod OnFixedUpdateAutoFire;
    public UpdateMethod OnFixedUpdateAutoMove;

    private bool _autoFire;
    public bool autoFire
    {
        get { return _autoFire;  }
        set
        {
            autoFiring = value;
            _autoFire = value;
            if (_autoFire)
                OnFixedUpdateAutoFire = FixedUpdateAutoFire;
            else
                OnFixedUpdateAutoFire = FixedUpdateNormal;   
        }
    }

    private bool _autoRotate;
    public bool autoRotate
    {
        get { return _autoRotate;  }
        set
        {
            autoRotating = value;
            _autoRotate = value;
            if (_autoRotate)
                OnFixedUpdateAutoMove = FixedUpdateAutoRotate;
            else if (!_autoTarget)
                OnFixedUpdateAutoMove = FixedUpdateNormal;
        }
    }

    private bool _autoTarget;
    public bool autoTarget
    {
        get { return _autoTarget; }
        set
        {
            autoTargeting = value;
            _autoTarget = value;
            if (_autoTarget)
                OnFixedUpdateAutoMove = FixedUpdateAutoTarget;
            else if (!_autoRotate)
                OnFixedUpdateAutoMove = FixedUpdateNormal;
        }
    }


    private float t;
    // Use this for initialization
    void Start()
    {
        autoFire = autoFiring;
        autoRotate = autoRotating;
        autoTarget = autoTargeting;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        OnFixedUpdateAutoFire();
        OnFixedUpdateAutoMove();
    }

    void FixedUpdateNormal()
    {
        
    }

    void FixedUpdateAutoFire()
    {
        t += Time.deltaTime;
        if (t >= spawnInterval)
        {
            Fire();
            t = 0;
        }
    }

    void FixedUpdateAutoRotate()
    {
        t = Time.deltaTime;
    }

    void FixedUpdateAutoTarget()
    {
        t = Time.deltaTime;
    }


    public void Fire()
    {
        GameObject newBullet = ObjectPool.instance.GetPooledObject(bullet.gameObject);

        newBullet.transform.position = this.transform.position;
        newBullet.transform.rotation = this.transform.rotation;
        Bullet tmp = newBullet.GetComponent<Bullet>();
        tmp.speed = speed;
        tmp.duration = duration;
    }
}