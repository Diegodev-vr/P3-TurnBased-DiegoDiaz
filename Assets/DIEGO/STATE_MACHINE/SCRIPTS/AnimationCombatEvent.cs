using UnityEngine;


///// This script is attached to animation events (LAST FRAME) on the Attack, Heal, and Block animations for both the player and the enemy
///// it reads the current intent of both boxers at the moment of impact and applies damage or healing accordingly,
///// while also updating the combat log text in the UI to reflect the outcome of each turn's combat resolution
///// this allows the combat resolution logic to be tightly integrated with the animations,
///// so that the damage and healing are applied at the correct moment in the animation, and the log
public class AnimationCombatEvent : MonoBehaviour
{

    ///// References to the Boxer scripts for both the player and the enemy, set in the inspector
    [SerializeField] private Boxer self;
    [SerializeField] private Boxer opponent;

    ///// This method is called by the Animation Events on the Attack animations for both boxers, at the moment of impact
    public void OnImpactFrame()
    {
        ///// Check if the boxer playing this animation has the Attack intent,
        ///// if not, do nothing (since the same animation could be played for different intents, we only want
        ///// to apply damage if the intent is actually Attack)
        if (self.GetIntent() != ActionType.Attack) return;

        ///// Read the opponent's intent to determine how much damage to apply and what log message to show
        ActionType enemyAction = opponent.GetIntent();
        string result = "";

        // Standard logic for Attack VS others
        ///// if both attack, both take damage
        if (enemyAction == ActionType.Attack)
        {
            self.TakeDamage(10); opponent.TakeDamage(10);
            result = "Both Attack! -20 HP each.";
        }
        ///// if the opponent blocks, the attacker takes some damage for getting countered
        else if (enemyAction == ActionType.Block)
        {
            self.TakeDamage(10);
            result = self.name + " attack got blocked! Counter -10 HP.";
        }
        ///// if the opponent heals they take full damage since they were vulnerable during the heal animation
        else
        {
            opponent.TakeDamage(20);
            result = self.name + " attacked successfully! " + opponent.name + " -20 HP.";
        }
        
        // Use the STATIC instance to update the log
        TurnManager.instance.UpdateCombatLog(result); 
    }

    ///// This method is called by the Animation Events on the Heal animations for both boxers,
    /// at the moment of the heal effect
    public void OnHealFrame()
    {
        ///// Check if the boxer playing this animation has the Heal intent,
        if (self.GetIntent() != ActionType.Heal) return;

        ///// Read the opponent's intent to determine how much healing to apply and what log message to show
        /// for example, if the opponent is attacking, the heal is interrupted and they take damage instead,
        /// since they were vulnerable during the heal animation
        if (opponent.GetIntent() == ActionType.Attack)
        {
            TurnManager.instance.UpdateCombatLog(self.name + " heal was INTERRUPTED! -20 HP!");
        }
             ///// if the opponent is also healing, both heal but we show a different message since they were both vulnerable
            else if (opponent.GetIntent() == ActionType.Heal)
        {
            self.Heal(10);
            TurnManager.instance.UpdateCombatLog(self.name + " and " + opponent.name + " both healed +10 HP!");
        }
            ///// if the opponent is blocking, the heal goes through but we show a different message since they were defending
            else if (opponent.GetIntent() == ActionType.Block)
        {
            self.Heal(10);
            TurnManager.instance.UpdateCombatLog(self.name + " healed +10 HP while " + opponent.name + " blocked.");
        }
            else
        {
            self.Heal(10);
            TurnManager.instance.UpdateCombatLog(self.name + " healed +10 HP.");
        }
    }

    ///// This method is called by the Animation Events on the Block animations for both boxers,
    /// at the moment of the block effect
    public void OnBlockFrame()
    {
        ///// Check if the boxer playing this animation has the Block intent,
        if (self.GetIntent() != ActionType.Block) return;

        ////// Read the opponent's intent to determine what log message to show
        if (opponent.GetIntent() == ActionType.Block)
        {
            ////// if both block, nothing happens but we show a different message since they were both defending
            TurnManager.instance.UpdateCombatLog(self.name + " and " + opponent.name + " both blocked! Nothing happened.");
        }
    }
}