using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleRefuel
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Refuel : MonoBehaviour
    {
        public bool canRefuel = false;

        int guid;
        Rect WindowRect;
        bool visible = true;
        GUIStyle style = GUIStyle.none;
        GUIStyle style2 = new GUIStyle(Instantiate(HighLogic.Skin).button);
        bool refuelling = false;
        Vessel vessel;
        int i;
        int o;
        bool select_fuel = false;

        List<string> resources = new List<string>();
        int current_resource = 0;

        void Reset()
        {
            refuelling = false;
            i = 0;
            o = 0;
            select_fuel = false;
        }

        void Awake()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);

            guid = Guid.NewGuid().GetHashCode();

            WindowRect = new Rect(Screen.width / 2f - 100f, Screen.height * 0.6f, 200f, 10f);
        }

        void Update()
        {
            if(FlightGlobals.ActiveVessel != vessel)
            {
                Reset();
                vessel = FlightGlobals.ActiveVessel;
            }

            if( (int)vessel.srfSpeed == 0 && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH) && (vessel.transform.position - SpaceCenter.Instance.transform.position).sqrMagnitude <= SpaceCenter.Instance.AreaRadius * SpaceCenter.Instance.AreaRadius  )
            {
                canRefuel = true;
            }
            else
            {
                canRefuel = false;
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
                        if (r.maxAmount - r.amount > 10f * TimeWarp.deltaTime)
                            r.amount += 10f * TimeWarp.deltaTime; // Charge 10 units per second
                        else
                            r.amount = r.maxAmount;
                        
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

        void OnGUI()
        {
            if(canRefuel && visible && !refuelling)
            {
                WindowRect = GUILayout.Window(guid, WindowRect, GUI, "", style);
            }
        }

        void GUI(int id)
        {
            GUILayout.BeginVertical();
            if (!select_fuel)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(60);
                if (GUILayout.Button("Refuel", style2, GUILayout.ExpandWidth(true)))
                {
                    select_fuel = true;
                    resources.Clear();
                    foreach(Part p in FlightGlobals.ActiveVessel.Parts)
                    {
                        foreach(PartResource r in p.Resources)
                        {
                            if(!resources.Contains(r.resourceName))
                            {
                                resources.Add(r.resourceName);
                            }
                        }
                    }
                }
                GUILayout.Space(60);
                GUILayout.EndHorizontal();
            }
            else
            {
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

            }
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
    }
}
