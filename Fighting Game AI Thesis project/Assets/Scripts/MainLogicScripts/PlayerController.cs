using System.Collections;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public SelfPlayManager selfPlayManager; //reference to check completion state of the training session
    public static Action<ActionType>PlayerActionAttempt; //event will be broadcast when we make an input
    public static Action<ActionType>CurrentActionReset;  //reset the players current action to neutral or idle since then are not doing anything
    public ActionType CommittAction = ActionType.Neutral; //the action the player will make 
    //public enum AttackPhase {StartUp, Active, Recovery, None};
    public AttackPhase PlayerAttackPhase = AttackPhase.None;

    bool isButtonpressed, alreadyTapped, IsBlocking;
    public bool IsHitStun = false;
    public float HitStunDuration = 0.2f;
    public float HitStunTradeDuration = 0.1f;
    private float TapThreshold = 0.15f;
    public float counter = 0f;
    bool CanAct => !IsHitStun && PlayerAttackPhase == AttackPhase.None;
    [SerializeField]public bool IsPlayerPhase => selfPlayManager.TrainingComplete;


    void Update()
    {      
        isButtonpressed = Input.GetKey(KeyCode.D) && IsPlayerPhase == true;
        if(Input.GetKeyDown(KeyCode.A) && IsPlayerPhase == true)
        {     
            CommittToAction(ActionType.Punch);
            CurrentActionReset?.Invoke(ActionType.Punch);
        }
        if(Input.GetKeyDown(KeyCode.S) && IsPlayerPhase == true)
        {
            CommittToAction(ActionType.Kick); 
            CurrentActionReset?.Invoke(ActionType.Kick);          
        }
        ManageHoldButton();
    }

//hold for block and tap for all others
    void ManageHoldButton()
    {
        
        if(isButtonpressed) //check the button is pressed
        {
            counter += Time.deltaTime;
            //minimum holdtime required to set block state 
            if(counter >= 0.15f && !IsBlocking)
            {
                CommittToAction(ActionType.Block);
                Debug.Log("Holding block");
            }
        }
        else if(Input.GetKeyUp(KeyCode.D) && IsPlayerPhase) // we reset everything on button release
        {
            if(counter < TapThreshold && !alreadyTapped)
            {
                CommittToAction(ActionType.Block);
                CurrentActionReset?.Invoke(ActionType.Block);
                Debug.Log("Tap to Block");
                alreadyTapped = true;
            }
            counter = 0f;
            alreadyTapped = false;
            IsBlocking = false;
           
        }
        
    }
    //called to enter hitstun state
    public void EnterHitStun(float Duration)
    {
        StopAllCoroutines();
        PlayerAttackPhase = AttackPhase.None;
        CommittAction = ActionType.Neutral;
        StartCoroutine(HitStunStart(Duration));
    }

    private IEnumerator HitStunStart(float Duration)
    {
        IsHitStun = true;

        Debug.Log("Entering Hitstun State");
        yield return new WaitForSeconds(Duration);
        IsHitStun=  false;
        Debug.Log("[Player] Hitstun is over: Reseting to Neutral State.");
    }
    public void CommittToAction(ActionType action)
    {
        if(!CanAct)
        {
            Debug.Log($"Player Input is ignored: {(IsHitStun? "Currently HITSTUN":"Currently Mid-Attack")}");
            return;
        }
        CommittAction = action;
        PlayerActionAttempt?.Invoke(action);
        var (StartUp, Active, Recovery, Length) = Actions.FrameData(action);
        StartCoroutine(AttackSequence(StartUp, Active, Recovery, Length));
    }

     public IEnumerator AttackSequence(int S, int A, int R, float Duration) 
    {
        // formula used will be seconds per frame then we use a coroutine to wait x time for each phases length
        int totalFrames = (S+A+R);

        if(totalFrames <= 0) yield break;

        float framerate = Duration/totalFrames;
        PlayerAttackPhase = AttackPhase.StartUp;
        yield return StartCoroutine(WaitedFrameTime(S,framerate));//startup time
        PlayerAttackPhase = AttackPhase.Active;
        yield return StartCoroutine(WaitedFrameTime(A,framerate));//active time
        PlayerAttackPhase = AttackPhase.Recovery;
        yield return StartCoroutine(WaitedFrameTime(R,framerate));//recovery time
        PlayerAttackPhase = AttackPhase.None;

        CommittAction = ActionType.Neutral;
        CurrentActionReset?.Invoke(ActionType.Neutral);
        Debug.Log("Player Attack Complete");

    }
     IEnumerator WaitedFrameTime(int FrameCount, float FrameRate)
    {
        yield return new WaitForSeconds(FrameCount * FrameRate);
    }
}
