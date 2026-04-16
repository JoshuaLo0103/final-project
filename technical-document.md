# **Technical Design Document** 

## **Required Mechanics Plan**

Each Required Mechanic totals 10 rubric points. Each sub-item names the exact rubric story it corresponds to.

### **1\. Sword Slicing System: Member A \- Joshua (10 pts)**

* **Slice and Dice (5 pts)** The core slicing mechanic. When the player swings a sword through a fruit with sufficient velocity, the system calculates a slice plane from the blade's trajectory and generates two independent half-objects where the original fruit was. Each half gets its own Rigidbody and Collider, and a force is applied perpendicular to the slice plane so the halves fly apart realistically. The new objects' geometry follows the line of the player's swing. This is the defining interaction of the entire game.

* **Hit Boxes \- 2 hit box events (2 pts)** Two distinct collision events drive the game logic: (1) **Sword-Fruit Hit Box**: when the sword's trigger collider intersects a fruit collider and the sword exceeds the velocity threshold, the slice is triggered and an `OnFruitSliced` event fires. (2) **Sword-Bomb Hit Box**: when the sword's trigger collider intersects a bomb collider, the `OnBombHit` event fires, which reduces the player's lives (see Member B's Enemies feature) or ends the game.

* **Juicy Feedback \- 3 feedbacks (3 pts)** Three feedback effects are triggered by the slicing interaction: (1) **Spatialized Slice Sound Effect**: a satisfying "slash" sound plays at the 3D position of the slice point, drawing the player's ear to where the cut happened. Different fruit types use slightly different pitch variations. (2) **Slice Particle Burst**: a non-looping burst of colored particles (matching the fruit type: red for watermelon, orange for orange, etc.) explodes at the slice position, visually confirming the hit. (3) **Bomb Explosion Particle Burst**: when a bomb is hit, a distinct non-looping explosion particle effect (dark red/orange, larger radius than fruit particles) plays at the bomb's position, clearly communicating the penalty.

### **2\. Object Spawning & Launch System: Member B \- Mame  (10 pts)**

* **NPC Spawner (2 pts)** A SpawnManager script controls the spawning of all non-player objects (fruit and bombs). It runs a coroutine that spawns objects at configurable intervals from a semicircular array of 5 to 7 spawn points positioned in front of the player. The spawn rate, object type distribution, and active spawn points are all parameterized so the Difficulty system (Member C) can adjust them over time. A 3-second grace period at the start of each run ensures no bombs spawn immediately.

* **Copier (3 pts)** The spawning system dynamically instantiates new copies of fruit and bomb prefabs using an object pool. Each instantiated copy has its own Rigidbody for physics simulation. There is no cap on how many objects can exist simultaneously (the pool expands automatically if needed). This meets the Copier requirement: an interaction (the spawn timer ticking) dynamically instances new copies of objects with physics rigidbodies, and the system continuously creates new ones throughout the entire run.

* **Conditional Despawning (2 pts)** Fruit and bombs are removed from the scene through two player-driven conditions: (1) **Sliced**: when Member A's hit box system triggers a successful slice, the original fruit is deactivated and returned to the pool (replaced by the two half-objects which themselves despawn after 2 seconds). (2) **Missed**: if a fruit falls below the platform (Y \< \-2) without being sliced, it is deactivated, returned to the pool, and an `OnFruitMissed` event fires. Bombs that are dodged (fall without being struck) are silently despawned.

* **Juicy Feedback \- 3 feedbacks (3 pts)** Three feedback effects tied to the spawning system: (1) **Spatialized Miss Whoosh Sound**: when a fruit is missed and despawns below the platform, a "whoosh" sound plays at its last position, alerting the player they missed one. (2) **Spawn Point Flash (Motion Ease)**: when a fruit launches from a spawn point, the spawn marker briefly scales up and glows using an ease-out animation, giving the player a subtle directional cue of where the object came from. (3) **Bomb Fuse Sizzle Sound**: bombs emit a spatialized sizzling sound while airborne, creating audio tension and helping the player identify incoming bombs by sound even before seeing them.

### **3\. Score, Combo & Difficulty Ramp System: Member C \- Minh (10 pts)**

* **Points Scoring (2 pts)** Certain discrete interactions award the player with points. Specifically, slicing a fruit awards its base point value (watermelon \= 3, orange \= 4, banana \= 5, apple \= 4, etc.) multiplied by the current combo multiplier. The total score is displayed on a world-space scoreboard visible in the dojo environment.

* **Combo Streak (2 pts)** Performing slices consecutively without missing increases a combo counter. The combo counter multiplies the points scored each time:

  * Combo 0–4: 1x multiplier  
  * Combo 5–14: 2x multiplier  
  * Combo 15–29: 3x multiplier  
  * Combo 30+: 4x multiplier Missing a fruit or hitting a bomb resets the combo counter to 0\. The combo count and current multiplier tier are displayed on the scoreboard.  
* **Win Condition Scoreboard (2 pts)** A visible world-space element (floating wooden board in the dojo) tracks the player's progress. It displays: current score, combo count, multiplier tier, and the current high score to beat. The high score serves as the "win condition" the player is working toward. When the player surpasses their high score, the board flashes gold and displays "NEW HIGH SCORE\!"

* **Restart Option (1 pt)** A UI button labeled "Restart" is available on the Game Over screen. Pressing it resets all game state (score, combo, difficulty, lives, spawning) and begins a new run from the start.

* **Juicy Feedback \- 3 feedbacks (3 pts)** Three feedback effects tied to the scoring and progression system: (1) **Spatialized Combo Chime Sound**: when the combo multiplier increases to a new tier (e.g., 1x→2x), a chime sound plays at the scoreboard's position, rewarding the player audibly. (2) **Score Punch (Motion Ease)**: each time points are added, the score text briefly scales up (punch effect) using an ease-out curve, drawing the player's eye to the score change. (3) **Combo Popup (Motion Ease)**: when a new multiplier tier is reached, a "2x COMBO\!" text pops up with a scale-and-fade ease animation near the scoreboard, clearly communicating the upgrade.

## **Alpha Features Plan**

Each teammate targets 10–20 additional rubric points. Listed by team member with the rubric story and point value for each feature.

### **Member A \- Alpha Features (15 pts)**

* **Grab Interactables \- 3 props (2 pts)** Three objects use Grab Interactable components with Direct Interactors on the player's hands: (1) Left sword: the player's left hand holds an energy sword. (2) Right sword: the player's right hand holds an energy sword. (3) Tutorial practice target: during the tutorial, a stationary fruit on a pedestal can be grabbed and repositioned before slicing practice.

* **Integrating 3D Meshes \- 5 meshes (2 pts)** Five bespoke 3D meshes are imported from Sketchfab and integrated into the scene: (1) Energy sword model (neon blade with hilt). (2) Watermelon half-mesh (used after slicing). (3) Orange half-mesh. (4) Banana half-mesh. (5) Apple half-mesh. All meshes are visible during gameplay when fruit is sliced.

* **Triggering Integrations \- 5 triggers (4 pts)** Five unique non-looping animations triggered by game events: (1) Slice triggers the fruit-split animation (halves separate along cut plane). (2) Bomb hit triggers an explosion animation (bomb model shatters outward). (3) Perfect slice (directional accuracy bonus) triggers a golden burst animation. (4) Combo tier change triggers a sword glow color-shift animation (blue→green→gold). (5) Game start triggers a sword ignition animation (blades light up from hilt to tip).

* **Juicy Feedback \- reach 8 total (5 pts total for Member A, \+2 pts incremental beyond Required)** Five additional feedbacks beyond the 3 in Required: (4) **Bomb Explosion Spatialized Sound**: a low "boom" plays at the bomb's position on hit. (5) **Sword Trail (Motion Ease)**: a TrailRenderer draws a glowing arc behind each sword swing, with length/opacity easing based on velocity. (6) **Perfect Slice Sound**: a distinct high-pitched "ding" plays on directional accuracy bonus slices. (7) **Fruit-Specific Splash Particles**: each fruit type has a unique non-looping particle color and shape on slice. (8) **Swoosh Sound on Fast Swing**: a wind-cutting sound plays when the sword exceeds a high velocity threshold, even without hitting anything.

* **Prop Verbs \- 2 prop verbs (1 pt)** Two button-press effects that only work while holding a sword (Grab Interactable): (1) Pressing the trigger while holding a sword activates a "Power Slash" mode for 1 second (wider slash hitbox, bonus points). (2) Pressing the grip button while holding a sword toggles the blade color between two styles (cosmetic).

* **Poke Interactor Buttons \- 2 buttons (2 pts)** Two buttons interactable via Poke Interactor on the player's hands: (1) Tutorial "Next" button to advance to the next tutorial stage. (2) Tutorial "Skip" button to jump directly into gameplay.

* **Secrets (2 pts)** A hidden interaction exists in the dojo environment. If the player slices a specific ornamental object on one of the dojo pillars (not obviously interactable), a secret compartment opens revealing a special golden sword skin. The golden sword is purely cosmetic but replaces the default sword model for the rest of the session.

**Member A Total: 10 (Required) \+ 15 (Alpha) \= 25 pts**

### **Member B \- Alpha Features (15 pts)**

* **Integrating 3D Meshes \- 8 meshes (3 pts)** Eight bespoke 3D fruit and bomb meshes imported from Sketchfab: (1) Watermelon, (2) Orange, (3) Banana, (4) Apple, (5) Grapes cluster, (6) Pineapple, (7) Cherry pair, (8) Bomb with fuse. All are visible during gameplay as the objects launched at the player.

* **Triggering Integrations \- 3 triggers (3 pts)** Three unique non-looping animations triggered by spawning events: (1) Fruit spawn triggers a brief "materialization" animation (fruit scales from 0 to full size over 0.2 seconds at the spawn point). (2) Bomb fuse triggers an ignition animation (fuse sparks ignite when the bomb launches). (3) Special spawn pattern triggers an announcement animation (e.g., "THE WALL\!" text flies in and fades for pre-designed formations).

* **Resource Simulation \- 1 resource (1 pt)** A "Lives" resource is Euler integrated and tracked. The player starts each run with 3 lives. Lives are displayed as 3 heart icons on the world-space HUD. When a life is lost (bomb hit), the heart empties with an animation. When lives reach 0, the game ends. This replaces the instant-death bomb mechanic with a more forgiving system that still creates tension.

* **Enemies (2 pts)** Bombs are classified as enemies. Through their collider intersecting with the sword (detected by Member A's Hit Box system), bombs reduce the player's Lives resource by 1\. The bomb's visual design (dark color, red glow, fuse sparks) and audio (sizzle sound) clearly distinguish it from fruit.

* **Loot Drop (2 pts)** Requires Conditional Despawning and Resource Simulation (both present). Occasionally, a special glowing green "healing fruit" spawns among the regular fruit. When the player slices it (conditional despawning via slice), instead of awarding points, it restores 1 life (up to the maximum of 3). This creates a risk-reward moment: the player must identify and prioritize the healing fruit among fast-moving regular fruit and bombs.

* **Collectibles (2 pts)** Rare golden star tokens occasionally spawn alongside fruit waves. They float briefly in the play area and can be collected by the player slicing them or touching them with a sword. A visible counter on the HUD tracks how many golden stars have been collected across the run. Stars contribute a flat bonus to the final score on the Game Over screen. This gives players a secondary objective beyond pure slicing.

* **Juicy Feedback \- reach 5 total (4 pts total for Member B, \+1 pt incremental beyond Required)** Two additional feedbacks beyond the 3 in Required: (4) **Bomb Warning Glow (Motion Ease)**: bombs pulse with an easing red glow that intensifies as they approach the player, creating visual urgency. (5) **Miss Screen Flash (Motion Ease)**: when a fruit is missed, a brief red vignette flashes at the edges of the player's view using an ease-in-out opacity animation on a world-space quad.

* **Path Following (2 pts)** Some special fruit (e.g., "Tricky Banana") follow a pre-defined curved path instead of a simple parabolic arc. The path is designed by the developer using a series of waypoints. The fruit smoothly interpolates along the path using a spline. These path-following fruit appear in later difficulty tiers and are harder to predict, adding variety to the spawning patterns beyond just faster/more fruit.

**Member B Total: 10 (Required) \+ 15 (Alpha) \= 25 pts**

### **Member C \- Alpha Features (15 pts)**

* **Quit Option (1 pt)** A UI button labeled "Quit" is available on both the Game Over screen and the Pause menu. Pressing it exits the application or returns to the main menu scene.

* **Loss Timer (1 pt)** A countdown timer is displayed on the world-space HUD. Each run has a configurable time limit (e.g., 3 minutes). When the timer reaches 0, the run ends (the player "survived" the full round). This creates a clear endpoint for each run rather than relying solely on bomb deaths. If the player loses all lives before the timer ends, the run also ends early.

* **Poke Interactor Buttons \- 2 buttons (2 pts)** Two buttons interactable via Poke Interactor: (1) "Restart" button on the Game Over screen: pokeable to restart the run. (2) "Quit" button on the Game Over screen: pokeable to exit.

* **Juicy Feedback \- reach 8 total (5 pts total for Member C, \+2 pts incremental beyond Required)** Five additional feedbacks beyond the 3 in Required: (4) **Game Over Stinger Sound**: a dramatic spatialized sound cue plays when the run ends (different tones for surviving the full timer vs. losing all lives). (5) **Difficulty Tier Change Sound**: when the difficulty ramp crosses a tier threshold (Easy→Medium→Hard→Insane), a unique escalation sound plays. (6) **Difficulty Tier Text (Motion Ease)**: the tier label (e.g., "HARD") slides in from the side with an ease-out animation and briefly pulses before settling. (7) **High Score Celebration Particles**: when the player beats their high score, a non-looping burst of golden confetti particles erupts from the scoreboard. (8) **Game Over Screen (Motion Ease)**: the Game Over panel eases in with a smooth scale-up animation, and each stat (score, combo, accuracy) counts up one by one with sequential ease animations.

* **Triggering Integrations \- 5 triggers (4 pts)** Five unique non-looping animations/music cues triggered by events: (1) Combo tier increase triggers the combo chime animation (multiplier badge scales and glows). (2) Game over triggers the stinger animation (screen darkens, stats fly in). (3) Difficulty tier change triggers a music cue (background music crossfades to a higher intensity layer). (4) High score beaten triggers the celebration animation (confetti \+ banner). (5) Run start triggers an intro animation (countdown "3, 2, 1, GO\!" with each number scaling in and out).

* **Multiple Levels \- 2 levels (3 pts)** The game has 2 distinct levels (separate scenes) with different content and difficulty curves: (1) **Dojo Courtyard (Level 1\)**: the default environment. Slower base spawn rate, lower bomb ratio. The "introductory" experience. (2) **Dojo Rooftop (Level 2\)**: unlocked after clearing Level 1 (surviving the full timer). Faster base spawn rate, higher bomb ratio, and the spawn arc is wider (150 degrees instead of 120). Different skybox and lighting (night sky with lanterns). The player moves to Level 2 after clearing Level 1\.

* **Level Transition (2 pts)** Upon clearing Level 1 (timer reaches 0 and the player has lives remaining), a transition sequence plays: spawning stops, remaining airborne fruit freeze and burst into particles, a "LEVEL COMPLETE\!" banner animates in, the score tallies with a gold-clinking sound, and the scene fades to black before loading Level 2\. This transition counts toward the Triggering Integrations (the music cue and celebration animation listed above).

* **Level Clear with Ranking (2 pts)** Before transitioning to the next level, the player's point total is ranked:

  * D-Rank: below 500 points  
  * C-Rank: 500–999 points  
  * B-Rank: 1,000–1,999 points  
  * A-Rank: 2,000–3,499 points  
  * S-Rank: 3,500–4,999 points  
  * SS-Rank: 5,000+ points The rank is displayed prominently on the level complete screen with a corresponding letter graphic and sound (higher ranks get more dramatic fanfare). Thresholds are tunable via a ScriptableObject.

**Member C Total: 10 (Required) \+ 15 (Alpha) \= 25 pts**

## **Stretch Features Plan**

These features would be great additions for the portfolio but are not expected to ship within 5 weeks:

* **Procedural Generation (7 pts)**: new levels are procedurally generated with randomized spawn point layouts, environmental props, and difficulty curves, creating endless unique stages.  
* **Multiplayer (10 pts)**: two players stand in adjacent dojos and compete for the higher score on the same fruit waves, seeing each other's combo status in real time.  
* **Inter-session Saves (3 pts)**: high scores, unlocked levels, and collected golden stars persist between play sessions using serialized JSON in `Application.persistentDataPath`.  
* **Branching Routes (2 pts)**: after clearing a level, the player chooses between two different next levels (e.g., "Volcano Forge" vs. "Ice Temple"), each with unique visuals and spawning behaviors.  
* **Mirrors (2 pts)**: a reflective surface in the dojo lets the player see their sword-wielding silhouette in real time using a Camera-to-RenderTexture setup.  
* **Character Cosmetics & Customization (2 pts)**: golden stars (Collectibles) can be spent to unlock new sword skins and hand models from a cosmetics menu.  
* **Branching Level Tree (2 pts)**: unlocking Level 2 opens two further level options (Level 3A and 3B), each of which unlocks two more, forming a branching tree of increasingly challenging stages.  
* **Idle Progress (3 pts)**: upon relaunching the game, bonus points or golden stars are awarded based on time elapsed since the last session.

## **Collaboration Summary**

The team of three will meet twice per week: **Tuesdays and Thursdays from 11:00 AM to 1:00 PM** in **NCSA**.

**Code coordination** uses a shared **GitHub repository**.

**Dependencies and handoff plan:**

1. **Member B's NPC Spawner and Copier must work first.** Even with placeholder cubes, fruit prefabs need to exist and fly toward the player before Member A can test slicing or Member C can test scoring. Member B will deliver a minimal spawning prototype (cubes launching in arcs) by the end of Week 1\.

2. **Prefab contract (agreed in Week 0):** Every fruit prefab has a Collider (trigger), a Rigidbody, a tag ("Fruit" or "Bomb"), and a FruitData component with `int pointValue` and `FruitType enum`. Member A's slice detection depends on this structure. Member B's enemies and loot drop also build on these tags.

3. **Member A fires events that Member C consumes.** The `OnFruitSliced(FruitSliceEventArgs)`, `OnBombHit()`, and `OnFruitMissed()` events are defined in a shared `GameEvents.cs` file on `main` in Week 0\. Member C subscribes to these events for Points Scoring, Combo Streak, and Win Condition Scoreboard. Member B also fires `OnFruitMissed` from the despawn logic.

4. **Member C's difficulty ramp modifies Member B's spawn parameters.** Member B exposes public methods: `SetSpawnInterval(float)`, `SetBombRatio(float)`, `SetLaunchSpeedMultiplier(float)`. Member C's DifficultyManager calls these methods based on AnimationCurve evaluations. These methods must be available by end of Week 2\.

5. **Member C's Multiple Levels depend on both A and B's systems being stable.** Level 2 reuses the same slicing and spawning systems with different parameter values. No additional code is needed from A or B; the Level 2 scene simply references different ScriptableObject configs for spawn rates and difficulty curves.

**Timeline:**

* **Week 1**: Each member delivers their Required Mechanic prototype in isolation.  
* **Week 2**: Integration sprint. All systems connected. A playable core loop exists.  
* **Week 3**: Polish pass. Juicy feedback, 3D meshes, tuning. External playtesting.  
* **Week 4**: Alpha features. Each member builds their additional 15 pts of features.  
* **Week 5**: Bug fixes, performance optimization on Quest, final build, submission by April 30th.

