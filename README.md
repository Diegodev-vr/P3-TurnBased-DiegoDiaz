# P3-TurnBased-DiegoDiaz
Dawson College | Scripting 2 | 582-85E-DW
Diego Diaz - Student # 2545873

https://github.com/Diegodev-vr/P3-TurnBased-DiegoDiaz

FIGHT GAME - TURN BASE - GAME LOGIC FLOW:

> Player clicks Attack/Block/Heal
> TurnManager records player's intent
> AI picks its own intent (smart logic)
> Both animations play simultaneously
> Animation events trigger damage/heal resolution (last frame
> Back to player's turn (or Game Over if player/enemy HP <= 0)

PLAYER ACTIONS
I based my self on Rock, paper, sissors and create a similar version with Attack, Block Heal with my rules

Situation > Result

Attack vs Attack > both take damage
Attack vs Block > attacker punished
Attack vs Heal > damage opponent
Heal vs Heal > both heal
Block vs Block > nothing happend

AI LOGIC LEVELS

> 25% chance AI makes a random mistake
> Randomly choose any action, ignoring HP ratios (for unpredictability and to prevent the AI from being too perfect)
> Low HP: 70% block, 30% panic attack ELSE Moderately hurt: heal itself
> Player is healthy: mostly attack but sometimes block to counter
> Player is weakened: keep pressure but mix in blocks
