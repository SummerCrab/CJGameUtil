using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum GridContentType
{ 
   conner,
   bottom_edge,
   bottom_face,
   left_edge,
   left_face,
   foward_edge,
   front_face,
   block,
}

public enum GridContentState
{
    link = -1,
    empty,//空
    bloking,//地面，障碍等不可通行区域
    filled,//草丛，水等可通行区域
}

public enum PathFindingType
{
    Greedy,
    Astar,
}


/// <summary>
/// 网格规划的基本空间单位
/// </summary>
public class GameGrid3dContent : FastPriorityQueueNode
{
    public int weight;
    public Vector3Int realIndex;
    public GridContentState GetState()
    {
        if (weight < 0) return GridContentState.link;
        if (weight ==0) return GridContentState.empty;      
        if (weight >98)return GridContentState.bloking;       
        return GridContentState.filled;
    }
}

/// <summary>
/// unity坐标系，从(0,0,0)开始，三维立方体空间，暂不支持负数
/// </summary>
public class GameGrid3d 
{
    GameGrid3dContent[,,] _contents;
    Vector3 _unit = Vector3.one;
    float _percent = 0.1f;

    public void InitWithContents(GameGrid3dContent[,,] contents, float percent = 0f)
    {
        _percent = percent;
        _contents = contents;
    }

    public void InitSize(int size,float percent = 0f)
    {
        Vector3Int size3d = new Vector3Int(
            Mathf.Clamp(size, 1, 256),
            10,
            Mathf.Clamp(size, 1, 256));

        _percent = percent;
        _contents = new GameGrid3dContent[size3d.x * 2+1, size3d.y * 2+1, size3d.z * 2+1];

        for (int i = 0; i < _contents.GetLength(0); i++)
        {
            for (int j = 0; j < _contents.GetLength(1); j++)
            {
                for (int k = 0; k < _contents.GetLength(2); k++)
                {
                    _contents[i, j, k] = new GameGrid3dContent();
                    _contents[i, j, k].realIndex = new Vector3Int(i, j, k);
                }
            }
        }

    }

    public GameGrid3dContent GetBlock(Vector3Int index)
    {
        return _contents[index.x * 2+1, index.y * 2+1, index.z * 2+1];
    }

    public GameGrid3dContent GetContent(Vector3Int index, GridContentType type = GridContentType.block)
    {
        switch (type)
        {
            case GridContentType.conner:
                return  _contents[index.x * 2, index.y * 2, index.z * 2];
            case GridContentType.bottom_edge:
                return _contents[index.x * 2 + 1, index.y * 2, index.z * 2];
            case GridContentType.bottom_face:
                return _contents[index.x * 2 + 1, index.y * 2, index.z * 2+1];
            case GridContentType.left_edge:
                return _contents[index.x * 2 , index.y * 2+1, index.z * 2];
            case GridContentType.left_face:
                return _contents[index.x * 2, index.y * 2 + 1, index.z * 2 + 1];
            case GridContentType.foward_edge:
                return _contents[index.x * 2, index.y * 2, index.z * 2 +1];
            case GridContentType.front_face:
                return _contents[index.x * 2 + 1, index.y * 2 + 1, index.z * 2];
        }
        return GetBlock(index);
    }

    Vector3Int _tempIndex = Vector3Int.zero;
    Vector3Int _tempAddtion = Vector3Int.zero;
    public GameGrid3dContent GetBlock(Vector3 localPos)
    {
        try
        {
            _tempIndex = GetIndex(localPos);
            _tempAddtion = GetAddition(localPos);
            return _contents[_tempIndex.x * 2 + _tempAddtion.x, _tempIndex.y * 2 + _tempAddtion.y, _tempIndex.z * 2 + _tempAddtion.z];
        }
        catch (System.Exception)
        {

            Debug.LogErrorFormat(" localPos {0}  _tempIndex {1}", localPos, _tempIndex);
            return _contents[0, 0, 0];
        }
    
    }

    /// <summary>
    /// 获得块的局部坐标
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetLocation(Vector3Int index)
    {
        float block_x = index.x * _unit.x + (_percent + 1.0f) * _unit.x * 0.5f;
        float block_y = index.y * _unit.y + (_percent + 1.0f) * _unit.y * 0.5f;
        float block_z = index.z * _unit.z + (_percent + 1.0f) * _unit.z * 0.5f;
        return new Vector3(block_x, block_y, block_z);
    }

    /// <summary>
    /// 获取当前局部点对应的块的位置
    /// </summary>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public Vector3 GetLocation(Vector3 localPos)
    {
        return GetLocation(GetIndex(localPos));
    }

    /// <summary>
    /// 获取整个图的局部中心
    /// </summary>
    /// <returns></returns>
    //public Vector3 GetCenterLocation()
    //{

    //}


    public Vector3Int GetIndex(GameGrid3dContent content)
    {
        return new Vector3Int(content.realIndex.x >> 1, content.realIndex.y >> 1, content.realIndex.z >> 1);
    }

    public Vector3Int GetIndex(Vector3 localPos)
    {
        _tempIndex.x = Mathf.FloorToInt(localPos.x / _unit.x);
        _tempIndex.y = Mathf.FloorToInt(localPos.y / _unit.y);
        _tempIndex.z = Mathf.FloorToInt(localPos.z / _unit.z);
        return _tempIndex;
    }

    public bool ContainsIndex(Vector3Int index)
    {
        if (index.x > _contents.GetLength(0)>>1 - 1 || index.x < 0) return false;
        if (index.y > _contents.GetLength(1)>>1 - 1 || index.y < 0) return false;
        if (index.z > _contents.GetLength(2)>>1 - 1 || index.z < 0) return false;
        return true;
    }

    public Vector3Int GetAddition(Vector3 localPos)
    {
        _tempAddtion.x = (localPos.x - _tempIndex.x) > _percent ? 1 : 0;
        _tempAddtion.y = (localPos.y - _tempIndex.y) > _percent ? 1 : 0;
        _tempAddtion.z = (localPos.z - _tempIndex.z) > _percent ? 1 : 0;
        return _tempAddtion;
    }

    List<Vector3Int> _neighbor_directions = new List<Vector3Int>
    {
         new Vector3Int(0,0,1),
         new Vector3Int(0,0,-1),
         Vector3Int.left,
         Vector3Int.right,
    };

    List<Vector3Int> _neighbor_updown = new List<Vector3Int>
    {
         Vector3Int.up,
         Vector3Int.down,
    };

    /// <summary>
    /// 寻找路面和楼梯等路径
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public List<GameGrid3dContent> GetNeighborsOnGroundWithLink(GameGrid3dContent current)
    {
        List<GameGrid3dContent> neighbors = new List<GameGrid3dContent>();
        var index = GetIndex(current);
        foreach (var dir in _neighbor_directions)
        {
            var neighbor_index = index + dir;
            if (ContainsIndex(neighbor_index))
            {
                var neighbor = GetBlock(neighbor_index);
                //搜寻地表块（上层不为bloking的可通行块）
                if (neighbor.GetState() != GridContentState.empty)
                {
                    if (GetBlock(neighbor_index + Vector3Int.up).GetState()!= GridContentState.bloking)
                    {
                        neighbors.Add(GetBlock(neighbor_index));
                    }
                }
            }
        }

        //搜索上下的link块
        foreach (var dir in _neighbor_updown)
        {
            var neighbor_index = index + Vector3Int.up;
            if (ContainsIndex(neighbor_index))
            {
                var neighbor = GetBlock(neighbor_index);
                if (neighbor.GetState() == GridContentState.link) neighbors.Add(GetBlock(neighbor_index));
            }

            if (current.GetState() == GridContentState.link)
            {
                neighbor_index = index + Vector3Int.down;
                if (ContainsIndex(neighbor_index))
                {
                    var neighbor = GetBlock(neighbor_index);
                    if (neighbor.GetState() == GridContentState.link) neighbors.Add(GetBlock(neighbor_index));
                    if (neighbor.GetState() == GridContentState.bloking) neighbors.Add(GetBlock(neighbor_index));
                }
            }
            break;
        }

        return neighbors;
    }

    //def heuristic(a, b):
    //# Manhattan distance on a square grid
    //return abs(a.x - b.x) + abs(a.y - b.y)
    //启发式探索算法
    private float Heuristic(GameGrid3dContent c1, GameGrid3dContent c2)
    {
        return Mathf.Abs(c1.realIndex.x - c2.realIndex.x) + Mathf.Abs(c1.realIndex.y - c2.realIndex.y) + Mathf.Abs(c1.realIndex.z - c2.realIndex.z);
    }

    /// <summary>
    /// 贪婪/快速寻路算法,返回一个首尾为传入参数的list,如果尾部不同，则说明没有寻找到路径
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public List<GameGrid3dContent> PathFindingGreedy(GameGrid3dContent start, GameGrid3dContent goal)
    {
        List<GameGrid3dContent> path = new List<GameGrid3dContent>();
        FastPriorityQueue<GameGrid3dContent> frontier = new FastPriorityQueue<GameGrid3dContent>(512);
        frontier.Enqueue(start, 0f);
        Dictionary<GameGrid3dContent, GameGrid3dContent> came_from = new Dictionary<GameGrid3dContent, GameGrid3dContent>();
     
        came_from.Add(start, null);
        while (frontier.Count>0)
        {
            var current = frontier.Dequeue();        
            if (current==goal) break;
            foreach (var next in GetNeighborsOnGroundWithLink(current))
            {
                if (!came_from.ContainsKey(next))
                {
                    frontier.Enqueue(next, Heuristic(goal, next));
                    came_from.Add(next, current);
                }
            }
        }

        var node = goal;
        while (node != start)
        {
            path.Add(node);
            if (!came_from.TryGetValue(node, out node))break;                            
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

}
