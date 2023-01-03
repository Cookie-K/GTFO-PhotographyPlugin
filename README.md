# Cinematography Plugin

A bepinex plugin to enable a cinematic camera mode.

Note: Everyone in the lobby must have this plugin installed in order for this mod to work!
| ![drone fpv 1](https://i.imgur.com/W00cemO.gif) | ![drone fpv 2](https://i.imgur.com/021Qiji.gif) |
|---|---|
| ![slow-mo mine](https://i.imgur.com/QvpHisJ.gif) | ![slow-mo fight](https://i.imgur.com/mdgretE.gif) |

## Plugin Menu
Pressing `F4` will open the following menu: 

![Plugin Menu](https://i.imgur.com/8Shl0iB.png)

1. `Free Camera`: The main tool of the plugin. Strips the entire UI and FPS effects off the screen and enters the player into free camera mode. You cannot interact with anything while in this state and the player will return to where they started free cam when exiting (the world may be gone when you exit but just walk and the culler should update).

    When entering free cam, the UI, Body, and FPS look smoothing options are disabled. The free cam option it self will also be disabled too if everyone else in the lobby is already in free cam.

    `Enemies`: Free cam does not hard aggro enemies but if you are in Free Cam with enemies already aggroed to you, enemies will attack the spot where you entered Free Cam (it will look kind of awkward...). In multiplayer, not everyone can be in Free Cam at the same time to avoid this issue. There must be at least one player who is not in free cam such that there is always somebody that the plugin can divert enemies to. 

    `Team Scans`: When a player is in free cam team scans will temporarily reduce the required members down so you can still complete them without the free cam player. The HUD won't update but you will still need all your non free cam players for it to progress. (For Bots, they do not have the intelligence to do scans on their own so this is only for use with real people)
  
    `Controls`:
    - `WASD`: move camera forward, backward, left, and right.
    - `space`: move up
    - `left ctrl`: move down
    - `left shift`: speed up (x2)
    - `left alt`: slow down (x1/2)
    - `left click`: roll counter clock wise
    - `right click`: roll clock wise
    - `scroll wheel forward`: zoom in
    - `scroll wheel back`: zoom out
    - `middle click`: reset camera roll and zoom

    Do take a look at the config file if you want to change some of these keybinds.

    Use the various sliders for movement, rotation, and zoom to adjust speed and smoothing weight to your liking.
    - `Dynamic Roll`: Rolls the camera in proportion to the camera's horizontal velocity. Gives a kind of FPV drone type feel.
    - `Align Pitch Axis w/ Cam`: Aligns the camera's axis of forwards and backwards movement with the camera's pitch. It's like the G-Mod no-clip controls when on and like the Minecraft creative controls when off.
    - `Align Roll Axis w/ Cam`: Aligns the camera's axis of left and right movement with the camera's roll. When turned off the camera's horizontal movement will be parallel to the horizon regardless of camera roll. When turned on, moving left and right will move the camera along the camera's rolled axis.
    - `Hide Targeting Text`: Hide the targeting text that highlights what agent to focus on when using `C`
    - `Edit Spot Light`: Edit the spotlight settings while in free cam. Range, Intensity, and Angle can be set.
    - `Edit Ambient Light`: Edit the ambient light that emits from your character. Range and Intensity can be set.

2. `Body`: Hides the body from the camera (arms and legs)
3. `UI`: Strip the UI of UI elements
    - `Toggle Bio`: Toggle ON to see the bio pings even if UI is turned off
4. `Aspect Ratio`: Add black bars to top and bottom of the screen to adjust the aspect ratio  
5. `FPS Look Smoothing`: Smooths the FPS camera movements for use outside of free cam
6. `DoF`: Adjusts the depth of field of the camera to get the blur effect on the foreground. (Unfortunately there seems to be a bug with how post processing is taken care of in R7's new pipeline such that the background can no longer be blurred. As of V 1.2.0 this setting can only blur the foreground)

    | ![DoF artifact](https://i.imgur.com/ymUAgRG.gif) | ![DoF Terminal](https://i.imgur.com/NwQRy0M.gif) |
    |---|---|

7. `Vignette`: turns the camera's vignette on/off 
8. `Ambient Particles`: turns off the ambient particles that are floating around the map
9. `Time Scale`: Changes the time scale of the game (the gifs below are showing the in game speed and are not edited down). Only one person at a time can change the time scale (the option will be disabled for those who are not controlling time). Additionally you can use the key binds `Q` `E` & `R` to slow, speed up, and pause/play time.

    | ![slow-mo bonk](https://i.imgur.com/CTjIc6N.gif) | ![slow-mo strikers](https://i.imgur.com/tapeOp8.gif) |
    |---|---|

## Targeting Cam

The targeting camera can be used to follow an agent (player or enemy) around in a third person perspective. While in free cam press `C` and look at the agent you want to target and let go of the key to enter the targeting camera mode.

`Controls`:
- `C`: Enter/exit targeting mode
- `wasd`: Adjust center of orbit
- `LMB`: Toggle rotation lock

*It should be noted that if the time scale is set to its minimum, the targeting select lags behind a bit as well. Increase the time scale just a little bit to make it select faster if needed.

   | ![targeting cam 1](https://i.imgur.com/laWrzXW.gif) | ![targeting cam 2](https://i.imgur.com/zJpeWme.gif) |
   |---|---|

## Keybinds
While in FPS:
- `F4`: Show/hide plugin menu
- `F5`: Enter/exit free cam

While in free cam:
- `Q`: Slow down time
- `E`: Speed up time
- `R`: Pause/play time
- `T`: Teleport player to camera location
- `Y`: Warp dimensions (includes pouncer dimensions)
- `C`: Target lock/unlock on agent

---
### Credits

- Developed by `Cookie_K`
- `Kolfo`, cameraman of these dramatic action shots in the examples here
- Special thanks to `Basrijs`, `iRekia`, and `Kolfo` for testing

#### If something doesn't work quite right or you want something specific added feel free to ping me on the official modding discord.

---
### Patches

- 1.2.4:
    - Updated for Alt R2 (finally eh?)
    - Updated for Bepinex 3.2.0
	
- 1.2.3:
    - Updated for Bepinex 3.0.0

- 1.2.2:
	- Nothing happened. I totally did not push the old .dll file by accident and not update in the last patch...

- 1.2.1:
    - Added a rotation lock feature to the targeting cam
    - Fixed a bug where the first input of time pause would not register

- 1.2.0:
    - Updated for R7 compatibility (upgrade to Net 6 & Bepinex 2.0.X)
    - Added Option to adjust the following 
        - spot light
        - ambient light
        - aspect ratio 
        - show/hide bio pings even if UI is off
    - Added a mode to target and follow agents in third person perspective
    - Added keybinds for enter/exit free cam, time scale, teleport, dimensional warp, and enter/exit target mode
    - Adjusted some default settings to better suit the feel of the camera on start up
    - Fixed issue where menu would not persist settings upon enter exit free camera 

- 1.1.4:
    - Updated to fix the mod from crashing on start up due to an oversight in the last patch

- 1.1.3:
    - Updated to fix a crash caused by the 2022-01-27 patch 

- 1.1.2: 
    - Fixed issue where bots will prevent the plugin from opening
	- Added chat message when plugin not installed by all players
	- Removed watermark when UI is stripped

- 1.1.1: 
    - Fixed issue where weapons were still active when menu is open

- 1.1.0: 
    - Updated to work with R6
    - Added ambient particle toggle

- 1.0.1: 
    - Removed package reference to Nidhogg networking api in favor of GTFO API
    - Small update to read me on enemies and team scans