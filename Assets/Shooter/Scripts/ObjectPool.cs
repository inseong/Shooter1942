using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Dictionary<string, List<GameObject>> objectPool = new Dictionary<string, List<GameObject>>();
   
    private static ObjectPool _instance;
    public static ObjectPool instance
    {
        get { return _instance; }
    }
	
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public GameObject GetPooledObject(GameObject obj)
    {
        GameObject newObj = null;
        List<GameObject> objList;

        if(obj)
        {
            if(objectPool.ContainsKey(obj.name))
            {
                objList = objectPool[obj.name];
                if (objList != null && objList.Count > 0)
                {
                    newObj = objList[0];
                    objList.RemoveAt(0);
                }
                else
                {
                    newObj = Instantiate(obj);
                    newObj.name = obj.name;
                }
            }
            else
            {
                newObj = Instantiate(obj);
                newObj.name = obj.name;
            }

            newObj.SetActive(true);
        }

        return newObj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {

        if(!objectPool.ContainsKey(obj.name))
            objectPool.Add(obj.name, new List<GameObject>());

        objectPool[obj.name].Add(obj);
        obj.SetActive(false);
    }
}
