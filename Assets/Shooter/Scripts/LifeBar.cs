using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LifeBar : MonoBehaviour
{
    public float lifeAmount = 100;
    public float lifeBarSize = 20;          // by a screen unit

    private SpriteRenderer lifeBar;
    private float lifeScale;
    private float spriteWidth = 4;
    private Vector3 barScale;

    public void UpdateLifeBar(float life)
    {
        if(lifeBar == null)
        {
            lifeBar = GetComponent<SpriteRenderer>();
            spriteWidth = lifeBar.sprite.rect.width;
        }

        if(lifeBar != null)
        {
            float lifeRatio = life / lifeAmount;
            lifeScale = lifeBarSize / spriteWidth * lifeRatio;
            lifeBar.material.color = Color.Lerp(Color.green, Color.red, 0.6f - lifeRatio);
            lifeBar.transform.localScale = new Vector3(lifeScale, 1, 1);
        }
    }
}
