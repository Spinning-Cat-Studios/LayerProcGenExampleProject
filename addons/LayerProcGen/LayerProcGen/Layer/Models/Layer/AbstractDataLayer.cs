/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

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

		static Dictionary<(Type, string), AbstractDataLayer> s_LayerDict = new();

		/// <summary>
		/// Check if a layer of the specified type exists without creating it as a side effect.
		/// </summary>
		public static bool HasLayer<T>(string subtype = "") where T : AbstractDataLayer
		{
			return s_LayerDict.ContainsKey((typeof(T), subtype));
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

		protected AbstractDataLayer(string subtype = null)
		{
			Subtype = subtype ?? "";
			var key = (GetType(), Subtype);

			if (s_LayerDict.ContainsKey(key))
				Logg.LogError($"Layer {GetType().Name} with subtype '{Subtype}' already created!");

			s_LayerDict.Add(key, this);
		}
	}
}
