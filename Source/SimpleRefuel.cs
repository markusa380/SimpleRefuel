using System;
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
        bool refuelling = false;
        Vessel vessel;
        int i;
        int o;

        void Reset()
        {
            refuelling = false;
            i = 0;
            o = 0;
        }

        void Awake()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);

            guid = Guid.NewGuid().GetHashCode();

            WindowRect = new Rect(Screen.width / 2f - 40f, Screen.height * 0.6f, 80f, 10f);
        }

        void Update()
        {
            if(FlightGlobals.ActiveVessel != vessel)
            {
                Reset();
                vessel = FlightGlobals.ActiveVessel;
            }

            if( Math.Round(vessel.srf_velocity.magnitude) == 0f && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH) && (vessel.transform.position - SpaceCenter.Instance.transform.position).magnitude <= SpaceCenter.Instance.AreaRadius  )
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

                if (part.Resources.list.Count > 0)
                {
                    PartResource r = part.Resources.list[o];

                    if ((r.resourceName == "LiquidFuel" || r.resourceName == "Oxidizer" || r.resourceName == "ElectricCharge" || r.resourceName == "MonoPropellant") && r.amount < r.maxAmount)
                    {
                        r.amount += 10f * TimeWarp.deltaTime; // Charge 10 units per second
                        still_refuelling = true;
                    }

                    if (r.amount >= r.maxAmount)
                    {
                        o++;
                        if (o >= part.Resources.list.Count)
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

                    if (i >= vessel.parts.Count - 1)
                    {
                        refuelling = false;
                        i = 0;
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
            if (GUILayout.Button("Refuel", new GUIStyle(Instantiate(HighLogic.Skin).button ), GUILayout.ExpandWidth(true)))
            {
                refuelling = true;
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
