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
			this.Load();
		}

		internal float RefuelSpeed;

		private void Load() {
			var node = ConfigNode.Load(ConfigFilePath);
			var settings = node.GetNode(settingsNodeName);
			this.RefuelSpeed = float.Parse(settings.GetValue("RefuelSpeed"));
		}
	}
}
