# SonicV
 
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

