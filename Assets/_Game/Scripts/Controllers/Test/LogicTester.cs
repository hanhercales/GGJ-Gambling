using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;

public class LogicTester : MonoBehaviour
{
    [Header("Data References")]
    // Kéo thả tất cả Symbol SO vào đây
    public List<SymbolData> allSymbols;
    // Kéo thả tất cả Pattern SO vào đây
    public List<PatternData> allPatterns;

    [Header("Settings")]
    public int rows = 3;
    public int cols = 5;

    private GridModel _gridModel;
    private PatternEvaluator _evaluator;

    private void Start()
    {
        // Khởi tạo Logic Core
        _gridModel = new GridModel(allSymbols);
        _evaluator = new PatternEvaluator(allPatterns);
        
        Debug.Log("Logic Core Initialized. Press 'Space' to Simulate.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunSimulation();
        }
    }

    private void RunSimulation()
    {
        Debug.ClearDeveloperConsole();
        Debug.Log("--- START SIMULATION ---");

        // 1. Sinh Grid
        SymbolData[,] grid = _gridModel.GenerateMatrix(rows, cols);
        PrintGridToConsole(grid);

        // 2. Tính toán Pattern
        List<MatchResult> results = _evaluator.Evaluate(grid, cols, rows);

        // 3. In kết quả
        float totalScore = 0;
        if (results.Count == 0)
        {
            Debug.Log("<color=red>NO MATCH FOUND</color>");
        }
        else
        {
            foreach (var res in results)
            {
                float score = res.GetScore();
                totalScore += score;
                Debug.Log($"MATCH: <color=yellow>{res.pattern.patternName}</color> | Symbol: {res.symbol.idName} | Score: {score}");
            }
        }
        Debug.Log($"<b>TOTAL WIN: {totalScore}</b>");
    }

    // Hàm phụ trợ để in cái bảng ra Console cho dễ nhìn
    private void PrintGridToConsole(SymbolData[,] grid)
    {
        string log = "Grid Generated:\n";
        // Duyệt ngược từ hàng cao nhất xuống thấp nhất để in đúng chiều mắt nhìn
        for (int y = rows - 1; y >= 0; y--) 
        {
            string rowStr = "";
            for (int x = 0; x < cols; x++)
            {
                // In tên Symbol viết tắt
                string symName = grid[x, y].idName.Substring(0, 3); 
                rowStr += $"[{symName}] ";
            }
            log += rowStr + "\n";
        }
        Debug.Log(log);
    }
}