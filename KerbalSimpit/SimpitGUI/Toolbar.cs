using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace KerbalSimpit.SimpitGUI
{
	// Start at main menu
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class AppButton : MonoBehaviour
	{
		const ApplicationLauncher.AppScenes buttonScenes = ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT;
		private static ApplicationLauncherButton button;

		public static Callback Toggle = delegate { };

		static bool buttonVisible
		{
			get
			{
				return true;
			}
		}

		public void UpdateVisibility()
		{
			if (button != null)
			{
				button.VisibleInScenes = buttonVisible ? buttonScenes : 0;
			}
		}

		private static void onToggle()
		{
			Toggle();
		}

		public void Start()
		{
			GameObject.DontDestroyOnLoad(this);
			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
		}

		void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
		}

		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready && button == null)
			{
				var tex = GameDatabase.Instance.GetTexture("KerbalSimpit/icon_simpit", false);
				button = ApplicationLauncher.Instance.AddModApplication(onToggle, onToggle, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, tex);
				UpdateVisibility();
			}
		}
	}


	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Toolbar_Flight : MonoBehaviour
	{
		public void Awake()
		{
			AppButton.Toggle += WindowFlight.ToggleGUI;
		}

		void OnDestroy()
		{
			AppButton.Toggle -= WindowFlight.ToggleGUI;
		}
	}

	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class Toolbar_SpaceCenter : MonoBehaviour
	{
		public void Awake()
		{
			AppButton.Toggle += WindowSpaceCenter.ToggleGUI;
		}

		void OnDestroy()
		{
			AppButton.Toggle -= WindowSpaceCenter.ToggleGUI;
		}
	}
}
