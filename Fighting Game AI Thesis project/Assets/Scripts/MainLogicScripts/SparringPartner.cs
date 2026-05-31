using System;
using System.Collections;
using UnityEngine;

public class SparringPartner : MonoBehaviour
{
    public AIBrain aIBrainRef;
    public PlayerController playerRef;

    public float MoveInterval = 1.5f;
    public float RandomInfluence = 0.3f;
    private float [,] SparringMatrix = new float[3,3];
    private ActionType lastSparringAction = ActionType.Neutral;
    private ActionType previousSparringAction = ActionType.Neutral;
    private bool isRunning = false;

    private void InitialiseMatrix()
    {
        for(int i = 0; i < 3; i++){
            for(int j = 0; j < 3; j++)
            {
                SparringMatrix[i,j] = 1f/3f;
            }
        }
    }

    public void StartSpar()
    {
        if(isRunning) return;
        isRunning = true;
        InitialiseMatrix();
        StartCoroutine(SLoop());
    }
    public void StopSpar()
    {
        isRunning = false;
        StopAllCoroutines();
        Debug.Log("Training Stopped");
    }

    private IEnumerator SLoop()
    {
        while(isRunning)
        {
            float waited = 0f;
            yield return new WaitForSeconds(MoveInterval);
            while((aIBrainRef.process == AIBrain.Process.Hitstun || aIBrainRef.process == AIBrain.Process.Perform) && waited < 3f)
            {
                yield return new WaitForSeconds(0.1f);
                waited += 0.1f;
            }
            ActionType nextmove = NextMove();
            PerformMove(nextmove);
        }       
    }

    private ActionType NextMove()
    {
        if(UnityEngine.Random.value < RandomInfluence || lastSparringAction == ActionType.Neutral)
        {
            return (ActionType)UnityEngine.Random.Range(0,3);
        }
        int lastIndx = (int)lastSparringAction;
        float roll = UnityEngine.Random.Range(0,1);
        float cumulativeValue = 0f;

        for(int i = 0; i<3; i++)
        {
            cumulativeValue += SparringMatrix[lastIndx,i];
            if(roll <=cumulativeValue) return (ActionType)i;
        }

        return (ActionType)UnityEngine.Random.Range(0,3);
    }

    private void PerformMove(ActionType action)
    {
        previousSparringAction = lastSparringAction;
        lastSparringAction = action;

        playerRef.CommittToAction(action);
    }

    public void ObserveAICounter(ActionType aiAction, bool aiWon)
    {
        if(previousSparringAction == ActionType.Neutral || previousSparringAction == ActionType.Neutral) return;

        int row = (int)previousSparringAction;
        int col = (int)lastSparringAction;
        if(aiWon)
        {
            SparringMatrix[row,col] = Math.Max(0.05f, SparringMatrix[row,col] - 0.1f);
        }
        else
        {
            SparringMatrix[row,col] = Math.Max(0.05f, SparringMatrix[row,col] + 0.1f);
        }
        NormaliseSparRow(row);
    }

    private void NormaliseSparRow(int row)
    {
        for (int i = 0; i < 3; i++)
        {
            SparringMatrix[row,i] = Math.Max(0.05f, SparringMatrix[row,i]);
        }
        float total =  SparringMatrix[row,0]+ SparringMatrix[row,1]+ SparringMatrix[row,2];
        if(total<=0)
        {
            for (int i = 0; i < 3; i++)
            {
                SparringMatrix[row,i] = 1f/3f;
            }
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            SparringMatrix[row,i] /= total;
        }
    }
}
