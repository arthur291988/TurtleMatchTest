using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPuller : MonoBehaviour
{
    public static ObjectPuller current;


    private int pullOfObjects100 = 100;
    private bool willGrow;


    [SerializeField]
    private GameObject Tiles;
    
    private List<GameObject> TilesList;


    private void Awake()
    {
        willGrow = true;
        current = this;
    }

    private void OnEnable()
    {
        TilesList = new List<GameObject>();


        for (int i = 0; i < pullOfObjects100; i++)
        {
            GameObject obj1 = Instantiate(Tiles);
            obj1.SetActive(false);
            TilesList.Add(obj1);

        }

    }

    public List<GameObject> GetTilePullList()
    {
        return TilesList;
    }



    //universal method to set active proper game object from the list of GOs, it just needs to get correct List of game objects
    public GameObject GetGameObjectFromPull(List<GameObject> GOLists)
    {
        for (int i = 0; i < GOLists.Count; i++)
        {
            if (!GOLists[i].activeInHierarchy) return GOLists[i];
        }
        if (willGrow)
        {
            GameObject obj = Instantiate(GOLists[0]);
            obj.SetActive(false);
            GOLists.Add(obj);
            return obj;
        }
        return null;
    }

}