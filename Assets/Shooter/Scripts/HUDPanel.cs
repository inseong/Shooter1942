using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{
    public Text score;
    public Text highScore;
    public Text lifeCount;
    public Text killCount;

    public void UpdateScores(int sc, int hsc, int life)
    {
        score.text = sc.ToString();
        highScore.text = hsc.ToString();
        lifeCount.text = life.ToString();
    }

    public void UpdateKillCount(int bossKill, int fighterKill)
    {
        killCount.text = string.Format("B:{0}, F:{1}", bossKill, fighterKill);
    }

}
