# **Blade Frenzy**

## **Pitch**

You are a warrior apprentice stepping into a mystical floating dojo to prove your worth through the ancient art of blade mastery. Waves of enchanted fruit are magically hurled toward you from all directions, and your task is to slice them apart using two glowing energy swords, one in each hand. But beware: deadly bombs are mixed into the waves, and striking one ends your run instantly. The challenge is simple to understand but difficult to master. As you progress, the fruit comes faster, in trickier formations, and from more unpredictable angles. Your reflexes, precision, and spatial awareness are put to the test as you chase higher scores, longer combos, and the glory of the dojo's leaderboard.

## **World**

* **Setting**: A stylized fantasy dojo floating among clouds in an ethereal sky. The environment is minimal and clean so the player's attention stays on the incoming objects and their swords.  
* **Aesthetic**: Low-poly, cel-shaded art style with glowing neon accents on the swords and sliced fruit. The dojo floor is a circular stone platform with ornamental pillars at the edges. The sky shifts color gradually as the player's score increases (calm blue at the start transitioning to intense sunset orange and finally a dramatic dark purple at high scores).  
* **Atmosphere**: Calm and focused at the start (gentle wind ambience, soft chimes). As the difficulty ramps, the music intensifies, particle effects increase, and the sky grows more dramatic, creating a building sense of pressure and excitement.  
* **Scale**: The player stands in the center of the dojo platform. Fruit launchers are positioned in a semicircle in front of and slightly above the player (roughly 120-degree arc), ensuring comfortable VR play without requiring the player to turn fully around.

## **Genre**

* **Primary**: VR Action Arcade  
* **Secondary**: Reflex / Skill-based Score Attack  
* The gameplay is fast-paced, physical, and score-driven. There is no narrative progression or exploration. The game is purely about moment-to-moment reaction, rhythm, and chasing a higher score. Sessions are short (2 to 5 minutes per run), making it ideal for pick-up-and-play VR.

## **Inspirations**

* **Fruit Ninja VR** \- The core slicing interaction: physically swinging your arms to cut objects in half. The satisfaction of clean cuts and seeing fruit split apart.  
* **Beat Saber** \- The sense of flow and physical rhythm. The way combo systems reward consistent performance. The clean, neon-lit visual style that reads clearly in VR. The gradual difficulty escalation within a session.  
* **SUPERHOT VR** \- The minimalist, stylized art direction that keeps the visual field uncluttered. The intense focus on spatial awareness and physical movement.  
* **Fruit Ninja Classic (Mobile)** \- The bomb avoidance mechanic that adds a layer of risk and decision-making to otherwise pure reflex gameplay. The "Arcade Mode" structure of timed runs with escalating difficulty.  
* **Arcade Cabinet Games (general)** \- The "one more try" loop driven by high score chasing. Simple rules, deep skill expression. Instant restart.

## **Required Mechanics**

Each mechanic is owned by one team member and is essential for the game to function:

1. **Sword Slicing System** (Member A \- Joshua)  
   * The player holds two virtual energy swords (one per VR controller). The swords track the controllers' position and rotation in real time. When a sword passes through a sliceable object (fruit) with sufficient velocity, the object is cut in half along the blade's trajectory. The two halves separate with physics, and a particle burst and sound effect play. This is the player's primary and only form of interaction with the game world. Without this mechanic, the player cannot do anything.  
2. **Object Spawning & Launch System** (Member B \- Mame)  
   * A spawn manager controls when, where, and how objects (fruits and bombs) enter the play space. Objects are instantiated at spawn points arranged in a semicircle around the player, then launched in an arc toward the player's position using physics forces. The spawn manager controls the timing intervals between spawns, the number of objects per wave, the ratio of fruit to bombs, and the trajectory parameters (speed, arc height, spread). This mechanic feeds the core loop by continuously giving the player things to react to. Without this mechanic, there is nothing for the player to slice.  
3. **Score, Combo & Difficulty Ramp System** (Member C \- Minh)  
   * The score system awards points for each fruit sliced. A combo counter tracks consecutive successful slices without a miss. As the combo grows, a score multiplier increases (e.g., 2x at 5 combo, 3x at 15 combo, 4x at 30 combo). Missing a fruit (letting it fall past the player) or hitting a bomb resets the combo. Hitting a bomb ends the round entirely. The difficulty ramp system monitors elapsed time and current score, and gradually increases spawn rate, object speed, and bomb frequency. This mechanic provides the long-term motivation: the player is always chasing a higher score and a longer combo streak, and the game pushes back harder the better they do.

## **Core Loop Schedule**

These are the mechanics the player interacts with constantly, every second of gameplay. They form the fundamental moment-to-moment fun:

1. **Sword Slicing System** (encountered first)  
   * This is the very first thing the player experiences. When the game starts (or during a brief tutorial), the player sees swords attached to their controllers and is prompted to swing them. The physicality of swinging and seeing the blade trail is immediately engaging even before any fruit appears. The tutorial could begin with a simple prompt: "Swing your swords\!" followed by a single stationary fruit to practice on.  
2. **Object Spawning & Launch System** (encountered second)  
   * Immediately after the player understands they can swing swords, fruit begins launching toward them. The first few fruit spawn slowly, one at a time, from directly in front of the player. This teaches the player the basic loop: see fruit coming, swing sword, slice it. Within seconds, the player is in the core loop: watch, react, slice, repeat.

## **Meta Loop Schedule**

These mechanics ramp up excitement the longer the player plays. They are not interacted with directly but shape the overall experience arc:

1. **Score & Combo Tracking** (noticed first within the meta loop)  
   * As soon as the player slices their first fruit, a score number appears and increments. After a few consecutive slices, a "Combo x2\!" indicator flashes. The player notices their score climbing faster. This is the first meta-level feedback the player receives: "I'm doing well, and the game is rewarding me for consistency." The combo multiplier creates micro-goals within each run (can I keep this streak going?).  
2. **Difficulty Ramp** (noticed gradually after score)  
   * The player won't immediately notice the difficulty increasing because it happens gradually. After 30 to 60 seconds, fruit start coming slightly faster and from wider angles. After 90 seconds, bombs start appearing more frequently. By the 2-minute mark, the player is dealing with rapid multi-fruit waves with bombs mixed in. The difficulty ramp is what gives each run its dramatic arc: a calm opening that builds to a frantic climax. The player only fully appreciates this system after a few runs, when they realize each session follows this escalation pattern and they are surviving longer as they improve.

## **Summary**

Blade Frenzy is a VR action arcade game built for Meta Quest using Unity. The player stands in a stylized floating dojo wielding two glowing energy swords, one in each hand. Enchanted fruit launches toward the player in arcs, and the player must physically swing their arms to slice the fruit apart. Bombs are mixed into the waves and must be avoided; striking a bomb ends the run. The core gameplay loop is simple and immediately satisfying: see an object, swing, slice it. A combo system rewards consecutive successful slices with escalating score multipliers, creating moment-to-moment tension as the player tries to maintain their streak. Over the course of each 2-to-5-minute run, a difficulty ramp system gradually increases the spawn rate, object speed, and bomb frequency, building the experience from a calm warm-up to an intense, arm-swinging frenzy. The clean, minimal art style ensures the player can always read the action clearly, and short session lengths encourage the "one more try" loop that defines great arcade games. The result is a physically engaging, easy-to-learn, hard-to-master VR experience that leverages the unique strengths of VR (spatial awareness, physical movement, presence) in a focused, polished package.

