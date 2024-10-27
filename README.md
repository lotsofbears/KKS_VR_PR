# KKS_VR - VR Plugin for Koikatsu Sunshine
A BepInEx plugin for Koikatsu Sunshine (KKS) that allows you to play both the main game and studio in VR.
The difference from the official VR modules is that you have access to the full game/studio, while the official modules have limited features and spotty mod support.

Currently only the standing (aka room-scale) mode is fully supported.

The main game part is a fork/port of the KoikatuVR/KK_MainGameVR plugin developed by mosirnik, vrhth, KoikatsuVrThrowaway and Ooetksh.

The studio part is a fork of the [KKS_CharaStudioVR](https://vr-erogamer.com/archives/1065) plugin.

## Prerequisites

* Koikatsu Sunshine
* Latest version of BepInEx 5.x and KKSAPI/ModdingAPI
* SteamVR
* A VR headset supported by SteamVR
* VR controllers

## Installation

1. Make sure BepInEx, KKSAPI and all their dependencies have been installed.
2. Download the latest [release](https://github.com/IllusionMods/KKS_VR/releases).
3. Extract the zip into the game folder (where the abdata and BepInEx folders are).
4. Create a shortcut to KoikatsuSunshine.exe and/or CharaStudio.exe, and add `--vr` to the command line.

## Control

**Warning: This section was written for KK_MainGameVR and might not be accurate, especially in Studio.**

This plugin assumes that your VR controller has the following buttons/controls:

* Application menu button
* Trigger button
* Grip button
* Touchpad or Thumbstick

You may need to tweak button assignments in SteamVR's per-game settings if your
controllers don't natively have these. See the Controller Support section for
a list of known-to-work controllers.

In the game, each of the controllers has 3 tools: Menu, Warp and School/Hand. Only
one of them can be active at a time. You can cycle through the tools by pressing
the Application menu button. Each controller has a cyan icon indicating which
tool is currently active.

When any of the tools is active, you can press and hold the Application menu
button to see in-game help on button roles.

### Menu tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_menu.png" height="30">

The menu tool comes with a little screen on which game menus, icons and texts
are shown. You can use the other controller as a virtual laser pointer, and
pull the Trigger to click on the screen. Most game interactions (specifically,
the ones that don't involve touching 3D objects) are done this way.

Pressing the Grip button while the Menu tool is active causes the screen
to be detached and left at the current position in the 3D space. Pressing it
again reclaims the screen.

A laser pointer can also generate a right click (Touchpad right), middle click
(Touchpad center) and scroll up/down (Touchpad up/down). You can also grab
a detached screen by holding Grip. Press and hold the Application menu button
while the laser is visible to see help about this.

### Warp tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_warp.png" height="30">

The warp tool allows you to move around in the 3D space. 

Use the touchpad to teleport. Before you finish teleporting, you can draw a
circle along the rim of the trackpad (or similarly rotate the thumbstick)
to change your would-be orientation after teleporting.

Holding the Grip button takes you into grab action. Here you can move around
by "grabbing" the world. If you additionally press Trigger, you can also rotate
the world. Pressing both Trigger and the touchpad gives you the full power
of general 3D rotation, allowing you to turn a wall into the floor, for
example. Double click the touchpad to become upright again.

Grab action is also avaible in the school and hand tools.

### School tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_school.png" height="30"> and Hand tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_hand.png" height="30">

These tools are collections of Koikatsu-specific action commands and simulated
mouse/keyboard inputs. The hand tool is for H scenes, and the school tool is for
all other scenes. Other than that, these two are similar to each other. The
button mappings are configurable for each of them separately.
The default for the school tool is:

* Trigger: Walk (Roam mode)
* Grip: Grab action
* Touchpad up: F3
* Touchpad down: F1
* Touchpad left: Turn left
* Touchpad right: Turn right
* Touchpad center: Right mouse button

For the hand tool:

* Trigger: Left mouse button
* Grip: Grab action
* Touchpad up: Mouse wheel scroll up
* Touchpad down: Mouse wheel scroll down
* Touchpad left: (unassigned)
* Touchpad right: Right mouse button
* Touchpad center: Middle mouse button

For touchpad inputs, you need to press the touchpad or click the thumbstick.
Just touching the touchpad or tilting the thumbstick won't be recognized.
Exceptions to this rule are mouse wheel scroll actions and rotate actions,
which only require touching.

## Situation-specific controls

**Warning: This section was written for KK_MainGameVR and might not be accurate, especially in Studio.**

The school tool can be used when you need more complex interactions than simple
mouse clicks.

There are also a few types of context-specific controls, where you can interact
directly with 3D objects using the controllers. This type of interaction does
not require any specific tool to be selected. The tool icon disappears when
such an interaction is available.

Below is a list of situations that offer special controls.

### Roaming

In the Roaming mode, you can move around by using the school tool to walk
(default: Trigger), and turn left and right (default: Touchpad left and right).
You can also use the warp tool to teleport.

You can use the school tool to simulate ordinary mouse and keyboard
inputs, e.g. right click (default: touchpad center) for interacting with an
object.

Use the laser pointer (touchpad center) to open the middle-button menu.

You can crouch by lowering your viewpoint relative to the floor. To do this,
you can either physically move your head or use grab action to bring the
floor closer. This behavior can be disabled in the config.

### Talk scene

When talking to a character, most interactions are done through the menu.
In addition, you can touch or look at the character by putting one of the
controllers at the position you want to touch/look at, then pulling the
Trigger. 

### H scene

Caressing can be done in the same way as touching in talk scenes. Additionally,
you can switch to a different mode of caressing by pressing the Application
menu button with the controller in place.

Optionally, automatic touching can be enabled, so that you don't even need to
pull the Trigger.

You can also kiss or lick the female by moving your head to the right place.
This can be turned off in the config.

You can undress a female character by putting a controller on one of her
clothing items, then pressing the touchpad. A single touchpad click will
perform one level of undressing. If you press and hold the touchpad,
move the controller away and then release the touchpad, it will completely
remove that part of clothing.

Unfortunately this direct undressing operation doesn't work as expected for
all clothes because the plugin doesn't exactly know what parts of
body each clothing item covers. You can always fall back to
using the menu for clothing state changes.

When changing location, you can use the green laser to point to a new location
icon and pull the Trigger to confirm.

## Configuration

**Warning: This section was written for KK_MainGameVR and might not be accurate, especially in Studio.**

This plugin has a lot of configuration options. It is recommended that you use
[ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager),
which allows you to change settings of this plugin from within the game.

Alternatively you can manually edit `BepInEx\config\mosirnik.kk-main-game-vr.cfg`
with a text editor.

## Controller Support

At the moment, most VR controllers seem to work out of the box with this plugin.
Below is an incomplete list of the current support status. If your controllers
are not listed here, please let us know if they work or not (either edit this 
file or create a new issue).

### Works out of the box
* Oculus Rift / Rift S / Quest 2
* Valve Index
* Vive

### HTC Vive

Works out of the box.

### HP motion controllers

The following button assignments are needed:

* Enumlated trackpad: (remove assignments)
* B and Y buttons: Application Menu Button
* Joystick: Trackpad position & value

In addition, you need to make it "pretend to be Oculus Touch controllers".

## Common issues

### Can't click on the virtual screen

This plugin requires that the game window on the Windows desktop is visible and
not covered by something else.

### Framerate is low

If you experience a framerate drop when the camera approaches a character,
particularly in an H scene, then the bottleneck is likely your GPU. I'd suggest
turning down the antialiasing setting using the
[GraphicsSettings](https://github.com/BepInEx/BepInEx.GraphicsSettings) plugin.
If that is not enough, consider disabling some visual effects or reducing the rendering
resolution in SteamVR.

If you experience a low framerate when roaming in main game, try disabling expensive plugins 
or reducing the number of characters that can be loaded at the same time (on left in roster).

## Building (for developers)

You should be able to open the solution in Visual Studio 2019 and just hit Build to build everything.
