using System;
using System.Collections.Generic;
using System.Text;
using Godot;

public class StatefulLSystem
{
    private readonly Random _rng;

    // --- tuning knobs -----------------------------------------------------
    private const int M_MIN =  5,  M_INC = 15, M_MAX = 80;   // trunk
    private const int S_MIN =  3,  S_INC = 12, S_MAX = 60;   // side streets
    private const int S_MIN_LEN = 4;     // side street must travel this far
    private const int S_KILL_P  = 25;    // % chance to terminate *after* min
    // ----------------------------------------------------------------------

    public StatefulLSystem(Random rng) => _rng = rng;

    public string Generate(string axiom, int iterations, TurtleState state)
        // 1. Generate a string using the L-system rules
        // 2. Interpret the string to produce a list of house positions
        // 3. Return the list of house positions

        // As a side-effect, the L-system will also modify the turtle state
    {
        string s = axiom;
        for (int i = 0; i < iterations; ++i)
            s = RewriteOnce(s, state);
        return s;
    }

    private string RewriteOnce(string src, TurtleState state) {
        var sb    = new StringBuilder();
        int dist = 0, len = 0; // distance since last fork, length of side street
        var stack = new Stack<(int dist,int len)>(); // one counter per branch

        foreach (char c in src)
        {
            switch (c)
            {
                //----------------------------------------------------------------
                //  Main trunk
                //----------------------------------------------------------------
                case 'M':
                {
                    int pFork = Clamp(M_MIN + dist * M_INC, M_MIN, M_MAX);

                    if (_rng.Next(100) < pFork)          // take a fork?
                    {
                        bool left = _rng.Next(2) == 0;
                        sb.Append($"F [ {(left ? "<" : ">")} S ] "); // branch
                        dist = 0;                      // reset after fork
                    }
                    else                                // keep going
                    {
                        sb.Append("F H M");
                        dist++;
                    }
                    break;
                }

                //----------------------------------------------------------------
                //  Side street
                //----------------------------------------------------------------
                case 'S':
                {
                    int pFork = Clamp(S_MIN + dist * S_INC, S_MIN, S_MAX);

                    bool canStop   = len >= S_MIN_LEN && _rng.Next(100) < S_KILL_P;
                    bool willFork  = !canStop && _rng.Next(100) < pFork;

                    if (willFork) {
                        bool left = _rng.Next(2) == 0;
                        sb.Append($"F [ {(left ? "<" : ">")} S ] ");
                        dist = 0; len = 0;
                    }
                    else {
                        sb.Append("F H S");
                        dist++;   len++;
                    }
                    break;
                }

                //----------------------------------------------------------------
                //  Support symbols – copied through unchanged
                //----------------------------------------------------------------
                case '[':                               // new branch → push
                    stack.Push((dist,len));
                    (dist,len) = (0,0);
                    sb.Append('[');
                    break;

                case ']':                               // back to parent
                    (dist,len) = stack.Pop();
                    sb.Append(']');
                    break;
                
                case '|':                               // turn around
                    state.Direction = state.Direction.Rotated(Vector3.Up, Mathf.Pi);
                    break;

                default:                                // F, H, <, >, …
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    private static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
}
