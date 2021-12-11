# Cinematography Plugin

A bepinex plugin to enable a cinematic camera mode 
| ![drone fpv 1](https://i.imgur.com/W00cemO.gif) | ![drone fpv 2](https://i.imgur.com/021Qiji.gif) |
|---|---|
| ![slow-mo mine](https://i.imgur.com/QvpHisJ.gif) | ![slow-mo fight](https://i.imgur.com/mdgretE.gif) |


## Plugin Menu
Pressing `F4` will open the following menu: 

![Plugin Menu](https://i.imgur.com/LV8iRto.png)

1. `Free Camera`: The main tool of the plugin. Strips the entire UI and FPS effects off the screen (except the modded watermark) and enters the player into free camera mode. You cannot interact with anything while in this state and the player will return to where they started free cam when exiting (the world may be gone when you exit but just walk and the culler should update).

    When entering free cam, the UI, Body, and FPS look smoothing options are disabled. The free cam option it self will also be disabled too if everyone else in the lobby is already in free cam.

    `Enemies`: Free cam does not hard aggro enemies but if you are in Free Cam with enemies already aggroed to you, enemies will attack the spot where you entered Free Cam (it will look kind of awkward...). In multiplayer, not everyone can be in Free Cam at the same time to avoid this issue. There must be at least one player who is not in free cam such that there is always somebody that the plugin can divert enemies to. 

    `Team Scans`: When a player is in free cam team scans will temporarily reduce the required members down so you can still complete them without the free cam player (The HUD won't update but you will still need all your non free cam players for it to progress).
  
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

2. `UI`: Strip the UI of UI elements (except the watermark)
3. `Body`: Hides the body from the camera (arms and legs)
4. `FPS Look Smoothing`: Smooths the FPS camera movements for use outside of free cam
5. `DoF`: Adjusts the depth of field of the camera to get the blur effect on the foreground. Google will tell you more about what the sliders do better than I can. (Animation updates to your weapons seems to reset the changes to DoF. It's recommended to be in free cam or to turn off the body if you are trying to use this feature)

    | ![DoF artifact](https://i.imgur.com/ymUAgRG.gif) | ![DoF Terminal](https://i.imgur.com/NwQRy0M.gif) |
    |---|---|

6. `Vignette`: turns the camera's vignette on/off 
7. `Ambient Particles`: turns off the ambient particles that are floating around the map
8. `Time Scale`: Changes the time scale of the game (the gifs below are showing the in game speed and are not slowed down). Only one person at a time can change the time scale (the option will be disabled for those who are not controlling time).

    | ![slow-mo bonk](https://i.imgur.com/CTjIc6N.gif) | ![slow-mo strikers](https://i.imgur.com/tapeOp8.gif) |
    |---|---|
---

If something doesn't work quite right or you want something specific added feel free to ping me on the official modding discord.

###### (btw, there is no guarantee that this will work correctly when R6 comes around soon lul (or even now...))

### Credits

- Developed by Cookie_K
- Kolfo, cameraman of these dramatic action shots in the examples here
- Special thanks to Basrijs, iRekia, and Kolfo for testing
---
### Patches
- 1.0.1: 
    - Removed package reference to Nidhogg networking api in favor of GTFO API
    - Small update to read me on enemies and team scans
- 1.1.0: 
    - Updated to work with R6
    - Added ambient particle toggle