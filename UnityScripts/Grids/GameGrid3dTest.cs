using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameGrid3dTest : MonoBehaviour
{
    GameGrid3d gameGrid3D = new GameGrid3d();
    public GameObject starter;
    public GameObject ender;


    // Start is called before the first frame update
        Dictionary<GameGrid3dContent, Collider> gridMap = new Dictionary<GameGrid3dContent, Collider>();
    IEnumerator Start()
    {
        gameGrid3D.InitSize(128);

        foreach (var item in GetComponentsInChildren<Collider>())
        {
            var info =  item.gameObject.AddComponent<GameGridContentInfo>();
            info.gameGrid3D = gameGrid3D;
            info.gameGrid3DContent = gameGrid3D.GetBlock(item.transform.position);
            info.gameGrid3DContent.weight = 99;

            if (item is CapsuleCollider)
            {
                info.gameGrid3DContent.weight = -1;
            }

            gridMap.Add(info.gameGrid3DContent, item);
        }

            yield return new WaitForSeconds(3f);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in gridMap.Keys)
            {
                if (gridMap[item]!=null)
                {
                    gridMap[item].gameObject.GetComponent<Renderer>().material.color = Color.white;

                }
            }


            var starter_node = starter.GetComponent<GameGridContentInfo>().gameGrid3DContent;
            var end_node = ender.GetComponent<GameGridContentInfo>().gameGrid3DContent;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var paths = gameGrid3D.PathFindingGreedy(starter_node, end_node);
            sw.Stop();
            //获取运行时间间隔  
            TimeSpan ts = sw.Elapsed;

            UnityEngine.Debug.Log(ts.Milliseconds);
            foreach (var item in paths)
            {
                gridMap[item].gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }



}
