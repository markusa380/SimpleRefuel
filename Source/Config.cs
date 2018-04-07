using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRefuel {
	public class Config {
		private const string settingsNodeName = "SimpleRefuel_Settings";
		private static Config instance;

		private static string ConfigFilePath {
			get { return KSPUtil.ApplicationRootPath + "GameData/SimpleRefuel/SimpleRefuel.cfg"; }
		}

		internal static Config Instance {
			get { return instance ?? ( instance = new Config() ); }
		}

		private Config() {
			this.ResourcesCost = new Dictionary<string, float>();
			this.Load();
		}

		internal float RefuelSpeed;
		internal Dictionary<String, float> ResourcesCost;

		private void Load() {
			var node = ConfigNode.Load(ConfigFilePath);
			var settings = node.GetNode(settingsNodeName);
			this.RefuelSpeed = float.Parse(settings.GetValue("RefuelSpeed"));

			var resCosts = settings.GetValue("ResourceCost").Split(';');
            foreach (var resCost in resCosts) {                
				var parts = resCost.Split(',');
                this.ResourcesCost.Add(parts[0], float.Parse(parts[1]));
				
			}
		}
	}
}
