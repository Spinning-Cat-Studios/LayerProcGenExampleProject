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
using System.Linq;
using System.Reflection;

namespace Runevision.LayerProcGen {

	[Serializable]
	public class LayerNamedReference {
		public string className;

		Type cachedLayerType;
		AbstractChunkBasedDataLayer cachedLayerInstance;
		string cachedClassName;

		public Type GetLayerType() {
			if (string.IsNullOrEmpty(className)) {
				cachedLayerType = null;
				cachedClassName = className;
			}
			else if (cachedLayerType == null || className != cachedClassName) {
				cachedLayerType = AppDomain.CurrentDomain.GetAssemblies()
					.Select(domainAssembly => domainAssembly.GetType(className))
					.FirstOrDefault(type => type != null);
				if (cachedLayerType == null)
					Logg.LogError("Could not find layer type " + className + ".");
				cachedClassName = className;
			}
			return cachedLayerType;
		}

		public AbstractChunkBasedDataLayer GetLayerInstance(LayerArgumentDictionary layerArguments = null) {
			Type t = GetLayerType();
			if (t == null)
				return null;

			if (layerArguments != null && layerArguments.parameters != null && layerArguments.parameters.Count > 0)
			{
				// Look for the static InstanceWithArguments method
				var method = t.GetMethod("InstanceWithArguments", BindingFlags.Public | BindingFlags.Static);
				if (method != null)
				{
					return (AbstractChunkBasedDataLayer)method.Invoke(null, new object[] { layerArguments.parameters });
				}
			}

			// Fallback to singleton instance
			PropertyInfo propInfo = t.GetProperty("instance",
				BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			return (AbstractChunkBasedDataLayer)propInfo?.GetValue(null);
		}
	}

}
