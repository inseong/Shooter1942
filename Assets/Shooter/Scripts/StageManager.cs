using System;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public string gameDataName = "GameData";
    public float cameraSize = 190;                  // default camera size by vertical
    public float backgroundWidth = 224;             // default width value of the base background image, in pixel.
    public float scrollingSpeed = 40;
    public int UnitPerOneMeter = 2;                 // A spawn point of enemy's planes in GameData is defined by n meter.
                                                    //   This value means a screen unit per meter. In this game, 1 meter is 2 units(pixels).
    public EnemySpawn [] planeSpawns;

    [HideInInspector]
    public float screenX, screenY;

    [HideInInspector]
    public Camera mainCamera;

    private LinkedList<GameObject> bgLinkedList;

    private int mapNo;
    private int objectNo;
    private int repeatNo;
    private Vector3 movePos;
    private StageInfo currentStageInfo;

    private float distance;                           // total running distance
    private int currentWave;

    private bool loading = true;
    public bool IsLoading
    {
        get { return loading;  }
    }
    private static StageManager _instance;
    public static StageManager instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        bgLinkedList = new LinkedList<GameObject>();

        movePos = new Vector3();
    }

	void Start ()
    {
        mainCamera = Camera.main;
        CorrectCameraSize();
        CorrectSpawnPostion();
	}

    public void GenerateWave()
    {
        distance += Time.deltaTime * scrollingSpeed;
        if (currentWave < currentStageInfo.waveList.Count)
        {
            WaveData wave = currentStageInfo.waveList[currentWave];
            if(distance >= wave.distance)
            {
                ++currentWave;
                distance = 0;
                SpawnData spawn;
                for (int i = 0; i < wave.spawnList.Count; i++)
                {
                    spawn = wave.spawnList[i];
                    if (spawn.spawnIndex < planeSpawns.Length)
                    {
                        planeSpawns[spawn.spawnIndex].SpawnningStart(spawn); 
                    }
                }
            }
        }
    }

    public void FixedUpdateBackground()
    {
        float m = Time.deltaTime * scrollingSpeed;
        GameObject obj;

        // if the top position of the last node enter to the screen scope, new one would be added.
        LinkedListNode<GameObject> schNode = bgLinkedList.Last;

        Sprite sprite = schNode.Value.GetComponent<SpriteRenderer>().sprite;
        if(schNode.Value.transform.position.y + sprite.rect.height / 2.0f - m <= screenY)
        {
            AddNextBackground(schNode.Value.transform.position.y + sprite.rect.height / 2.0f);
        }

        // if the top position of the first node is out of screen scope, it would be removed.
        schNode = bgLinkedList.First;

        sprite = schNode.Value.GetComponent<SpriteRenderer>().sprite;
        if(schNode.Value.transform.position.y + sprite.rect.height/2.0f < -screenY)
        {
            ObjectPool.instance.ReturnObjectToPool(schNode.Value);
            bgLinkedList.RemoveFirst();
            schNode = bgLinkedList.First;
        }

        while (schNode != null)
        {
            obj = schNode.Value;
            obj.transform.Translate(0, -m, 0);
            schNode = schNode.Next;
        }
    }

    // correcting a camera size to fit the width size of BG images.
    protected void CorrectCameraSize()
    {
        if(GameData.instance.backGrounds.Length > 0 && GameData.instance.backGrounds[0].backList.Count > 0)
        {
            GameObject obj = GameData.instance.backGrounds[0].backList[0];
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr && sr.sprite)
                backgroundWidth = sr.sprite.rect.width;
        }

        float h = backgroundWidth * (float)Screen.height / (float)Screen.width;

        screenX = backgroundWidth / 2.0f;
        screenY = h / 2.0f;

        if (mainCamera)
            mainCamera.orthographicSize = screenY;
    }

    // correcting all spawn positions since some spawns may be in screen scope. (by Y-axis)
    public void CorrectSpawnPostion()
    {
        Vector3 v;

        for (int i = 0; i < planeSpawns.Length; i++)
        {
            v = planeSpawns[i].transform.position;
            if (v.y > 0 && v.y < screenY)
            {
                v.y = screenY;
                planeSpawns[i].transform.position = v;
            }
            else if (v.y < 0 && v.y > -screenY)
            {
                v.y = -screenY;
                planeSpawns[i].transform.position = v;
            }
        }
    }

    public void LoadStage(int stageNum)
    {
        loading = true;

        InitStage();

        if(GameData.instance.stageList.Length > 0)
        {
            mapNo = 0;
            objectNo = 0;
            repeatNo = 0;
            distance = 0;
            currentWave = 0;

            currentStageInfo = GameData.instance.stageList[stageNum];

            float h = 0;
            float y = -screenY;
            float spriteH;
 
            while(h <= screenY*2)
            {
                spriteH = AddNextBackground(y);
 
                y += spriteH;
                h += spriteH;
            }
        }
        loading = false;
    }

    public void InitStage()
    {
        LinkedListNode<GameObject> schNode = bgLinkedList.First;

        while (schNode != null)
        {
            ObjectPool.instance.ReturnObjectToPool(schNode.Value);
            schNode = schNode.Next;
        }

        bgLinkedList.Clear();

        GameObject [] objectList = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < objectList.Length; i++)
            ObjectPool.instance.ReturnObjectToPool(objectList[i]);
        
        objectList = GameObject.FindGameObjectsWithTag("PlayerBullet");
        for (int i = 0; i < objectList.Length; i++)
            ObjectPool.instance.ReturnObjectToPool(objectList[i]);

        objectList = GameObject.FindGameObjectsWithTag("EnemyBullet");
        for (int i = 0; i < objectList.Length; i++)
            ObjectPool.instance.ReturnObjectToPool(objectList[i]);
    }

    public float AddNextBackground(float previousY)
    {
        GameObject obj;
        GameObject newObj;
        Sprite sprite;

        obj = GetNextBackgroundObject(currentStageInfo);
        newObj = ObjectPool.instance.GetPooledObject(obj);

        sprite = newObj.GetComponent<SpriteRenderer>().sprite;
        movePos.Set(0, previousY + sprite.rect.height / 2.0f, 0);
        newObj.transform.position = movePos;

        bgLinkedList.AddLast(newObj);

        return sprite.rect.height;
    }

    public GameObject GetNextBackgroundObject(StageInfo stageInfo)
    {
        MapData mapData;
        List<GameObject> backList;

        mapData = stageInfo.mapList[mapNo];
        backList = GameData.instance.GetBackgroundObjectList(mapData.bgGroupName);

        // if one cycle of background images was finished,
        if (objectNo >= backList.Count)
        {
            objectNo = 0;

            // Checking whether this map is an infinity loop or not.
            //    mapData.repeatCount == 0 means this map items is looping infinitely.
            if (mapData.repeatCount > 0 && ++repeatNo >= mapData.repeatCount)
            {
                repeatNo = 0;

                // if the next map number > a size of the map list, assign 0 to mapNo.
                if (++mapNo >= stageInfo.mapList.Count)
                    mapNo = 0;

                mapData = stageInfo.mapList[mapNo];
                backList = GameData.instance.GetBackgroundObjectList(mapData.bgGroupName);
            }
        }

        return backList[objectNo++];
    }

    public bool IsWinned(WinningCondition p)
    {
        bool bossResult = false;
        bool fighterResult = false;
        bool scoreResult = false;

        WinningCondition w = currentStageInfo.winningCondition;
        if (w.bossKills == 0 || p.bossKills >= w.bossKills)
            bossResult = true;
        
        if (w.fighterKills == 0 || p.fighterKills >= w.fighterKills)
            fighterResult = true;
        
        if (w.winingScore == 0 || p.winingScore >= w.winingScore)
            scoreResult = true;

        return (bossResult && fighterResult && scoreResult);
    }
}
