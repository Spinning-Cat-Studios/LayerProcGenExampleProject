# LayerProcGen Example Project

This is a project showcasing what can be built with LayerProcGen on top of the Godot Game Engine.

## What is LayerProcGen?

LayerProcGen is a framework that can be used to implement layer-based procedural generation that's **infinite**, **deterministic** and **contextual**. It works out of the box in Unity but can be used in any C#-compatible engine.

This project is Godot Game engine specific.

## Getting started

- Download Godot Game Engine 4.4.1 mono (note, the C# build is required, not just the base engine).
- Clone this project.
- Open the project in Godot.
- Create a C# solution for your project if it doesn't work out of the box.

<img width="959" alt="Create a C# solution" src="https://github.com/user-attachments/assets/0b8ae0f5-344f-461d-a755-94ae25a1f5b2" />

- Click run + watch hamlet generation amazingness.
- Have a cookie.

## What is being demonstrated here?

Building on work due to Sythelux [here](https://github.com/Sythelux/LayerProcGen/tree/godot_project), I provide a simple Terrain3D layer, and a VillageLayer on top of that.

This provides a network of "hamlets", using white cubes as a placeholder asset for now.

## Demonstration

<p align="center">
  <img 
    src="https://github.com/user-attachments/assets/fbb62dfd-d2bf-4c2c-b19a-a8e4d0ed6e6e"
    alt="LayerProcGen Demo"
    width="800"
  />
</p>

# More on LayerProcGen

Read [here](layerprocgen.md).
