using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private SymbolSO[] symbols;

    public const int width = 5;
    public const int height = 3;

    private void Awake()
    {
        InitiateGrid();
    }

    public void InitiateGrid()
    {
        symbols = new SymbolSO[width * height];
    }
    
    public SymbolSO this[int x, int y]
    {
        get
        {
            if(!IsValid(x, y)) return null;
            return symbols[GetIndex(x, y)];
        }
        set
        {
            if(IsValid(x, y)) symbols[GetIndex(x, y)] = value;
        }
    }

    private int GetIndex(int x, int y)
    {
        return y * width + x;
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public List<SymbolSO> GetNeighbours(int  x, int y)
    {
        List<SymbolSO> neighbours = new List<SymbolSO>();
        
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { 1, 1, 1, 0, 0, -1, -1, -1 };

        for (int i = 0; i < 8; ++i)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsValid(nx, ny) && this[nx, ny] != null)
            {
                neighbours.Add(this[nx, ny]);
            }
        }
        return neighbours;
    }
}
