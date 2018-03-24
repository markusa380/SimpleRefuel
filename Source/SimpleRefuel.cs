using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleRefuel
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Refuel : MonoBehaviour
    {
        /* GUI FIELDS */
        private int guid;
        private Rect WindowRect;
        private bool visible = true;
        private GUIStyle style = GUIStyle.none;
        private GUIStyle style2 = new GUIStyle(Instantiate(HighLogic.Skin).button);

        /* PRIVATE FIELDS */
        private bool refuelling = false;
        private Vessel vessel;
        private int i;
        private int o;
        private bool select_fuel = false;
        private List<string> resources = new List<string>();
        private int current_resource = 0;
        private bool canRefuel = false;

        /* TOOLBAR FIELDS */
        private ApplicationLauncherButton button;
        private readonly string icon_green = "SimpleRefuel/Textures/icon_green";
        private readonly string icon_red = "SimpleRefuel/Textures/icon_red";

		/* RESOURCES COST */
		Dictionary<string, float> resourcesCost = new Dictionary<string, float> { //From: https://wiki.kerbalspaceprogram.com/wiki/Resource
			{ "ElectricCharge", 0.0f  }, 
			{ "LiquidFuel",     0.8f  },
			{ "Oxidizer",       0.18f },
			{ "IntakeAir",      0.0f  },
			{ "SolidFuel",      0.6f  },
			{ "MonoPropellant", 1.2f  },
			{ "XenonGas	",      4.0f  },
			{ "Ore",            0.02f },
			{ "Ablator",        0.5f  }
		};
		
		/* MONOBEHAVIOUR METHODS */
		void Awake()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);

            guid = Guid.NewGuid().GetHashCode();

            WindowRect = new Rect(Screen.width / 2f - 100f, Screen.height * 0.6f, 200f, 10f);

            CreateStockToolbar();
        }
        void Update()
        {
            if(FlightGlobals.ActiveVessel != vessel)
            {
                Reset();
                vessel = FlightGlobals.ActiveVessel;
            }

            if ((int)vessel.srfSpeed == 0 && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH) && (vessel.transform.position - SpaceCenter.Instance.transform.position).sqrMagnitude <= SpaceCenter.Instance.AreaRadius * SpaceCenter.Instance.AreaRadius  )
            {
                canRefuel = true;
                ChangeButtonTexture(icon_green);
            }
            else
            {
                canRefuel = false;
                ChangeButtonTexture(icon_red);
                Reset();
            }

            if(canRefuel && refuelling)
            {
                Part part = FlightGlobals.ActiveVessel.parts[i];

                bool still_refuelling = false;

                if (part.Resources.dict.Count > 0)
                {
                    PartResource r = part.Resources.dict.At(o);

                    if (r.resourceName == resources[current_resource] && r.amount < r.maxAmount)
                    {
                        if (r.maxAmount - r.amount > Config.Instance.RefuelSpeed * TimeWarp.deltaTime){				
							if(Funding.CanAfford(Config.Instance.RefuelSpeed * resourcesCost[r.resourceName] * TimeWarp.deltaTime)){
								r.amount += Config.Instance.RefuelSpeed * TimeWarp.deltaTime; // Charge RefuelSpeed units per second	
								Funding.Instance.AddFunds(-Config.Instance.RefuelSpeed * resourcesCost[r.resourceName] * TimeWarp.deltaTime, TransactionReasons.Any);
							}
						}
                        else {
							if(Funding.CanAfford((float)(r.maxAmount - r.amount) * resourcesCost[r.resourceName])){
								r.amount = r.maxAmount;	
								Funding.Instance.AddFunds(-(r.maxAmount - r.amount) * resourcesCost[r.resourceName], TransactionReasons.Any);
							}
						}
                        
                        still_refuelling = true;
                    }
                    if (r.amount >= r.maxAmount)
                    {
                        o++;
                        if (o >= part.Resources.dict.Count)
                        {
                            still_refuelling = false;
                            o = 0;
                        }
                        else
                        {
                            still_refuelling = true;
                        }
                    }
                }
                if (!still_refuelling)
                {
                    i++;
                    o = 0;

                    if (i >= vessel.parts.Count)
                    {
                        Reset();                        
                    }
                }
            }
        }
        void OnDestroy()
        {
            DeleteStockToolbar();
        }

        /* GUI METHODS */
        void OnGUI()
        {
            if(canRefuel && select_fuel && visible && !refuelling)
            {
                WindowRect = GUILayout.Window(guid, WindowRect, GUI, "", style);
            }
        }
        void GUI(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("<", style2, GUILayout.Width(20)))
            {
                current_resource--;
                if (current_resource < 0)
                    current_resource = resources.Count - 1;
            }
            if (GUILayout.Button(resources[current_resource], style2, GUILayout.Width(160)))
            {
                    refuelling = true;
            }
            if (GUILayout.Button(">", style2, GUILayout.Width(20)))
            {
                current_resource++;
                if (current_resource > resources.Count - 1)
                    current_resource = 0;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        void OnShowUI()
        {
            visible = true;
        }
        void OnHideUI()
        {
            visible = false;
        }

        /* PRIVATE METHODS */
        private void Reset()
        {
            refuelling = false;
            i = 0;
            o = 0;
            select_fuel = false;
            resources.Clear();
        }
        private void Open()
        {
            resources.Clear();
            foreach (Part p in vessel.Parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (!resources.Contains(r.resourceName))
                    {
                        resources.Add(r.resourceName);
                    }
                }
            }

            if (canRefuel && !select_fuel)
            {
                if (resources.Count > 0)
                    select_fuel = true;
            }
            else
            {
                Reset();
            }
        }

        /* TOOLBAR METHODS */
        private void CreateStockToolbar()
        {
            if (!button)
            {
                button = ApplicationLauncher.Instance.AddModApplication(
                () =>
                {
                    Open();
                },
                () =>
                {
                    Open();
                },
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.FLIGHT,
                GameDatabase.Instance.GetTexture(icon_red, false)
                );
            }
        }
        private void DeleteStockToolbar()
        {
            if (button)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
                button = null;
            }
        }
        private void ChangeButtonTexture(string path)
        {
            button.SetTexture(GameDatabase.Instance.GetTexture(path, false));
		}
	}
}
