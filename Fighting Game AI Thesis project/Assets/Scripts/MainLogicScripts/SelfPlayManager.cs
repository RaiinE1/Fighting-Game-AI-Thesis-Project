using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfPlayManager : MonoBehaviour
{
    public AIBrain aIBrainRef;

    public SparringPartner sPartnerRef;

    public int TrainingCount = 50;
    public bool RunAtStart = true;
    

    public bool TrainingComplete = false;
    private bool TrainingInProgress = false;

    private int ExchangeCount =0;
    private int trainingWins = 0;
    private List<string> trainingLogs = new List<string>();

    void Start()
    {
        if(RunAtStart) StartTraining();
        else TrainingComplete = true;
    }

    public void StartTraining()
    {
        if(TrainingInProgress) return;
        CombatResolveManager.onExchangeResolved += HandleExchangeResult;
        Debug.Log($"Starting Training: Running - {TrainingCount} exchanges for warmup");
        trainingLogs.Add("=== SP Training Logs ===");
        trainingLogs.Add("--------------------------------------------------------------");
        StartCoroutine(TrainSessions());
    }

    private IEnumerator TrainSessions()
    {
        TrainingInProgress = true;
        TrainingComplete = false;
        ExchangeCount = 0;
        trainingWins = 0;
        
        yield return new WaitForSeconds(0.5f);
        sPartnerRef.StartSpar();
        while (ExchangeCount < TrainingCount)
        {
            yield return new WaitForSeconds(0.1f);
        }
        sPartnerRef.StopSpar();
        CombatResolveManager.onExchangeResolved -= HandleExchangeResult;

        TrainingInProgress = false;
        TrainingComplete = true;
        SessionSummary();
        WriteFile();
    }

    private void HandleExchangeResult(bool aiWon, ActionType aiAction, ActionType opponentSparAction)
    {
        ExchangeCount ++;
        if(aiWon)trainingWins++;
        sPartnerRef.ObserveAICounter(aiAction,aiWon);
        float WinRate = (float)trainingWins/ExchangeCount;
        //add log to relay information about the exchange
        string line = $"Exchange {ExchangeCount}/{TrainingCount}: " + $"[AI] {aiAction} vs [SPartner] {opponentSparAction}" + $"{(aiWon ?"A WINS":"SP WINS")} WinRate= {WinRate}";
        trainingLogs.Add(line);
        Debug.Log("[SELF_PLAY]" + line);
    }

    private void SessionSummary()
    {
        float Wrate = (ExchangeCount >0) ? (float)trainingWins/ExchangeCount : 0f;

        string summary = "\n[SELFPLAY SUMMARY]\n" +
                $" Exhchanges : {ExchangeCount}\n"+
                $" AI Wins :  {trainingWins}\n" +
                $" Win Rate : {Wrate}\n";
                trainingLogs.Add("---------------------------------------------------");
                trainingLogs.Add(summary);
                Debug.Log("[SELFPLAY]: "+ summary);
        
    }

    private void WriteFile()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath,$"SPM_SessionLog_{System.DateTime.Now:yyyy-mm-dd_HH-mm-ss}.txt");
        System.IO.File.WriteAllLines(path,trainingLogs);
        Debug.Log($"[SparringLogs] Logs saved to: {path}");
    }
    

    public bool IsPrepForPlayer()
    {
        return TrainingComplete && !TrainingInProgress;
    }

}
