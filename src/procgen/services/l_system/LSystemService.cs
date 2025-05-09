using System;

public class LSystemService
{
    private readonly Random _rnd;

    public LSystemService(int chunkSeed)
    {
        _rnd = new Random(chunkSeed);
    }

    public string GenerateSequence(string axiom, int iterations)
    {
        var lSystem = new StatefulLSystem(_rnd);
        return lSystem.Generate(axiom, iterations);
    }

    public string SelectRandomAxiom()
    {
        string THREE_ROADS_AXIOM = "[ M ] [ | M ][ > M ] [ > | M ][ < M ] [ < | M ]";
        string TWO_ROADS_AXIOM = "[ M ] [ | M ][ >> M ] [ >> | M ]";
        
        return _rnd.Next(2) == 0 ? THREE_ROADS_AXIOM : TWO_ROADS_AXIOM;
    }

    public (float jitterX, float jitterZ) GenerateJitter(float range)
    {
        float jitterX = (float)(_rnd.NextDouble() * (2 * range) - range);
        float jitterZ = (float)(_rnd.NextDouble() * (2 * range) - range);
        return (jitterX, jitterZ);
    }
}
