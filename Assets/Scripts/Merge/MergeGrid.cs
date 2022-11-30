using UnityEngine;


[System.Serializable]
public class TileData
{
    [System.Serializable]
    public struct rowData
    {
        public Transform[] row;
    }

    public rowData[] rows;
}
public class MergeGrid : MonoBehaviour
{
    [SerializeField] private TileData _grid;

    public TileData Grid { get => _grid;}



    public Transform GetCurrentCellByIndexes(int z, int x)
    {
        return _grid.rows[z].row[x];
    }
}
