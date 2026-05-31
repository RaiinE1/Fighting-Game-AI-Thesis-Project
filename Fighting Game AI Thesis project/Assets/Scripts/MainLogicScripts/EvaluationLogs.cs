using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Based on: NickGlenn. (n.d.). Unity3D [ShowOnly] Inspector Attribute. GitHub. https://gist.github.com/NickGlenn/84b8b43004a642b96ce9b6fef0bbcc8d
public class EvaluationLogs : MonoBehaviour
{
    //total predictions, correct predictions, accuracy of it in %
   [ShowOnly]public int  TotalPredictions = 0;
   [ShowOnly]public int CorrectPredicitons = 0;
   [ShowOnly]public float AiAccuracy = 0f; 
  

   [ShowOnly]public int AiWins = 0; //count of total wins made so far
   [ShowOnly]public int TotalRounds = 0;
   [ShowOnly]public float AiWinRate = 0f; // wins/total TotalRounds

   private int[] actionPredicted = new int[3];
   private int[] actionPCorrect = new int[3];
   private int[] AIactionWins = new int[3];
   private int[] AIactionTotal = new int[3];

   private ActionType LastPrediciton = ActionType.Neutral;
   private bool LoadedPrediction = false;
   private bool LastPCorrect = false;
   private ActionType LastPredictionUsed = ActionType.Neutral;

   private List<string> ExcelRows = new List<string>();

   void OnEnable()
    {
        AIBrain.PlayerActionPredicted += HandlePredict;
        PlayerController.PlayerActionAttempt += HandleActualPAction;

        ExcelRows.Add("Exchange No.;"+ "AiAction;"+"PlayerAction;"+"AiPrediction;" + "PredictionsCorrect;" + "AiWon;"+"AiAccuracy;"+"AiWinRate;");
    }
    void OnDisable()
    {
        AIBrain.PlayerActionPredicted -= HandlePredict;
        PlayerController.PlayerActionAttempt -= HandleActualPAction;
    }

    void OnApplicationQuit()
    {
        WriteFile();
    }

    private void HandlePredict(ActionType prediction)
    {
        LastPrediciton = prediction;
        LoadedPrediction = true;
    }  

    private void HandleActualPAction(ActionType actual)
    {
        if(!LoadedPrediction) return;
        TotalPredictions++ ;
        bool correct = actual == LastPrediciton;

        if(correct) CorrectPredicitons++;
        AiAccuracy = (float)CorrectPredicitons/TotalPredictions;

        int indx = (int)actual;
        if(indx >=0 && indx <= 2)
        {
            actionPredicted[indx]++;
            if(correct) actionPCorrect[indx]++;
        }

        LastPCorrect = correct;
        LastPredictionUsed = LastPrediciton;
        LoadedPrediction = false;
    }

    public void InteractionResults(bool aiWon, ActionType aiAction,ActionType playerAction)
    {
        TotalRounds ++;
        if(aiWon) AiWins++;
        AiWinRate = (float)AiWins/TotalRounds;
        int aiIndx = (int)aiAction;
        if(aiIndx >=0 && aiIndx <=2)
        {
            AIactionTotal[aiIndx]++;
            if(aiWon) AIactionWins[aiIndx]++;
        }
        string rows = string.Join(";",TotalRounds,aiAction,playerAction,
            LastPredictionUsed,LastPCorrect?"YES":"NO",aiWon?"YES":"NO",(AiAccuracy).ToString(),(AiWinRate).ToString());
        ExcelRows.Add(rows);
        Debug.Log($"[Logs] Exchange {TotalRounds}:{rows}");
    }

    private void WriteFile()
    {
        LogSummary();
        string path = Path.Combine(Application.persistentDataPath,$"PLAY_SessionLog_{System.DateTime.Now:yyyy-mm-dd_HH-mm-ss}.csv");
        System.IO.File.WriteAllLines(path,ExcelRows);
        Debug.Log($"SAVED LOGS: {path}");
        Application.OpenURL(Application.persistentDataPath);
    }

    private void LogSummary()
    {
       ExcelRows.Add("");
       ExcelRows.Add($"Total Exchanges;{TotalRounds}");
       ExcelRows.Add($"Total Predictions;{TotalPredictions}");
       ExcelRows.Add($"Correct Prediction;{CorrectPredicitons}");
       ExcelRows.Add($"Accuracy(%);{AiAccuracy}");
       ExcelRows.Add($"Total AI Wins;{AiWins}");
       ExcelRows.Add($"Win Rate(%); {AiWinRate}");
       ExcelRows.Add("");
       ExcelRows.Add("Action Prediction Accuracy");
       ExcelRows.Add("Action;No. of Predictions;Times Correct;Accuracy");
       ExcelRows.Add(EachActionPredictionRow("Punch",0));
       ExcelRows.Add(EachActionPredictionRow("Kick",1));
       ExcelRows.Add(EachActionPredictionRow("Block",2));
       ExcelRows.Add("");
       ExcelRows.Add($"Action success rate");
       ExcelRows.Add($"Ai Actions;Times Used; Times Won; Win Rate");
       ExcelRows.Add(EachActionWinsRow("Punch",0));
       ExcelRows.Add(EachActionWinsRow("Kick",1));
       ExcelRows.Add(EachActionWinsRow("Block",2));
    }

    private string EachActionPredictionRow(string label, int index)
    {
        int predicted = actionPredicted[index];
        int correct = actionPCorrect[index];
        float accuracy = (predicted >0)? (float)correct/predicted  : 0f;
        return $"{label};{predicted};{correct};{accuracy}";
    }

    private string EachActionWinsRow(string label, int index)
    {
        int total = AIactionTotal[index];
        int wins = AIactionWins[index];
        float winRate = (total >0)? (float)wins/total  : 0f;
        return $"{label};{total};{wins};{winRate}";
    }
}
