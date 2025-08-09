using Godot;
using System;
using System.Collections.Generic;

namespace Terrain3D.Scripts.Utilities;

public sealed class Terrain3DShim
{
    private readonly Node _terrain;
    private readonly GodotObject _data;
    private readonly float _vertexSpacing;

    public Terrain3DShim(Node terrain)
    {
        _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        // Terrain3D.data (0.9.3+)
        _data = _terrain.Get("data").AsGodotObject(); // dynamic access
        _vertexSpacing = (float)_terrain.Get("vertex_spacing");
    }

    public float HeightAt(Vector3 worldPos)
        => (float)_data.Call("get_height", worldPos); // returns NAN if outside/hole

    // Minimal “road painter”: paints a base texture strip along a path.
    public void PaintRoad(IList<Vector3> path, float widthMeters, int baseTextureId,
                          bool disableAutoshader = true,
                          float? angleDeg = null, float? scalePercent = null, float blend01 = 0f,
                          float stepMultiplier = 0.25f)
    {
        if (path == null || path.Count < 2) return;

        float step = Math.Max(0.1f, _vertexSpacing * stepMultiplier);
        float radius = widthMeters * 0.5f;

        for (int seg = 0; seg < path.Count - 1; seg++)
        {
            Vector3 a = path[seg];
            Vector3 b = path[seg + 1];
            float segLen = a.DistanceTo(b);
            Vector3 dir = (b - a).Normalized();

            for (float d = 0; d <= segLen; d += step)
            {
                Vector3 p = a + dir * d;

                // Rasterize a disc so we don’t miss pixels between sample points
                for (float ox = -radius; ox <= radius; ox += _vertexSpacing * 0.5f)
                for (float oz = -radius; oz <= radius; oz += _vertexSpacing * 0.5f)
                {
                    if (ox*ox + oz*oz > radius*radius) continue;
                    Vector3 q = new Vector3(p.X + ox, p.Y, p.Z + oz);

                    if (disableAutoshader) _data.Call("set_control_auto", q, false);
                    _data.Call("set_control_base_id", q, baseTextureId);
                    if (angleDeg.HasValue) _data.Call("set_control_angle", q, angleDeg.Value);     // 22.5° steps internally
                    if (scalePercent.HasValue) _data.Call("set_control_scale", q, scalePercent.Value); // -60..+80
                    _data.Call("set_control_blend", q, Mathf.Clamp(blend01, 0f, 1f));
                }
            }
        }

        // Rebuild control map texture array sent to the shader (MapType.TYPE_CONTROL = 1)
        _data.Call("force_update_maps", 1);
    }
}
