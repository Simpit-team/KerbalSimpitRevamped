using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KerbalSimpit.Serial;

namespace KerbalSimpit.SimpitGUI
{
	// Start at main menu
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class AppButton : MonoBehaviour
	{
		const ApplicationLauncher.AppScenes buttonScenes = ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW;
		private static ApplicationLauncherButton button;

		private static Texture2D iconRed, iconOrange, iconGreen;

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
			iconRed = GameDatabase.Instance.GetTexture("KerbalSimpit/Simpit_icon_red", false);
			iconOrange = GameDatabase.Instance.GetTexture("KerbalSimpit/Simpit_icon_orange", false);
			iconGreen = GameDatabase.Instance.GetTexture("KerbalSimpit/Simpit_icon_green", false);

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
				button = ApplicationLauncher.Instance.AddModApplication(onToggle, onToggle, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, iconRed);
				UpdateVisibility();
			}
		}

		void Update()
		{
			if (button == null) return; // button not yet initialised ?

			KSPSerialPort.ConnectionStatus status = KSPit.SerialPorts[0].portStatus;
			if (status == KSPSerialPort.ConnectionStatus.CLOSED || status == KSPSerialPort.ConnectionStatus.ERROR)
			{
				button.SetTexture(iconRed);
			}
			else if (status == KSPSerialPort.ConnectionStatus.WAITING_HANDSHAKE || status == KSPSerialPort.ConnectionStatus.HANDSHAKE)
			{
				button.SetTexture(iconOrange);
			}
			else if (status == KSPSerialPort.ConnectionStatus.CONNECTED || status == KSPSerialPort.ConnectionStatus.IDLE)
			{
				button.SetTexture(iconGreen);
			}
			else
			{
				//All cases should be covered, this should not happen.
				button.SetTexture(iconRed);
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
