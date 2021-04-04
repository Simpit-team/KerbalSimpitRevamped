using KerbalSimpit.Utilities;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;

namespace KerbalSimpit.KerbalSimpit.External
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class KeyboardEmulator : MonoBehaviour
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct KeyboardEmulatorStruct
        {
            public byte modifier;
            public Int16 key;
        }

        private EventData<byte, object> keyboardEmulatorEvent;

        private InputSimulator input;

        public void Start()
        {
            keyboardEmulatorEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.KeyboardEmulator);
            if (keyboardEmulatorEvent != null) keyboardEmulatorEvent.Add(KeyboardEmulatorCallback);

            input = new InputSimulator();
        }

        public void OnDestroy()
        {
            if (keyboardEmulatorEvent != null) keyboardEmulatorEvent.Remove(KeyboardEmulatorCallback);
        }

        public void KeyboardEmulatorCallback(byte ID, object Data)
        {
            KeyboardEmulatorStruct payload = KerbalSimpitUtils.ByteArrayToStructure<KeyboardEmulatorStruct>((byte[])Data);

            Int32 key32 = payload.key; //To cast it in the enum, we need a Int32 but only a Int16 is sent

            if (Enum.IsDefined(typeof(VirtualKeyCode), key32)){
                VirtualKeyCode key = (VirtualKeyCode) key32;
                if ((payload.modifier & KeyboardEmulatorModifier.ALT_MOD) != 0)
                {
                    input.Keyboard.KeyDown(VirtualKeyCode.MENU);
                }

                if ((payload.modifier & KeyboardEmulatorModifier.CTRL_MOD) != 0)
                {
                    input.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                }

                if ((payload.modifier & KeyboardEmulatorModifier.SHIFT_MOD) != 0)
                {
                    input.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                }

                Debug.Log("Simpit emulates keypress of " + key);
                input.Keyboard.KeyPress(key);

                if ((payload.modifier & KeyboardEmulatorModifier.ALT_MOD) != 0)
                {
                    input.Keyboard.KeyUp(VirtualKeyCode.MENU);
                }

                if ((payload.modifier & KeyboardEmulatorModifier.CTRL_MOD) != 0)
                {
                    input.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                }

                if ((payload.modifier & KeyboardEmulatorModifier.SHIFT_MOD) != 0)
                {
                    input.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                }
            } else
            {
                Debug.Log("Simpit : I received a message to emulate a keypress of key " + payload.key + " but I do not recognize it. I ignore it.");
            }



        }

    }




}
