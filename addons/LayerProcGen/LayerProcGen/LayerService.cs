using Godot;
using Runevision.Common;
using Runevision.SaveState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Runevision.LayerProcGen {
    
	
	public abstract class AbstractLayerService {

        public abstract string name { get; }

		internal AbstractLayerService() { }

	}

    public abstract class LayerService : AbstractLayerService
    {
        public override string name => $"Layer Service for {layerName}";
        public string layerName { get; }

        public LayerService(string layerName)
        {
            this.layerName = layerName;
        }
    }
}
