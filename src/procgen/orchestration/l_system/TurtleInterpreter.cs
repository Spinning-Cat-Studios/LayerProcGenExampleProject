using Godot;
using System.Collections.Generic;
using System;
using Runevision.Common;

public class TurtleInterpreter
{
    const float roadLength = 10f;
    const float houseSpacing = 5f;
    const float rotationAngle = 30f;
    // 30–60° produces crooked, medieval-looking streets
    const float kTurnMin = 0.45f;   //  26°
    const float kTurnMax = 1.05f;   //  60°

    Stack<TurtleState> stack = new();
    Func<Vector3, float> getHeight;
    readonly int worldSeed;
    readonly bool noisy;

    public TurtleInterpreter(
        Func<Vector3,float> heightSampler,
        int seed = 0,
        bool useHeadingNoise = true
    ) {
        getHeight = heightSampler;
        worldSeed = seed;
        noisy     = useHeadingNoise;
    }

    public void Interpret(
        string sequence,
        TurtleState state,
        List<Vector3> housePositions,
        List<(Vector3, Vector3)> roadPositionDirections,
        List<int> roadStartIndices,
        List<int> roadEndIndices
    ) {
        foreach (char symbol in sequence)
        {
            switch (symbol)
            {
                case 'F':                       // “Forward” along a road
                    for (int i = 0; i < roadLength;  ++i)
                    {
                        if (noisy)
                            state.Direction = HeadingNoise.PerturbDirection(
                                                state.Direction, state.Position,
                                                worldSeed);
                        state.Position += state.Direction;
                        roadPositionDirections.Add((state.Position, state.Direction));
                    }
                    break;

                // Branch markers (no extra rotation here!)
                case '[': {
                    stack.Push(state.Clone());
                    roadStartIndices.Add(roadPositionDirections.Count);
                    break;
                }
                case ']': {
                    roadEndIndices.Add(roadPositionDirections.Count);
                    if (stack.Count > 0) state = stack.Pop(); break;
                }

                case '|':                               // turn around
                    state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.Pi);
                    break;

                //­­ Turn *in-place*, but by a **random** angle each time
                case '>':      // right turn
                    {
                        float θ = (float)GD.RandRange(kTurnMin, kTurnMax);
                        state.Direction = state.Direction.Rotated(Vector3.Up,  θ);
                    }
                    break;
                case '<':      // left turn
                    {
                        float θ = (float)GD.RandRange(kTurnMin, kTurnMax);
                        state.Direction = state.Direction.Rotated(Vector3.Up, -θ);
                    }
                    break;

                // Drop a house on the current cell
                case 'H':
                    housePositions.Add(state.Position);
                    break;
            }
        }
    }
}
