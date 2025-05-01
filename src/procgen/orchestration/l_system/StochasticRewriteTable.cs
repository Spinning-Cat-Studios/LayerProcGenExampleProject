using System;
using System.Collections.Generic;
using System.Linq;

public class StochasticRewriteTable
{
    private readonly Random _rnd;

    public StochasticRewriteTable(Random rnd)
    {
        _rnd = rnd;
    }

    // Build the full dictionary of rules
    public Dictionary<char, Func<string>> Build()
    {
        return new Dictionary<char, Func<string>>
        {
            // Main trunk ‘M’ – usually goes forward,
            // sometimes spawns a left or right branch
            ['M'] = () => Pick(
                (70, "FH M"),      // straight:   F = road, H = house
                (15, "F [ < S ] M"), // fork left
                (15, "F [ > S ] M")  // fork right
            ),

            // Side street ‘S’ – shorter and less likely to fork
            ['S'] = () => Pick(
                (60, "F H S"),
                (10, "F [ < S ] S"),
                (10, "F [ > S ] S"),
                (20, "F H")         // terminate
            ),

            // Road forward just keeps stepping; house every few cells
            ['F'] = () => Pick(
                (80, "F"),         // keep going
                (20, "F H")        // place a house
            )
        };
    }

    // Choose one expansion weighted by w
    private string Pick(params (int w, string expansion)[] choices)
    {
        int total = choices.Sum(c => c.w);
        int roll  = _rnd.Next(total);
        foreach (var (w, exp) in choices)
        {
            if (roll < w)
                return exp;
            roll -= w;
        }
        return choices.Last().expansion; // fallback
    }
}
