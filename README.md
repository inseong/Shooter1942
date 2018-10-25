# Shooter1942

### How to control
- If you want to use a keyboard control, turn on the ‘Use Keyboard’ toggle.
- Arrow keys are used for moving the player’s fighter and Left-shift key is a fire button.
- Touch is a default control with auto-firing.

### Screen Setup
- matched a pixel unit of sprites up with a screen unit.
- Pixel Per Unit in Sprite is set all by 1
- A size value in Camera is set by calculating screen ratio.
- All value in world coordinate are same to a pixel unit.

### Resources
- got these images which were used in the 1942 game by searching in internet.
- used only some background images and airplanes needed in my game.
- also attached sound files of the 1942 in my project but didn’t use it yet.


### Stage data structure definition
- used ScriptableObject to save all game data.
- it is possible to design more stages by modifying Assets/Resouces/GameData.asset
- Game Data Structure

```
    * StageInfo
        Map List
             Background image group
        Wave List
             Distance             - distance value among waves
             Spawn List
                 Plane
                 Count
                 Spawn Index     - a index of enemy spawner of StageManager
                 Spawn Offset
                 Interval          - a time interval of spawning each Plane by the Count value.
                 Energy
                 Speed
            Time Limit            - can limit a playing time in a wave (not completed)
        Winning Condition
            Boss kill count 
            Fighter kill count
            Winning score
```

### Finite State Machine
- managing all procedures by the FSM
- Nested classes are used as each state.

### Player/Enemy movement
- Player & Enemy (Boss/Fighter) classes have a same parent class (Plane)
- These plane classes have Ready/Move/Dead states

Object Pooling
- All enemies, bullets, background sprites are managed by ObjectPool class

### Main GameObjects / Classes
StageManager        - Loading stage and managing all data of game stages
    Enemy Spawn     - Spawner object & class of Enemy object for generating enemy objects
PlayManager         - Controlling entire game play.
Player
    Bullet Spawn    - Bullet spawner object & class
    Life Bar

### What I want to Improve in this project
- More dynamic movement of enemies (AI or multi-movement patterns)
- integrating Sounds/Effects in a real game
- Multiple background layers – Cloud layer, water/terrain layers
- Enemies on the ground
- Power up of player’s weapon (power items, bombs, etc.)
