using System;
using UnityEngine;

public class CombatResolveManager : MonoBehaviour
{
    public static Action<bool, ActionType, ActionType> onExchangeResolved;
    //get reference to the ai, player and EvaluationLogs
    public AIBrain AiPlayer;
    public PlayerController Player;
    public EvaluationLogs EvaLogs;   

    //stored attack phases to compare the next attach phase
    private AttackPhase PrevAiPhase = AttackPhase.None;
    private AttackPhase PrevPlayerPhase = AttackPhase.None;
    [SerializeField]private bool aiIsActive; 
    [SerializeField]private bool playerIsActive;

    void Update()
    {
        aiIsActive = AiPlayer.AiAttackPhases == AttackPhase.Active && PrevAiPhase != AttackPhase.Active;
        playerIsActive = Player.PlayerAttackPhase == AttackPhase.Active && PrevPlayerPhase != AttackPhase.Active;
        if(aiIsActive && playerIsActive)
        {
            //resolve simultaneous active frame interaction
            ResolveSimul();
        }
        else if(aiIsActive)
        {
            //call to resolve for the ai 
            ResolveAIAttack();
        }
        else if(playerIsActive)
        {
            //call to resolve for the player 
            ResolvePlayerAttack();
        }
        PrevAiPhase = AiPlayer.AiAttackPhases;
        PrevPlayerPhase = Player.PlayerAttackPhase;
    }

    private void ResolveAIAttack()
    {
        ActionType aiAction = AiPlayer.AIActions;
        ActionType playerAction = Player.CommittAction;
        if(Player.PlayerAttackPhase == AttackPhase.Active)
        {
            ResolveSimul();//simultaneous active function
            return;
        }
        //player got hit and it was without conflict
        string state = GetPlayerState();
        Debug.Log($"[FROM CRM] AI {aiAction} lands on player ({state}) - Player takes HITSTUN");
        Player.EnterHitStun(Player.HitStunDuration);
        ReportResults(true, aiAction,playerAction);


    }
    private void ResolvePlayerAttack()
    {
        ActionType aiAction = AiPlayer.AIActions;
        ActionType playerAction = Player.CommittAction;
        if(AiPlayer.AiAttackPhases == AttackPhase.Active)
        {
            ResolveSimul(); //simultaneous active function
            return;
        }
        string state = GetAIState();
        Debug.Log($"[FROM CRM] Player:{playerAction} lands on AI ({state}) - AI takes HITSTUN");
        AiPlayer.EnterHitStun(AiPlayer.HitStunDuration);
        ReportResults(false, aiAction,playerAction);
    }

    private void ResolveSimul()
    {
        ActionType aiAction = AiPlayer.AIActions;
        ActionType playerAction = Player.CommittAction;

        if(aiAction == playerAction)
        {
            Debug.Log($"[FROM CRM] Opponents traded: {aiAction} - Both HITSTUN");
            AiPlayer.EnterHitStun(AiPlayer.HitStunTradeDuration);
            Player.EnterHitStun(Player.HitStunTradeDuration);
            ReportResults(false, aiAction,playerAction);
        }
        bool aiWins = CounterSuccessful(aiAction,playerAction);

        if(aiWins)
        {
            Debug.Log($"[FROM CRM] AI:{aiAction} counters PLAYER: {playerAction} - player takes the HITSTUN");
            Player.EnterHitStun(Player.HitStunDuration);
        }
        else
        {
            Debug.Log($"[FROM CRM] Player:{playerAction} counters AI: {aiAction} - AI takes the HITSTUN");
             AiPlayer.EnterHitStun(AiPlayer.HitStunTradeDuration);
        }

        ReportResults(aiWins, aiAction,playerAction);
    }
    private void ReportResults(bool aiWon,ActionType aiAction,ActionType playerAction)
    {
        AiPlayer.CombatResolutionResults(aiWon);

        if(EvaLogs != null) EvaLogs.InteractionResults(aiWon, aiAction,playerAction);
        onExchangeResolved?.Invoke(aiWon, aiAction, playerAction);
    }


    private bool CounterSuccessful(ActionType AI, ActionType player)
    {
        return (AI == ActionType.Punch && player == ActionType.Kick)
            || (AI == ActionType.Kick && player == ActionType.Block)
            || (AI == ActionType.Block && player == ActionType.Punch);
    }

    private string GetPlayerState()
    {
        if(Player.IsHitStun) return "Hitstun";
        if(Player.PlayerAttackPhase == AttackPhase.StartUp) return "StartUp";
        if(Player.PlayerAttackPhase == AttackPhase.Recovery) return "Recovery";
        return "Idle";
    }
    private string GetAIState()
    {
        if(AiPlayer.process == AIBrain.Process.Hitstun) return "Hitstun";
        if(AiPlayer.AiAttackPhases == AttackPhase.StartUp) return "StartUp";
        if(AiPlayer.AiAttackPhases == AttackPhase.Recovery) return "Recovery";
        return "Think";
    }
}
