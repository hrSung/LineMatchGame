using System.Collections.Generic;
using UnityEngine;

public class TileLine : MonoBehaviour
{
    [SerializeField]
    private List<Tile> tiles;

    public int TileCount { get { return tiles.Count; } }
    public Vector3 CreatePosition { get { return GetTilePosition(TileCount - 1); } }


    #region MonoBehaviour Function

    void Awake()
    {

    }

    #endregion

    public Vector3 GetTilePosition(int idx)
    {
        return new Vector3(transform.localPosition.x, tiles[idx].transform.localPosition.y);
    }
}
