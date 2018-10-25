using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageClearPanel : MonoBehaviour
{
    public GameObject clearText;
    public GameObject allClearText;

    public void ShowClearText()
    {
        clearText.SetActive(true);
        allClearText.SetActive(false);
    }

    public void ShowAllClearText()
    {
        clearText.SetActive(false);
        allClearText.SetActive(true);
    }
}
