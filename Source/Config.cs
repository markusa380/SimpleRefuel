using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRefuel {
	public class Config {
		private const string _configFilePath = "GameData/SimpleRefuel/SimpleRefuel.cfg";
		private const string SettingsNodeName = "SimpleRefuel_Settings";
		private static Config _instance;

		private static string ConfigFilePath {
			get { return KSPUtil.ApplicationRootPath + _configFilePath; }
		}

		internal static Config Instance {
			get { return _instance ?? ( _instance = new Config() ); }
		}

		private Config() {
			this._load();
		}

		internal float RefuelSpeed;

		private void _load() {
			var node = ConfigNode.Load(ConfigFilePath);
			var settings = node.GetNode(SettingsNodeName);
			this.RefuelSpeed = float.Parse(settings.GetValue("RefuelSpeed"));
		}
	}
}
