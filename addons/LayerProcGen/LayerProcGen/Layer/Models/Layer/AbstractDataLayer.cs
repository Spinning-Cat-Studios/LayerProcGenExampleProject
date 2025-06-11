/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Godot;
using Runevision.Common;
using System;
using System.Collections.Generic;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Internal. Non-generic base class for all layers.
	/// </summary>
	public abstract class AbstractDataLayer
	{

		// Pertaining to all the different layers.
		protected string Subtype { get; }

		static Dictionary<LayerKey, AbstractDataLayer> s_LayerDict = new();

		/// <summary>
		/// Check if a layer with the exact key exists.
		/// </summary>
		public static bool HasLayer<T>(LayerArgumentDictionary args = null, string subtype = null) where T : AbstractDataLayer
		{
			var key = new LayerKey(typeof(T), args, subtype);
			return s_LayerDict.ContainsKey(key);
		}

		/// <summary>
		/// An enumeration of all current layers.
		/// </summary>
		public static IEnumerable<AbstractDataLayer> layers { get { return s_LayerDict.Values; } }

		internal static void ResetInstances()
		{
			foreach (var instance in s_LayerDict.Values)
				instance.ResetInstance();
			s_LayerDict.Clear();
		}

		// Pertaining to one layer.

		internal abstract void ResetInstance();

		private static readonly object s_GlobalLayerCreationLock = new object();

		protected AbstractDataLayer(LayerKey key)
		{
			lock (s_GlobalLayerCreationLock)
			{
				if (s_LayerDict.ContainsKey(key))
				{
					GD.Print($"Layer {key.ToString()} already exists, not creating a new one.");
					Logg.LogError($"Layer {key} already created!");
					return;
				}
				// GD.Print($"Creating layer {key.ToString()}");
				s_LayerDict.Add(key, this);
			}
		}
	}
}
