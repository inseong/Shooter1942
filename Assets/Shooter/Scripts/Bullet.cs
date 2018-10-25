using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Normal,
    GuidedMissile,
    Bomb,
    Laser,
}

public class Bullet : MonoBehaviour
{
    public BulletType bulletType;
    public float speed = 20;
    public float damage = 10;
    public float duration = 0;

    private float elapsedTime = 0;
    private float t = 0;
    private float marginW = 100;
    private float marginH = 100;

    public delegate void UpdateMethod();
    public UpdateMethod OnFixedUpdate;

    // Use this for initialization
    void Start ()
    {
        Init();
    }

    public void Init()
    {
        elapsedTime = 0;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null && sr.sprite != null)
        {
            marginH = sr.sprite.rect.height;
            marginW = sr.sprite.rect.width;
        }

        OnFixedUpdate = FixedUpdateNormal;
        if (duration > 0)
            OnFixedUpdate = FixedUpdateDuration;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        OnFixedUpdate();
    }

    void FixedUpdateNormal()
    {
        t = Time.deltaTime;
        this.transform.Translate(0, t * speed, 0);

        // if it's out of scope, return it to the pool
        float x = this.transform.position.x;
        float y = this.transform.position.y;
        if (x < -StageManager.instance.screenX - marginW || x > StageManager.instance.screenX + marginW
           || y < -StageManager.instance.screenY - marginH || y > StageManager.instance.screenY + marginH)
            ObjectPool.instance.ReturnObjectToPool(this.gameObject);
    }

    void FixedUpdateDuration()
    {
        t = Time.deltaTime;
        elapsedTime += t;

        if (elapsedTime > duration)
        {
            elapsedTime = 0;
            ObjectPool.instance.ReturnObjectToPool(this.gameObject);
        }
        else
        {
            this.transform.Translate(0, t * speed, 0);
        }
    }
}
