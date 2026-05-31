using System;
using System.Collections;
using UnityEngine;

// Based on: NickGlenn. (n.d.). Unity3D [ShowOnly] Inspector Attribute. GitHub. https://gist.github.com/NickGlenn/84b8b43004a642b96ce9b6fef0bbcc8d
public class AIBrain : MonoBehaviour
{
    public enum Process {Think = 0, Perform = 1, Hitstun = 2, Death = 3};
    
    private float[,] TransitMatrix = new float[3,3]; //this is the transition matrix of the players past to upcoming actions
    private int[,] ObservationCounterMatrix = new int[3,3]; //keep track of how many observations were made
    public PlayerController playerControllerRef;
    [ShowOnly]private float DiceRoll;

    [Header("Adjustment Settings")]
    [SerializeField][Range(0,1)]private float LearningRate =0.1f;
    [SerializeField][Range(0,1)]private float EpsilonValue = 0.05f; //used as  the border value to determine whether we use random actions or use our matrix
    [SerializeField]private float MaxCellProbabilityValue = 0.85f;
    [SerializeField]private float MinCellProbabilityValue = 0.05f;
    [SerializeField]private float SuccessMultiplier = 1.0f;
    [SerializeField]private float FailureMultiplier = 0.5f;
    [Min(0)]private int MinObservations = 6; //Minimum attempts it should use before using the Matrix
    

//states of the players actions to record and use as reference
    [Header("Reference Action States")]
    [ShowOnly]public ActionType LastPlayerAction = ActionType.Neutral;
    [ShowOnly]public ActionType CurrentPlayerMove = ActionType.Neutral;
    [ShowOnly]public ActionType AIActions = ActionType.Neutral;
    [ShowOnly]public Process process = Process.Think;
    [ShowOnly]public AttackPhase AiAttackPhases = AttackPhase.None;

//player likelihood percentage debug values
    [ShowOnly][SerializeField]private float PP,PK,PB,KK,KB,KP,BB,BP,BK;
   
    private float cumulative_Value;
    
    private int PredictionRow = -1;
    [ShowOnly][SerializeField]private ActionType PredictedPlayerMove = ActionType.Neutral;
    private bool PrevPredictionWasRandom = false; //guard to make sure the random action is not influencing matrix updates
    private bool isAIReacting = false;
    //public float MaxWaitTime, ReactionTime;

    //event for when the ai makes a prediction- resolver will listen out for it
    public static event Action<ActionType> PlayerActionPredicted;
    //time that the hitstun lasts depending on the situation
    public float HitStunDuration = 0.2f;
    public float HitStunTradeDuration = 0.1f;

    // Pressure mechanic for player idle
    // reference the player to keep track of the current state for pressure
    private float PressureTimeout = 2.5f;
    [ShowOnly][SerializeField]private float IdleTimer = 0f;

    void Start()
    {
        InitializeMatrix();
    }
    void OnEnable()
    {
        PlayerController.PlayerActionAttempt += UpdatePlayerMove;
        PlayerController.CurrentActionReset +=SetCurrentPlayerAction;
    }
    void OnDisable()
    {
        PlayerController.PlayerActionAttempt -= UpdatePlayerMove;
        PlayerController.CurrentActionReset -=SetCurrentPlayerAction;
    }
    void Update()
    {
        PressureTimer();
        UpdateVisualMatrix();
    }

    public void EnterHitStun(float Duration)
    {
        StopAllCoroutines();
        AiAttackPhases = AttackPhase.None;
        process = Process.Hitstun;
        StartCoroutine(HitStunStart(Duration));
    }

    private IEnumerator HitStunStart(float Duration)
    {
        Debug.Log("[AI]Entering Hitstun State");
        yield return new WaitForSeconds(Duration);
        process = Process.Think;
        isAIReacting = false;
        IdleTimer = 0f;
        Debug.Log("AI Hitstun is over: ready to react.");
        AIPrediction();
    }

    //set the default to an even distribution of the values
    public void InitializeMatrix()
    {
        for(int i = 0; i < 3; i++){
            for(int j = 0; j < 3; j++)
            {
                TransitMatrix[i,j] = 1f/3f;
                ObservationCounterMatrix[i,j] =0;
            }
        }
        UpdateVisualMatrix();
    } 
    //updates the lastplayed move and temporarily stores it
    public void UpdatePlayerMove(ActionType CurrentMove)
    {
        if(LastPlayerAction == ActionType.Neutral && CurrentMove == ActionType.Neutral) 
        {
            AIActions =(ActionType)UnityEngine.Random.Range(0,3);      
        }
        if(LastPlayerAction != ActionType.Neutral)
        {
            //function that updates the matrix values
            UpdateMatrix((int)LastPlayerAction,(int)CurrentMove);
        }
        LastPlayerAction = CurrentMove;
        IdleTimer = 0f;
        TriggerAIReaction();


    }
    public void SetCurrentPlayerAction(ActionType currentState)
    {
        CurrentPlayerMove = currentState;
    }

    //function called by the CombatResolveManager to modify the transition matrix
    public void CombatResolutionResults(bool AiWon)
    {
        int rows = PredictionRow;
        int col = (int)PredictedPlayerMove;
        //dont adjust if the prediciton was random
        if(PrevPredictionWasRandom || rows<0 || col<0) return;
        //if the result is true then add to the matrix 
        if(AiWon)
        {
            TransitMatrix[rows,col] += LearningRate * SuccessMultiplier;
            Debug.Log($"Result: SUCCESS! -> increasing [{rows},{col}] by {LearningRate}");
        }
        //if false prediction then subtract
        else
        {
            TransitMatrix[rows,col] -= LearningRate * FailureMultiplier;
            Debug.Log($"Result: FAILED! -> Decreasing [{rows},{col}] by {LearningRate}");
        }
        //function to keep everything equal to 1
        NormalizeChosenRow(rows);    
    }

    private void NormalizeChosenRow(int row)
    {
        
        for (int i = 0; i < 3; i++)
        {
            TransitMatrix[row,i] = Mathf.Max(MinCellProbabilityValue, TransitMatrix[row,i]);
        }
        float rowTotal = TransitMatrix[row,0]+TransitMatrix[row,1]+TransitMatrix[row,2];
        if(rowTotal <=0)
        {
            for (int j = 0; j < 3; j++)
            {
                TransitMatrix[row,j] = 1/3f;      
            }
            return;  
        }
        for (int i = 0; i < 3; i++)
            {
                TransitMatrix[row, i]/=rowTotal;
            }
        for(int i =0; i < 3; i++)
        {
            if(TransitMatrix[row,i] > MaxCellProbabilityValue)
            {
               float extra = TransitMatrix[row,i] -MaxCellProbabilityValue;
                TransitMatrix[row,i] = MaxCellProbabilityValue; 
                for(int j=0; j<3; j++)
                {
                    if(j !=i) TransitMatrix[row,j] += extra/2f;
                }
                Debug.Log($"[AI] Cell:[{row},{i}] is limited to {MaxCellProbabilityValue} -- {extra} will be redistributed to other cells. ");
            }
            
        }
    }


    //update function for matrix values
    public void UpdateMatrix(int lastindx, int currentindx)
    {
        //using previous and current action for row and column index
        ObservationCounterMatrix[lastindx,currentindx] += 1;
        for(int i = 0; i<3; i++)
        {
            if(i == currentindx)
            {
                TransitMatrix[lastindx,i] += LearningRate;
            }
            else
            {
                TransitMatrix[lastindx,i] -= LearningRate/2f;
            }
        }
        NormalizeChosenRow(lastindx);
        Debug.Log("[AI]Updated Matrix");
    }

    public void UpdateVisualMatrix()
    {
        PP = (float)Math.Round(TransitMatrix[0,0],2);
        PK = (float)Math.Round(TransitMatrix[0,1],2);
        PB = (float)Math.Round(TransitMatrix[0,2],2);
        KP = (float)Math.Round(TransitMatrix[1,0],2);
        KK = (float)Math.Round(TransitMatrix[1,1],2);
        KB = (float)Math.Round(TransitMatrix[1,2],2);
        BP = (float)Math.Round(TransitMatrix[2,0],2);
        BK = (float)Math.Round(TransitMatrix[2,1],2);
        BB = (float)Math.Round(TransitMatrix[2,2],2);
    }

    public ActionType AIPrediction()
    {
        int lastPlayer = (int)LastPlayerAction;
        //1st random selection below Epsilon
        if(UnityEngine.Random.value < EpsilonValue)
        {
            PredictedPlayerMove = (ActionType)UnityEngine.Random.Range(0,3);
            PrevPredictionWasRandom = true;
            PredictionRow = lastPlayer;           
            Debug.Log($"e-greedy Exploring - [AI predicted]: {PredictedPlayerMove}");

        } //2nd random selection if minimum is not met
        else if(lastPlayer == -1 || RowObservations(lastPlayer) < MinObservations)
        {
            PredictedPlayerMove = (ActionType)UnityEngine.Random.Range(0,3);
            PrevPredictionWasRandom = true;
            PredictionRow = lastPlayer;          
            Debug.Log($"[AI] Observation minimum not Met- Predicted random:{PredictedPlayerMove}");
        }//3rd use the matrix data
        else
        {
            DiceRoll = UnityEngine.Random.Range(0f,1f);
            Debug.Log("[DICE ROLL]");
            float cumulativeValue = 0f;
            for(int j = 0; j < 3; j++)
            {
                cumulativeValue +=  TransitMatrix[lastPlayer,j];
                if(DiceRoll <= cumulativeValue)
                {
                    cumulative_Value = cumulativeValue;
                    PredictedPlayerMove = (ActionType)j;
                    break;
                }      
            }
            PrevPredictionWasRandom = false;
            PredictionRow = lastPlayer;
            Debug.Log($"[Matrix] Based Prediction: {PredictedPlayerMove}");  
        }
        PlayerActionPredicted?.Invoke(PredictedPlayerMove);//broadcast to functions that are listening
        return CounterMove(PredictedPlayerMove); // determine a counter with this prediction
    }

    private int RowObservations(int row)
    {
        if(row<0 || row>2) return 0;
        return  ObservationCounterMatrix[row,0]+
                ObservationCounterMatrix[row,1]+
                ObservationCounterMatrix[row,2];//the matrix row with observations made
    }

    public ActionType CounterMove(ActionType GuessedAction)
    {
        switch (GuessedAction)
        { 
            case ActionType.Punch:
                return ActionType.Block;   
            case ActionType.Kick:              
                return ActionType.Punch;   
            case ActionType.Block:
                return ActionType.Kick; 
            default:
                return ActionType.Neutral;
        }

    }
    private void PressureTimer()
    {
        IdleTimer += Time.deltaTime;
        if(isAIReacting || process == Process.Perform || process == Process.Hitstun || process == Process.Death)
        {
            //dont do anything if ai is currently busy
            IdleTimer = 0;
            return;
        }
        
        if(IdleTimer >= PressureTimeout)
        {
            IdleTimer = 0f;
            Debug.Log("[AI Pressuring ] Player is Idle");
            PressuredAttack();
        }
    }
    private void PressuredAttack()
    {
        if(isAIReacting || process == Process.Perform || process == Process.Hitstun || process == Process.Death)
        {
            return;
        }
        isAIReacting = true;
        AIActions = AIPrediction();
        process  = Process.Perform;
        isAIReacting = false;

        var (StartUp, Active, Recovery, Length) = Actions.FrameData(AIActions);
        TryAttack(StartUp, Active, Recovery, Length);
    }


    private void TriggerAIReaction()
        {
            if(isAIReacting || process == Process.Perform || process == Process.Hitstun || process == Process.Death)
            {
                Debug.Log("AI is busy. [REACTION IGNORED]");
                return;
            }
            isAIReacting = true;
            ReactToPlayer();
        }

        private void ReactToPlayer()
        {
            if(process == Process.Hitstun || process == Process.Death)
            {
                isAIReacting = false;
                return;
            }
            if( playerControllerRef != null && CurrentPlayerMove != ActionType.Neutral && playerControllerRef.PlayerAttackPhase != AttackPhase.None)
            {
                ActionType current = CurrentPlayerMove;
                PrevPredictionWasRandom = false;
                PredictionRow = (int)LastPlayerAction;
                PredictedPlayerMove = current;
                PlayerActionPredicted?.Invoke(current);
                AIActions = CounterMove(current);
                Debug.Log($"[AI] is Countering Player current action: {current}");
                
            }
            else
            {
                AIActions = AIPrediction();
            }
            process = Process.Think;
            isAIReacting = true;
            var (StartUp, Active, Recovery, Length) = Actions.FrameData(AIActions);
            TryAttack(StartUp, Active, Recovery, Length);
        }

    public void TryAttack(int StartUp, int Active, int Recovery, float Duration)
    {
        //coroutine to run through all the attackphases
        StartCoroutine(AttackSequence(StartUp,Active,Recovery,Duration));
    }
 
    public IEnumerator AttackSequence(int S, int A, int R, float Duration) 
    {
        // formula used will be seconds per frame then we use coroutine to wait x time for each phase
        int totalFrames = (S+A+R);

        if(totalFrames <= 0) yield break;

        float framerate = Duration/totalFrames;
        AiAttackPhases = AttackPhase.StartUp;
        yield return StartCoroutine(WaitedFrameTime(S,framerate));//startup time
        AiAttackPhases = AttackPhase.Active;
        yield return StartCoroutine(WaitedFrameTime(A,framerate));//active time
        AiAttackPhases = AttackPhase.Recovery;
        yield return StartCoroutine(WaitedFrameTime(R,framerate));//recovery time

        AiAttackPhases = AttackPhase.None;
        process = Process.Think;
        Debug.Log("[AI]Attack done");
        
    }

    IEnumerator WaitedFrameTime(int FrameCount, float FrameRate)
    {
        yield return new WaitForSeconds(FrameCount * FrameRate);
    }



}
