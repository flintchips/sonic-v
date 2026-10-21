# SonicV.

Hey it's me, flintchips. You're probably wondering what this is all about. 
well a few years back me and a few people from Red Miso Studios began work
on a new sonic game, this was long after the release of Sonic Hellfire saga, 
which if you're a HUGE fan of sonic fan games, you might of heard of. This one
was aimed to be a lot different, utilizing the unity game engine instead.
I, flintchips was the main programmer at the time, using a guide from sonic
retro [LINK](https://info.sonicretro.org/Sonic_Physics_Guide).

### Art direction.
<img width="25%" alt="Screenshot 2026-10-21 114531" src="https://github.com/user-attachments/assets/388f7579-b623-47dd-989f-b4da874d1250" />

Art and game design came from pixelcat_ who developed WONDERFUL sprite work
including all the animations for sonic and the level/object sprites currently
present in the scene. They are truly something else and I give great gratitude
to how much effort they put into this project, it wouldn't be what it is, or was to be, without
them. There was the idea to involve 2D DYNAMIC LIGHTING at some point in development
which would really really give this project its own visual identity compared to 
other sonic fan games. Some game mechanics would stem from this as well such as using it
to restrict vision of bosses, etc.

### Substeps. 
THERE IS a substep system, its quite simple. The faster you run
the more times between frames the game calculates your position based on your
current velocity and runs the player physics loop in order to allow faster 
action without running into issues that would normally be caused with traveling
too far each frame, the number of substeps maxes out at 6 currently.

### The Blue/gray Spring.
<img width="25%" alt="Screenshot 2026-10-21 114553" src="https://github.com/user-attachments/assets/8528d896-abbf-47c6-a7ee-0da48dfd62d7" />

The blue spring is a spring originally from Knuckles Chaotix, aimed to give more force than
the red spring, this was planned to be included in the game and the substep feature would help
it not break any physics calculations as the player can now travel faster than 16px per second.

The gray spring is a new type of spring, aimed to be visually smaller and weaker than any other
spring in the base sonic games, I love these new springs and they are present in the debug build.
___

## - original readme contents below -

note: the lighting system is not present yet in the game as it was first described here.
 
# Sound Engine (FMOD)
See instructions [here](https://www.fmod.com/docs/2.01/unity/user-guide.html)

# Lighting System

## How it works
The general idea is to make each color have its own shading gradient defined manually<br>
like it is done in most pixel art palettes.

The shader does this using a set of [Lut](https://lettier.github.io/3d-game-shaders-for-beginners/lookup-table.html) tables generated from gradient ramps.<br>
The color for replacement is taken from the tables depending on the brightness from the lighting texture.

<img src="https://user-images.githubusercontent.com/1497430/229542912-c903c884-b08d-4772-bbee-3d2bd5a323aa.png" width="600"><br>

Colors from the tables can be taken with interpolation blending, to make smooth transitions.<br>
By specify color ramps and using material options.

<img src="https://user-images.githubusercontent.com/1497430/229517322-98e91e47-0c54-47f0-8f0a-a10487153583.png" width="500"><br>

## Usage
First needed to create the `LutLight2D Asset`, it will generate a texture with a set of lut tables for the material.

<img src="https://user-images.githubusercontent.com/1497430/229541874-3e9ebf82-ef78-4597-bf94-60e731550475.png" width="500"><br>

It will create a material and Grayscale Ramps for it, which should already be enough to apply the material.

<img src="https://user-images.githubusercontent.com/1497430/229521544-3bec1376-7c8f-4bb4-809d-ab106f638af5.png" width="600"><br>

Next step is to set the gradient table for each color from the palette.<br>
After the gradients are set, need to press the Bake button to apply the result if it was not done automatically.<br>


<img src="https://user-images.githubusercontent.com/1497430/230003982-d3a49fcd-8b62-44d3-b6b8-feb16b60d7d7.png" width="600"><br>

> The Ramps file is a common .png which contains shading gradient for each color from the palette, starting from the original, lightest, to the darkest.<br>
> Colors also can have alpha channel.

