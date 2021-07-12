using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridContentInfo : MonoBehaviour
{
    public GameGrid3d gameGrid3D;
    public GameGrid3dContent gameGrid3DContent;
    public Vector3Int index;
    public Vector3Int real;

    private void Start()
    {
        index = gameGrid3D.GetIndex(gameGrid3DContent);
        real = gameGrid3DContent.realIndex;
        name = index.ToString();
        //gameGrid3DContent.weight = 99;
    }

    private void OnDestroy()
    {
        gameGrid3DContent.weight = 0;
    }


}
