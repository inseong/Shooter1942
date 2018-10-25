using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Plane
{
    public bool useKeyboard = false;

    private Vector2 currentPos, previousPos;
    private Vector2 movingDir;
    private float moveSpeed;
    private Camera mainCamera;
    private float screenRatio;

    private Touch touch;
    private Vector2 pos;

    public FSM<Player> fsm;

    public delegate void UpdateInput();
    public UpdateInput OnUpdateInput;

    void Awake()
    {
        fsm = new FSM<Player>(this);
    }

    void Start()
    {
        Init();

        if(!useKeyboard)
        {
            autoFire = true;
            OnUpdateInput = UpdateInputTouch;
        }
        else
            OnUpdateInput = UpdateInputKeyboard;

        mainCamera = Camera.main;
    }

    public override void Init()
    {
        base.Init();

        moveSpeed = speed;
        ReadyMode();

        currentPos = previousPos = transform.position;
    }

    void FixedUpdate()
    {
        fsm.currentState.FixedUpdate();
    }

    protected void UpdateInputKeyboard()
    {
        pos = transform.position;

        if (Input.GetKey(KeyCode.DownArrow))
            MoveDown();
        else if (Input.GetKey(KeyCode.RightArrow))
            MoveRight();
        else if (Input.GetKey(KeyCode.UpArrow))
            MoveUp();
        else if (Input.GetKey(KeyCode.LeftArrow))
            MoveLeft();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            Shooting();

        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            movingDir = Vector2.zero;

        if (movingDir.x < 0)
            TurnLeftAnimation();
        else if (movingDir.x > 0)
            TurnRightAnimation();
        else
            IdleAnimation();
    }

    protected void UpdateInputTouch()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            screenRatio = StageManager.instance.screenY * 2 / (float)Screen.height;
            previousPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            currentPos = Input.mousePosition;
            pos.x = (currentPos.x - previousPos.x) * screenRatio;
            pos.y = (currentPos.y - previousPos.y) * screenRatio;
            transform.Translate(pos.x, pos.y, 0);
            previousPos = currentPos;
        }
#else
        touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            screenRatio = StageManager.instance.screenY*2 / (float)Screen.height;
            previousPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            currentPos = touch.position;
            pos.x = (currentPos.x - previousPos.x) * screenRatio;
            pos.y = (currentPos.y - previousPos.y) * screenRatio;
            transform.Translate(pos.x, pos.y, 0);
            previousPos = currentPos;
        }
#endif
    }

    public void ToggleUseKeyboard()
    {
        useKeyboard = !useKeyboard;
        if (useKeyboard)
        {
            autoFire = false;
            OnUpdateInput = UpdateInputKeyboard;
        }
        else
        {
            autoFire = true;
            OnUpdateInput = UpdateInputTouch;
        }
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

    protected void MoveDown()
    {
        if(transform.position.y - moveSpeed > -StageManager.instance.screenY)
        {
            transform.Translate(0, -moveSpeed, 0);
            previousPos = currentPos;
            currentPos = transform.position;
            movingDir = currentPos - previousPos;
        }
    }

    protected void MoveRight()
    {
        if (transform.position.x + moveSpeed < StageManager.instance.screenX)
        {
            transform.Translate(moveSpeed, 0, 0);
            previousPos = currentPos;
            currentPos = transform.position;
            movingDir = currentPos - previousPos;
        }
    }

    protected void MoveUp()
    {
        if (transform.position.y + moveSpeed < StageManager.instance.screenY)
        {
            transform.Translate(0, moveSpeed, 0);
            previousPos = currentPos;
            currentPos = transform.position;
            movingDir = currentPos - previousPos;
        }
    }

    protected void MoveLeft()
    {
        if (transform.position.x - moveSpeed > -StageManager.instance.screenX)
        {
            transform.Translate(-moveSpeed, 0, 0);
            previousPos = currentPos;
            currentPos = transform.position;
            movingDir = currentPos - previousPos;
        }
    }

    protected void IdleAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(!state.IsName("Idle"))
            animator.CrossFade("Idle", 0);
    }

    protected void DamageAnimation()
    {
        //AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        //if (!state.IsName("Damage"))
        //{
        //    animator.CrossFade("Damage", 0);
        //    state = animator.GetCurrentAnimatorStateInfo(0);
        //}
    }

    protected void TurnLeftAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("TurnLeft"))
            animator.CrossFade("TurnLeft", 0);
    }

    protected void TurnRightAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("TurnRight"))
            animator.CrossFade("TurnRight", 0);
    }

    protected void ExlosionAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Explosion"))
            animator.CrossFade("Explosion", 0);
    }

    protected void Shooting()
    {
        if(!autoFire)
        {
            for (int i = 0; i < activatedSpawnCount; i++)
            {
                bulletSpawns[i].Fire();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        if (!fsm.IsState(typeof(DeadState)) && !fsm.IsState(typeof(ReadyState)))
        {
            float dmg = 0;
            if (collision.gameObject.tag.Equals("Enemy"))
            {
                Plane enemy = collision.gameObject.GetComponent<Player>();
                if (enemy != null)
                    dmg = enemy.damage;

                DamageAnimation();
            }
            else if (collision.gameObject.tag.Equals("EnemyBullet"))
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

    //----------------------------------------------------------------------------------
    // FSM Classes
    public class ReadyState : BaseFSM<Player>
    {
        public override void Begin()
        {
            base.Begin();
            owner.DisableFire();
        }
        protected override void FixedUpdateFunc()
        {
        }
        public override void Finish()
        {
            base.Finish();
        }
   }

    public class MoveState : BaseFSM<Player>
    {
        //private Vector3 pos;

        public override void Begin()
        {
            base.Begin();
            owner.EnableFire();
        }

        protected override void FixedUpdateFunc()
        {
            owner.OnUpdateInput();
        }

        public override void Finish()
        {
            base.Finish();
        }

    }

    public class InvincibleState : BaseFSM<Player>
    {
        public override void Begin()
        {
            base.Begin();
        }

        protected override void FixedUpdateFunc()
        {
        }

        public override void Finish()
        {
            base.Finish();
        }        
    }

    public class DeadState : BaseFSM<Player>
    {
        public override void Begin()
        {
            base.Begin();
            owner.ExlosionAnimation();
            owner.StartCoroutine(DelayTime(1.5f));
        }

        public IEnumerator DelayTime(float t)
        {
            yield return new WaitForSeconds(t);
            owner.gameObject.SetActive(false);
            PlayManager.instance.GameOver();
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
