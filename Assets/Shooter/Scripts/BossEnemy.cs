using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Plane
{
    public FSM<BossEnemy> fsm;
    private float t = 0;
    private float elapsedTime = 0;
    private float distance = 0;

    private float marginW = 100;
    private float marginH = 100;

    void Awake()
    {
        fsm = new FSM<BossEnemy>(this);
    }

	void Start ()
    {
        Init();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            marginH = sr.sprite.rect.height;
            marginW = sr.sprite.rect.width;
        }
	}
	
    void FixedUpdate()
    {
        fsm.currentState.FixedUpdate();
    }

    public override void Init()
    {
        base.Init();

        elapsedTime = 0;
        distance = 0;

        ReadyMode();
    }

    public override void ReadyMode()
    {
        fsm.ChangeState(typeof(ReadyState));
    }

    public override void MoveMode()
    {
        fsm.ChangeState(typeof(MoveState));
    }

    public override void DeadMode()
    {
        fsm.ChangeState(typeof(DeadState));
    }

    protected void IdleAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Idle"))
            animator.CrossFade("Idle", 0);
    }

    protected void DamageAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Damage"))
        {
            animator.CrossFade("Damage", 0);
            state = animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    protected void ExlosionAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Explosion"))
            animator.CrossFade("Explosion", 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!fsm.IsState(typeof(DeadState)))
        {
            float dmg = 0;
            if (collision.gameObject.tag.Equals("Player"))
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                    dmg = player.damage;

                DamageAnimation();
            }
            else if (collision.gameObject.tag.Equals("PlayerBullet"))
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                if (bullet != null)
                    dmg = bullet.damage;

                ObjectPool.instance.ReturnObjectToPool(collision.gameObject);

                DamageAnimation();
            }

            energy -= dmg;
            if (lifeBar != null)
                lifeBar.UpdateLifeBar(energy);
            
            if (energy <= 0)
                DeadMode();            
        }
    }

    protected bool MoveForward(float dist)
    {
        t = Time.deltaTime;
        elapsedTime += t;
        distance += t * speed;
        this.transform.Translate(0, t* speed, 0);
        if (distance >= dist)
            return true;
        return false;
    }

    protected void MovingToPlayer()
    {
        t = Time.deltaTime * speed;

        Vector3 playerPos = PlayManager.instance.player.transform.position;
        Vector3 pos = transform.position;

        Vector3 mov = (playerPos - pos).normalized;
        mov *= t;

        this.transform.Translate(mov);
       
    }
    //----------------------------------------------------------------------------------
    // FSM Classes
    public class ReadyState : BaseFSM<BossEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            owner.DisableFire();
        }

        protected override void FixedUpdateFunc()
        {
            if (owner.MoveForward(350))
                owner.MoveMode();
        }
        public override void Finish()
        {
            base.Finish();
        }
    }

    public class MoveState : BaseFSM<BossEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            owner.EnableFire();
        }

        protected override void FixedUpdateFunc()
        {
            owner.MovingToPlayer();
        }
        public override void Finish()
        {
            base.Finish();
        }
    }

    public class DeadState : BaseFSM<BossEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            PlayManager.instance.AddScore(owner.score);
            PlayManager.instance.AddBossCount();
            PlayManager.instance.AddScoreCount(owner.score);

            owner.DisableFire();
            owner.ExlosionAnimation();
            owner.StartCoroutine(DelayTime(1.5f));
        }

        public IEnumerator DelayTime(float t)
        {
            yield return new WaitForSeconds(t);
            ObjectPool.instance.ReturnObjectToPool(owner.gameObject);
        }

        protected override void FixedUpdateFunc()
        {
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
