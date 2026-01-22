using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandAlgs : MonoBehaviour
{
    public List<SymbolSO> allSymbols;
    
    public GridSystem gridSystem;

    private void Start()
    {
        GenerateSpin();
    }

    [ContextMenu("Spin")]
    public void GenerateSpin()
    {
        if (allSymbols == null || allSymbols.Count == 0) return;

        int totalWeight = 0;
        foreach (SymbolSO symbol in allSymbols) totalWeight += symbol.weight;

        for (int y = 0; y < GridSystem.height; y++)
        {
            for (int x = 0; x < GridSystem.width; x++)
            {
                SymbolSO selectedSymbol = GetWeightedSymbol(totalWeight);
                gridSystem[x, y] = selectedSymbol;
            }
        }

        LogGridMatrix();
    }

    private SymbolSO GetWeightedSymbol(int totalWeight)
    {
        int randomValue = Random.Range(0, totalWeight);

        foreach (SymbolSO symbol in allSymbols)
        {
            if (randomValue < symbol.weight)
            {
                return symbol;
            }
            randomValue -= symbol.weight;
        }
        
        return allSymbols[0];
    }
    
    private void LogGridMatrix()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== KẾT QUẢ SPIN (5x3) ===");

        for (int y = GridSystem.height - 1; y >= 0; y--)
        {
            sb.Append($"Row {y}: ");
            for (int x = 0; x < GridSystem.width; x++)
            {
                string name = gridSystem[x, y] != null ? gridSystem[x, y].symbolName : "NULL";
                sb.Append($"[ {name} ]\t"); 
            }
            sb.AppendLine();
        }
        
        Debug.Log(sb.ToString());
    }
}
