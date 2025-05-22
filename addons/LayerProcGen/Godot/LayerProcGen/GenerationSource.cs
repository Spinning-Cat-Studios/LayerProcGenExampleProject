/*
 * Godot adaptation copyright (c) 2024 Sythelux Rikd
 *
 * Based on:
 * LayerProcGen copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4

using Runevision.Common;
using Godot;
using System.Linq;
using LayerProcGenExampleProject.Services.Database;

namespace Runevision.LayerProcGen;

/// <summary>
/// Godot component that creates a <see cref="TopLayerDependency"/>.
/// </summary>

public partial class GenerationSource : Node3D
{
	public LayerNamedReference layer = new();

	[Export(PropertyHint.ResourceType, nameof(LayerArgumentDictionary))]
	public LayerArgumentDictionary layerArguments = new();

	public Point size = Point.zero;

	public TopLayerDependency dep { get; private set; }

	public override void _EnterTree()
	{
		// TODO: this (the DatabaseService) should be an interface and not a concrete class
		// in order to be able to use this in other projects
		// Regardless should not be a direct reference to part of the implementation.
		using (var db = new DatabaseService())
		{
			db.ClearAllData();
		}

		if(!Engine.IsEditorHint())
			UpdateState();
	}

	public override void _ExitTree()
	{
		if (Engine.IsEditorHint()) return;
		if (dep != null)
			dep.isActive = false;
	}

	void UpdateState()
	{
		if (layer == null)
			return;

		// Get layer instance.
		AbstractChunkBasedDataLayer instance = layer.GetLayerInstance(layerArguments);

		// Create top layer dependency based on layer.
		if (instance != null && (dep == null || dep.layer != instance))
		{
			if (dep != null)
				dep.isActive = false;
			dep = new TopLayerDependency(instance, size, layerArguments);
		}
	}

	public override void _Process(double delta)
	{
		if(Engine.IsEditorHint()) return;
		base._Process(delta);
		UpdateState();
		if (dep == null)
			return;

		foreach (IGodotInstance layer in AbstractDataLayer.layers.OfType<IGodotInstance>())
		{
			// Node? layerRoot = layer.LayerRoot(); //temporarily disabled for debugging
			// if(layerRoot != null && layerRoot.GetParent() == null)
			//     CallDeferred("add_child", layerRoot); //TODO: would be cool to do this after build end
		}
		
		Vector3 focusPos = Position;
		Point focus;
		if (LayerManagerBehavior.instance?.generationPlane == LayerManagerBehavior.GenerationPlane.XZ)
			focus = (Point)(focusPos.xz());
		else
			focus = (Point)(focusPos.xy());
		dep.SetFocus(focus);
		dep.SetSize(Point.Max(Point.one, size));
	}
}
#endif
