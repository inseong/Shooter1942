using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterEnemy : Plane
{
    public FSM<FighterEnemy> fsm;

    private float t = 0;
    private float marginW = 100;
    private float marginH = 100;

    void Awake()
    {
        fsm = new FSM<FighterEnemy>(this);
    }

	// Use this for initialization
	void Start ()
    {
        Init();
	}
	
    public override void Init()
    {
        base.Init();
        ReadyMode();
    }

    void FixedUpdate()
    {
        fsm.currentState.FixedUpdate();
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
    }

    protected void ExlosionAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Explosion"))
            animator.CrossFade("Explosion", 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!fsm.IsState(typeof(DeadState)))
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

            if (energy <= 0)
                DeadMode();
        }
    }

    protected void MoveForward()
    {
        t = Time.deltaTime * speed;

        Vector3 playerPos = PlayManager.instance.player.transform.position;
        Vector3 pos = transform.position;

        speed += speed*0.005f;

        Vector3 mov = pos - playerPos;

        if (mov.y < 200.0f)
            mov.y = 200;

        mov = mov.normalized * t;
        this.transform.Translate(mov);

        // if it's out of scope, return it to the pool
        float x = this.transform.position.x;
        float y = this.transform.position.y;
        if (x < -StageManager.instance.screenX - marginW || x > StageManager.instance.screenX + marginW
           || y < -StageManager.instance.screenY - marginH || y > StageManager.instance.screenY + marginH)
            ObjectPool.instance.ReturnObjectToPool(this.gameObject);
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
    public class ReadyState : BaseFSM<FighterEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            owner.DisableFire();
            owner.MoveMode();
        }

        protected override void FixedUpdateFunc()
        {
        }
        public override void Finish()
        {
            base.Finish();
        }
    }

    public class MoveState : BaseFSM<FighterEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            owner.EnableFire();
        }
        protected override void FixedUpdateFunc()
        {
            owner.MoveForward();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }

    public class DeadState : BaseFSM<FighterEnemy>
    {
        public override void Begin()
        {
            base.Begin();
            PlayManager.instance.AddScore(owner.score);
            PlayManager.instance.AddScoreCount(owner.score);
            PlayManager.instance.AddFighterCount();
 
            owner.DisableFire();

            owner.ExlosionAnimation();
            owner.StartCoroutine(DelayTime(0.5f));
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
