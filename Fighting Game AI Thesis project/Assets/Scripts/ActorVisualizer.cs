using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorVisualizer : MonoBehaviour
{
    public SpriteRenderer spriteRendererRef;
    public PlayerController playerRef;
    public AIBrain aiBrainRef;

    public Sprite idleSprite, punchSprite, KickSprite, BlockSprite, HitstunSprite;

    public Color StartupColour = Color.yellow;
    public Color ActiveColour = Color.green;
    public Color RecoveryColour = Color.blue;
    public Color HitStunColour = Color.red;
    public Color IdleColour = Color.white;

    void Update()
    {
        if(playerRef != null) UpdatePlayerSprite();
        else if(aiBrainRef != null) UpdateAISprite();
    }

    void UpdatePlayerSprite()
    {
        //hitstun checked 
        if(playerRef.IsHitStun)
        {
            ApplySprite(HitstunSprite,HitStunColour);
            return;
        }
        //no action is committed, currently idle
        if(playerRef.PlayerAttackPhase == AttackPhase.None)
        {
            ApplySprite(idleSprite,IdleColour);
            return;
        }
         //assign the image and color  based the referenced states
        Sprite sprite = ActiveSprite(playerRef.CommittAction);
        Color highlight = SelectColor(playerRef.PlayerAttackPhase);
        
        ApplySprite(sprite,highlight);
    }

        void UpdateAISprite()
    {
        //hitstun checked 
        if(aiBrainRef.process == AIBrain.Process.Hitstun)
        {
            ApplySprite(HitstunSprite,HitStunColour);
            return;
        }
        //thinking state
        if(aiBrainRef.AiAttackPhases == AttackPhase.None)
        {
            ApplySprite(idleSprite,IdleColour);
            return;
        }
        //assign the image and color  based the referenced states
        Sprite sprite = ActiveSprite(aiBrainRef.AIActions);
        Color highlight = SelectColor(aiBrainRef.AiAttackPhases);
        
        ApplySprite(sprite,highlight);
    }

    private void ApplySprite(Sprite sprite, Color colour)
    {
        //checks existing sprite renderer and sprite
        if(sprite != null && spriteRendererRef.sprite != sprite)
        {
            spriteRendererRef.sprite = sprite;
        }
        spriteRendererRef.color = colour;
    }

    private Sprite ActiveSprite(ActionType action)
    {
        switch (action)
        {
            case ActionType.Punch:
                return punchSprite;
            case ActionType.Kick:
                return KickSprite;
            case ActionType.Block:
                return BlockSprite;
            default:
                return idleSprite;
        }
    }
    
    private Color SelectColor(AttackPhase phase)
    {
        switch (phase)
        {
            case AttackPhase.StartUp:
                return StartupColour;
            case AttackPhase.Active:
                return ActiveColour;
            case AttackPhase.Recovery:
                return RecoveryColour;
            default:
                return IdleColour;
        }
    }


}
