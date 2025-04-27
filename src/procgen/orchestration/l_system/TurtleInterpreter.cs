using Godot;
using System.Collections.Generic;
using System;

public class TurtleInterpreter
{
    const float roadLength = 10f;
    const float houseSpacing = 5f;
    const float rotationAngle = 30f;

    TurtleState state;
    Stack<TurtleState> stack = new();
    Func<Vector3, float> getHeight;

    public TurtleInterpreter(Func<Vector3, float> heightSampler)
    {
        getHeight = heightSampler;
    }

    public void Interpret(string sequence, Vector3 startPosition, Vector3 startDirection, List<Vector3> housePositions)
    {
        state = new TurtleState(startPosition, startDirection);

        foreach (char symbol in sequence)
        {
            switch (symbol)
            {
                case 'A':
                    housePositions.Add(state.Position);
                    state.Position += state.Direction * houseSpacing;
                    break;

                case 'B':
                    stack.Push(state.Clone());
                    state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.DegToRad(rotationAngle));
                    break;

                case '[':
                    stack.Push(state.Clone());
                    break;

                case ']':
                    if (stack.Count > 0)
                    {
                        state = stack.Pop();
                        state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.DegToRad(-rotationAngle));
                    }
                    break;

                case 'C':
                    housePositions.Add(state.Position + state.Direction.Cross(Vector3.Up) * houseSpacing);
                    housePositions.Add(state.Position - state.Direction.Cross(Vector3.Up) * houseSpacing);
                    state.Position += state.Direction * houseSpacing;
                    break;

                case 'D':
                    state.Position += state.Direction * roadLength;
                    break;
            }
        }
    }
}
