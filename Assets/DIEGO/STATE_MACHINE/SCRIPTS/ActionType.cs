///// define the different types of actions that can be performed in the game
///// the player and the AI can perform these actions during their turn
///// scripts refer to these action types to determine the outcome of each turn's combat resolution
///// intent system: player/AI choose an ActionType, then the game resolves the outcome based on those choices

public enum ActionType
{
    Attack,
    Block,
    Heal
}
/*
Instead of using strings like "Attack" everywhere, use a clean enum:

ActionType.Attack
ActionType.Block
ActionType.Heal


GAME LOGIC FLOW:

Player clicks Attack/Block/Heal

TurnManager records player's intent

AI picks its own intent (smart logic)

Both animations play simultaneously

Animation events trigger damage/heal resolution (last frame)

Back to player's turn (or Game Over if player/enemy HP <= 0)

*/