using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerInfo
{
    public GameObject player;
    public float energy;
    public int lifeCount;
}

[Serializable]
public struct WinningCondition
{
    public int bossKills;
    public int fighterKills;
    public int winingScore;
}

[Serializable]
public struct HighScore
{
    public string name;
    public int score;
}

[Serializable]
public struct SpawnData
{
    public GameObject plane;
    public int count;
    public int spawnIndex;          // an index of spawn objects in StageManager object.
    public float spawnOffset;       // how far is from an original spawn position.
    public float interval;
    public float energy;
    public float speed;
}

[Serializable]
public struct WaveData
{
    public int distance;                        // Distance in metric of a wave from a starting point. 
    public List<SpawnData> spawnList;           // These spawn items in the list generate its gameobjects simultaniously.

    public float timeLimit;                     // Time limitaton for this Wave
}

[Serializable]
public struct BackgroundGroup
{
    public string name;
    public List<GameObject> backList;
}

[Serializable]
public struct MapData
{
    public string bgGroupName;
    public int repeatCount;             // if this value is 0, it means the infinite loop.
}

[Serializable]
public struct StageInfo
{
    public List<MapData> mapList;
    public List<WaveData> waveList;
    public WinningCondition winningCondition;   // Winning conditions for this stage
}

[Serializable]
public class GameData : ScriptableObject
{
    private static GameData _instance;
    public static GameData instance
    {
        get
        {
            if (_instance == null)
            {
                string dataFileName = StageManager.instance.gameDataName;

#if UNITY_EDITOR
                string path = "Assets/Shooter/Resources/" + dataFileName + ".asset";
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) as GameData;
#else
                string path = dataFileName+".asset";
                string dir = System.IO.Path.GetDirectoryName(path);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (dir.Length > 0)
                    _instance = Resources.Load(dir + "/" + fileName, typeof(GameData)) as GameData;
                else
                    _instance = Resources.Load(fileName, typeof(GameData))as GameData ;
#endif
            }

            return _instance;
        }
    }

    public int highScoreLimitation = 10;
    public PlayerInfo playerInfo;
    public BackgroundGroup[] backGrounds;
    public StageInfo[] stageList;

    //[HideInInspector]
    public List<HighScore> highScores;

    public List<GameObject> GetBackgroundObjectList(string groupName)
    {
        for (int i = 0; i < backGrounds.Length; i++)
        {
            if (backGrounds[i].name.Equals(groupName))
                return backGrounds[i].backList;
        }

        return null;
    }

    public int InputHighScore(string playerName, int score)
    {
        int index;

        if (highScores == null)
            highScores = new List<HighScore>();
            

        for (index = 0; index < highScores.Count; index++)
        {
            if (score > highScores[index].score)
                break;
        }

        if(index < highScoreLimitation)
        {
            HighScore newHighScore = new HighScore();
            newHighScore.name = playerName;
            newHighScore.score = score;

            highScores.Insert(index, newHighScore);
        }

        if (highScores.Count > highScoreLimitation)
        {
            for (int i = highScores.Count-1; i >= highScoreLimitation; i--)
            highScores.RemoveAt(i);
        }

        return index;
    }

    public int GetHighestScore()
    {
        if (highScores == null || highScores.Count == 0)
            return 0;
        
        return highScores[0].score;
    }
}