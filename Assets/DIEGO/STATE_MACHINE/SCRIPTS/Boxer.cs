using UnityEngine;
using UnityEngine.UI;

///// this code is for the player and for the AI opponent
///// since they share the same logic for taking damage, healing, and setting their intent
///// same script, 2 instances (one for player, one for enemy)
///// this code doesnt resolve combat outcomes, it just tracks HP and the current intent for each boxer
///// the combat resolution logic is handled by the AnimationCombatEvent script, which reads the intents
public class Boxer : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthSlider;
    
    ///// tracks HP
    private int _currentHealth;

    ///// tracks the current intent (Attack/Block/Heal) chosen by this boxer for the turn
    private ActionType _currentIntent;

    ///// cache the Animator for triggering animations on damage/heal
    private Animator _anim;

    ///// Added for easier access to max HP in TurnManager on complex enemy mode
    ///// allows TurnManager to compare current HP to max HP for more dynamic win conditions
    public int MaxHP => maxHealth;

    void Awake()
    {
        ///// Try to get the Animator component on this GameObject and cache it for later use
        TryGetComponent(out _anim); // Cache animator

        ///// Initialize current health to max health at the start of the game
        _currentHealth = maxHealth;
        ///// Update the health slider UI to reflect the starting health
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        ///// Reduce current health by the damage amount
        _currentHealth -= amount;
        // Play 'Block' if they chose it, otherwise 'Hit'
        string trigger = (_currentIntent == ActionType.Block) ? "Block" : "Hit";
        ///// Trigger the appropriate animation based on whether they blocked or got hit
        _anim.SetTrigger(trigger);
        ///// Update the health slider UI to reflect the new health value
        UpdateUI();
    }

    public void Heal(int amount)
    {
        ///// Increase current health by the heal amount, but do not exceed max health
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        ///// Trigger the 'Heal' animation
        UpdateUI();
    }

    void UpdateUI()
    {
        ///// Update the health slider's value to reflect the current health percentage
        if (healthSlider != null) healthSlider.value = (float)_currentHealth / maxHealth;
    }

    ///// Set the current intent for this boxer (called by player input or AI logic)
    ///// this intent will be read by the AnimationCombatEvent script to determine the outcome of the turn's combat resolution
    ///// for example, if the boxer sets their intent to Attack,
    ///// then the AnimationCombatEvent will check the opponent's intent and apply damage/heal accordingly when
    ///// the animation event triggers (end freame of the animation)
    public void SetIntent(ActionType action) => _currentIntent = action;

    ///// Get the current intent (used by AnimationCombatEvent to determine outcomes)
    /// since the AnimationCombatEvent only needs to know the intent, not how it was chosen
    public ActionType GetIntent() => _currentIntent;

    ///// Public getter for current HP, used by TurnManager to check win conditions and update UI
    /// also used by AnimationCombatEvent to determine how much damage to apply based on current HP
    public int CurrentHP => _currentHealth;

    ///// Public getter for the Animator, used by TurnManager to trigger win/lose animations
    /// also used by AnimationCombatEvent to trigger hit/block/heal animations based on the current intent and combat outcome
    public Animator GetBoxerAnimator() => _anim;
}