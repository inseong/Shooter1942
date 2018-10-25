using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Player player;
    public Canvas canvasTitle;
    public Canvas canvasReady;
    public Canvas canvasHUD;
    public Canvas canvasStageClear;
    public Canvas canvasGameOver;
    public HUDPanel hudPanel;
    public int lifeCount = 3;

    private int currentStage = 0;

    [HideInInspector]
    public int totalScore = 0;

    [HideInInspector]
    public int highScore = 0;

    private WinningCondition winningCount;               // Winning count to check a winning condition

    private static PlayManager _instance;
    public static PlayManager instance
    {
        get { return _instance; }
    }

    public FSM<PlayManager> fsm;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        fsm = new FSM<PlayManager>(this);
    }

    void Start ()
    {
        fsm.ChangeState(typeof(TitleState));
	}
	
	void Update ()
    {
        fsm.currentState.Update();		
	}

    void FixedUpdate()
    {
        fsm.currentState.FixedUpdate();
    }

    void LateUpdate()
    {
        fsm.currentState.LateUpdate();
    }

    public void AddFighterCount()
    {
        winningCount.fighterKills++;
        hudPanel.UpdateKillCount(winningCount.bossKills, winningCount.fighterKills);

        if (StageManager.instance.IsWinned(winningCount))
            StageClearMode();
    }

    public void AddBossCount()
    {
        winningCount.bossKills++;
        hudPanel.UpdateKillCount(winningCount.bossKills, winningCount.fighterKills);

        if (StageManager.instance.IsWinned(winningCount))
            StageClearMode();
    }

    public void AddScoreCount(int score)
    {
        winningCount.winingScore += score;
        if (StageManager.instance.IsWinned(winningCount))
            StageClearMode();
    }

    public void ClearWinningCount()
    {
        winningCount.fighterKills = 0;
        winningCount.bossKills = 0;
        winningCount.winingScore = 0;
    }

    public void PlayGame()
    {
        ClearWinningCount();

        lifeCount = GameData.instance.playerInfo.lifeCount;
        player.energy = GameData.instance.playerInfo.energy;

        currentStage = 0;
        StageManager.instance.LoadStage(currentStage);

        totalScore = 0;
        highScore = GameData.instance.GetHighestScore();
        hudPanel.UpdateScores(totalScore, highScore, lifeCount);
        hudPanel.UpdateKillCount(winningCount.bossKills, winningCount.fighterKills);

        fsm.ChangeState(typeof(ReadyState));
    }

    public void RevivalGame()
    {
        ClearWinningCount();
        player.energy = GameData.instance.playerInfo.energy;
        hudPanel.UpdateScores(totalScore, highScore, lifeCount);
        hudPanel.UpdateKillCount(winningCount.bossKills, winningCount.fighterKills);

        fsm.ChangeState(typeof(ReadyState));
    }

    public void NextStage()
    {
        ClearWinningCount();

        currentStage++;
        StageManager.instance.LoadStage(currentStage);

        fsm.ChangeState(typeof(ReadyState));
    }

    public void GameOver()
    {
        if (--lifeCount <= 0)
            fsm.ChangeState(typeof(GameOverState));
        else
            RevivalGame();
    }

    public void AddScore(int score)
    {
        totalScore += score;
        if (totalScore >= highScore)
            highScore = totalScore;

        hudPanel.UpdateScores(totalScore, highScore, lifeCount);
        hudPanel.UpdateKillCount(winningCount.bossKills, winningCount.fighterKills);
    }

    protected void TitleMode()
    {
        fsm.ChangeState(typeof(TitleState));
    }

    protected void ReadyMode()
    {
        fsm.ChangeState(typeof(ReadyState));
    }

    protected void PlayMode()
    {
        fsm.ChangeState(typeof(PlayState));
    }

    protected void StageClearMode()
    {
        fsm.currentState.Finish();
    }

    protected void GameOverMode()
    {
        fsm.ChangeState(typeof(GameOverState));
    }

    protected void TitleView()
    {
        player.gameObject.SetActive(false);
        canvasTitle.gameObject.SetActive(true);
        canvasReady.gameObject.SetActive(false);
        canvasHUD.gameObject.SetActive(false);
        canvasStageClear.gameObject.SetActive(false);
        canvasGameOver.gameObject.SetActive(false);
    }

    protected void ReadyView()
    {
        player.gameObject.SetActive(true);
        canvasTitle.gameObject.SetActive(false);
        canvasReady.gameObject.SetActive(true);
        canvasHUD.gameObject.SetActive(true);
        canvasStageClear.gameObject.SetActive(false);
        canvasGameOver.gameObject.SetActive(false);
    }

    protected void PlayView()
    {
        canvasTitle.gameObject.SetActive(false);
        canvasReady.gameObject.SetActive(false);
        canvasHUD.gameObject.SetActive(true);
        canvasStageClear.gameObject.SetActive(false);
        canvasGameOver.gameObject.SetActive(false);
    }

    protected void StageClearView()
    {
        canvasTitle.gameObject.SetActive(false);
        canvasReady.gameObject.SetActive(false);
        canvasHUD.gameObject.SetActive(true);
        canvasStageClear.gameObject.SetActive(true);
        canvasGameOver.gameObject.SetActive(false);
    }

    protected void GameOverView()
    {
        GameData.instance.InputHighScore(player.name, totalScore);

        canvasTitle.gameObject.SetActive(false);
        canvasReady.gameObject.SetActive(false);
        canvasHUD.gameObject.SetActive(false);
        canvasStageClear.gameObject.SetActive(false);
        canvasGameOver.gameObject.SetActive(true);
    }

    //----------------------------------------------------------------------------------
    // FSM Classes

    public class TitleState : BaseFSM<PlayManager>
    {
        public override void Begin()
        {
            base.Begin();
            owner.TitleView();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }

    public class ReadyState : BaseFSM<PlayManager>
    {
        public override void Begin()
        {
            base.Begin();
            owner.ReadyView();
            owner.player.ReadyMode();
            owner.StartCoroutine(DelayTime(1.5f));
        }

        public IEnumerator DelayTime(float t)
        {
            yield return new WaitForSeconds(t);
            owner.PlayMode();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }

    public class PlayState : BaseFSM<PlayManager>
    {
        private float dist;
        public override void Begin()
        {
            base.Begin();
            owner.PlayView();
            owner.player.MoveMode();
        }

        public override void Finish()
        {
            owner.StartCoroutine(DelayedFinish(3.0f));
            //base.Finish();
        }

        IEnumerator DelayedFinish(float t)
        {
            yield return new WaitForSeconds(t);
            owner.fsm.ChangeState(typeof(StageClearState));
        }

        protected override void FixedUpdateFunc()
        {
            if(!StageManager.instance.IsLoading)
                StageManager.instance.FixedUpdateBackground();

            StageManager.instance.GenerateWave();
        }
    }

    public class StageClearState : BaseFSM<PlayManager>
    {
        public override void Begin()
        {
            base.Begin();
            owner.StageClearView();

            StageClearPanel panel = owner.canvasStageClear.GetComponent<StageClearPanel>();
            if (panel != null)
            {
                if (owner.currentStage+1 < GameData.instance.stageList.Length)
                {
                    panel.ShowClearText();
                    owner.StartCoroutine(PlayNextStage(1.5f));
                }
                else
                {
                    panel.ShowAllClearText();  
                    owner.StartCoroutine(GoToGameOver(1.5f));
                }
            }
        }

        public IEnumerator PlayNextStage(float t)
        {
            yield return new WaitForSeconds(t);
            owner.NextStage();
        }

        public IEnumerator GoToGameOver(float t)
        {
            yield return new WaitForSeconds(t);
            owner.GameOverMode();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }

    public class GameOverState : BaseFSM<PlayManager>
    {
        public override void Begin()
        {
            base.Begin();
            owner.player.ReadyMode();
            owner.GameOverView();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
