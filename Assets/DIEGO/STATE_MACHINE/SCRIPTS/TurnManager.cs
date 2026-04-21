using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

///// This script manages the overall turn flow of the game, including player input, AI decision-making, and win conditions
///// it also provides a static instance for other scripts to call methods like UpdateCombatLog without needing a reference to the TurnManager object
///// The singleton controller. One instance exists and any script can reach it via TurnManager.instance


/// THE GAME BRAIN:
/// PlayerTurn ──(player picks action)──► EnemyTurn
/// EnemyTurn ──(AI picks action)──► PlayerTurn
/// PlayerTurn/EnemyTurn ──(HP <= 0)──► GameOver
/// GameOver ──(Player clicks Play Again)──► PlayerTurn (new game starts)

public class TurnManager : MonoBehaviour
{
    // --- STATIC SINGLETON SETUP ---
    // This allows any other script to call TurnManager.instance
    public static TurnManager instance;

    private void Awake()
    {
        // Ensures there is only ever one TurnManager
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    // ------------------------------

    ///// Define the different states of the game flow
    /// PlayerTurn: waiting for player input
    /// EnemyTurn: AI is deciding and performing its action
    /// GameOver: either player or enemy HP reached 0, show results and disable input
    public enum TurnState { PlayerTurn, EnemyTurn, GameOver }

    [SerializeField] private Boxer player;
    [SerializeField] private Boxer enemy;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI combatLog;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private GameObject gameOverPanel;

    ///// Tracks the current state of the game flow
    private TurnState _currentState = TurnState.PlayerTurn;

    ///// Used to prevent multiple AI actions from overlapping if the player clicks buttons rapidly during the enemy turn
    private bool _isAIActing = false;

    ///// Used to ensure Game Over logic only triggers once, preventing animation conflicts and repeated scene reloads
    private bool _gameOverTriggered = false;


    void Start()
    {
        ///// Initialize the game state, ensuring the player starts first and the UI is set up correctly
        gameOverPanel.SetActive(false);
        UpdateCombatLog("Fight!");
    }

    void Update()
    {
        ///// Check win conditions at the start of each frame and transition to GameOver state if either boxer has 0 or less HP
        if (player.CurrentHP <= 0 || enemy.CurrentHP <= 0) _currentState = TurnState.GameOver;

        ///// Update the UI and game flow based on the current state
        switch (_currentState)
        {
            case TurnState.PlayerTurn:
                actionPanel.SetActive(true);
                turnText.text = "YOUR TURN";
                break;
            case TurnState.EnemyTurn:
                EnemyMode();
                break;
            case TurnState.GameOver:
                GameOverMode();
                break;
        }
    }

    void EnemyMode()
    {
        ///// Prevent the AI from taking multiple actions if the player clicks buttons rapidly during the enemy turn
        if (_isAIActing) return;
        _isAIActing = true;

        ///// Hide the action buttons and update the turn text to indicate the enemy is acting
        actionPanel.SetActive(false);
        turnText.text = "FIGHTING...";

        /* Simple AI logic based on current HP ratios and a bit of randomness to keep it unpredictable
       
        25% chance → Random action (intentional mistake)
        75% chance → Reads the situation:
            Enemy HP < 30%  → Mostly Block (panic mode)
            Enemy HP < 60%  → Heal
            Player HP > 50% → Mostly Attack (occasional Block)
            Player HP < 50% → Heavy Attack pressure
        */


        // 25% chance AI makes a random mistake
        if (Random.value < 0.25f)
        {
            ///// Randomly choose any action, ignoring HP ratios (for unpredictability and to prevent the AI from being too perfect)
            enemy.SetIntent((ActionType)Random.Range(0, 3));
        }
        else
        {
            ///// Calculate HP ratios to inform AI decisions
            float enemyHPRatio = (float)enemy.CurrentHP / enemy.MaxHP;
            float playerHPRatio = (float)player.CurrentHP / player.MaxHP;


            if (enemyHPRatio < 0.3f)
            {
                // Low HP: 70% block, 30% panic attack
                enemy.SetIntent(Random.value < 0.7f ? ActionType.Block : ActionType.Attack);
            }
            else if (enemyHPRatio < 0.6f)
            {
                // Moderately hurt: heal itself
                enemy.SetIntent(ActionType.Heal);
            }
            else if (playerHPRatio > 0.5f)
            {
                // Player is healthy: mostly attack but sometimes block to counter
                enemy.SetIntent(Random.value < 0.3f ? ActionType.Block : ActionType.Attack);
            }
            else
            {
                // Player is weakened: keep pressure but mix in blocks
                enemy.SetIntent(Random.value < 0.2f ? ActionType.Block : ActionType.Attack);
            }
        }

        ///// Trigger the appropriate animations for both player and enemy based on their chosen intents
        player.GetBoxerAnimator().SetTrigger(player.GetIntent().ToString());
        enemy.GetBoxerAnimator().SetTrigger(enemy.GetIntent().ToString());

        ///// After a short delay to allow animations to play, end the enemy turn and return control to the player
        Invoke("EndAITurn", 2f);
    }


    void EndAITurn()
    {
        ///// Reset the flag to allow the AI to act again on the next enemy turn
        _isAIActing = false;
        if (_currentState != TurnState.GameOver) _currentState = TurnState.PlayerTurn;
    }

    void GameOverMode()
    {
        ///// Hide the action buttons and show the game over panel with the results
        actionPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (!_gameOverTriggered)
        {
            _gameOverTriggered = true;

            ///// Trigger the appropriate win/lose/draw animations based on the final HP values of the player and enemy
            if (player.CurrentHP <= 0)
            {
                player.GetBoxerAnimator().SetTrigger("Die");
                enemy.GetBoxerAnimator().SetTrigger("Win");
            }
            else if (enemy.CurrentHP <= 0)
            {
                enemy.GetBoxerAnimator().SetTrigger("Die");
                player.GetBoxerAnimator().SetTrigger("Win");
            }
            else // Draw
            {
                player.GetBoxerAnimator().SetTrigger("Idle");
                enemy.GetBoxerAnimator().SetTrigger("Idle");
            }
        }

        ///// Update the turn text to show the final result of the match
        if (player.CurrentHP > enemy.CurrentHP) turnText.text = "PLAYER 1 WINS!";
        else if (enemy.CurrentHP > player.CurrentHP) turnText.text = "PLAYER 2 WINS!";
        else turnText.text = "DRAW!";
    }


    ////// This method is called by the AnimationCombatEvent script to update the combat log text based on the outcome of each turn's combat resolution
    /// since the combat resolution logic is handled in the AnimationCombatEvent script, it can call this method to update the log without needing a reference to the TurnManager object, thanks to the static instance setup at the top of this script
    public void UpdateCombatLog(string msg) => combatLog.text = msg;

    ///// These methods are called by the UI buttons to set the player's intent and transition to the enemy turn
    public void UI_Attack() => SelectAction(ActionType.Attack);
    public void UI_Block() => SelectAction(ActionType.Block);
    public void UI_Heal() => SelectAction(ActionType.Heal);

    ///// This method centralizes the logic for handling player input and transitioning to the enemy turn,
    /// ensuring that the player can only select an action during their turn and that the game flow is consistent
    private void SelectAction(ActionType action)
    {
        if (_currentState != TurnState.PlayerTurn) return;

        ///// Set the player's intent based on the button they clicked, which will be read by the
        /// AnimationCombatEvent script to determine the outcome of the turn's combat resolution
        player.SetIntent(action);
        _currentState = TurnState.EnemyTurn;
    }

    ///// This method is called by the "Play Again" button on the game over panel to reload the current scene and restart the game
    public void UI_PlayAgain() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}