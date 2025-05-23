﻿using Runevision.Common;
using System.Collections.Generic;
using Godot;

public abstract class DeformationSpec {
	public Vector4 control;

	public List<SpecPointB> points = new List<SpecPointB>();
	public GridBounds bounds;

	public GridBounds GetBounds() { return bounds; }

	public void CalculateBounds() {
		bounds = GridBounds.Empty();
		for (int i = 0; i < points.Count; i++) {
			Vector2 point = points[i].pos.xz();
			Vector2 padding = Vector2.One * points[i].outerWidth;
			bounds.Encapsulate((Point)(point - padding));
			bounds.Encapsulate((Point)(point + padding + Vector2.One));
		}
	}
}
