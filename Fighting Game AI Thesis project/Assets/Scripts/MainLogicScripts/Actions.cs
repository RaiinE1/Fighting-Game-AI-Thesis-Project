using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType {Punch = 0, Kick =1, Block = 2, Neutral = -1}; 
public enum AttackPhase {StartUp, Active, Recovery, None};
public class Actions 
{
   public static(int StartUpF, int ActiveF,int intRecoveryF, float ActLength) FrameData(ActionType ChosenAction)
    {
        switch (ChosenAction)
        {
            case ActionType.Punch:
                return (2,1,3,0.7f);
            case ActionType.Kick:
                return (5,1,4,0.9f);
            case ActionType.Block:
                return (1,6,1,0.7f);
            default:
                return (1,1,1,1f);
        }
    }
}