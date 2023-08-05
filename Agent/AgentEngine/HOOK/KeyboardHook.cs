using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AgentEngine
{

    /// <summary>
    /// Captures global keyboard events
    /// </summary>
    public class KeyboardHook : GlobalHook
    {        

        #region Events

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event KeyPressEventHandler KeyPress;

        #endregion

        #region Constructor

        public KeyboardHook()
        {
            _hookType = WH_KEYBOARD_LL;
        }

        #endregion

        #region Methods

        protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
        {
            bool handled = false;

            if (nCode > -1 && (KeyDown != null || KeyUp != null || KeyPress != null))
            {

                if (wParam == WM_SYSKEYDOWN || wParam == WM_SYSKEYUP)
                {
                    return 1;
                }

                KeyboardHookStruct keyboardHookStruct =
                    (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

                if (keyboardHookStruct.dwExtraInfo != CUSTSKEYSTROKE)
                    return 1;

                byte[] keyState = new byte[256];
                //GetKeyboardState(keyState);

                //keyState[VK_LALT] = 0;
                //keyState[VK_RALT] = 0;
                //keyState[VK_LCONTROL] = 0;
                //keyState[VK_RCONTROL] = 0;
                //SetKeyboardState(keyState);
                

                // Is Alt being held down?
                bool alt = ((GetKeyState(VK_LALT) & 0x80) != 0) ||
                           ((GetKeyState(VK_RALT) & 0x80) != 0);
                if (alt)
                    MessageBox.Show("alt");

                // Is Control being held down?
                //bool control = ((GetKeyState(VK_LCONTROL) & 0x80) != 0) ||
                //               ((GetKeyState(VK_RCONTROL) & 0x80) != 0);
                //control = false;

                // Is Shift being held down?
                bool shift = ((GetKeyState(VK_LSHIFT) & 0x80) != 0) ||
                             ((GetKeyState(VK_RSHIFT) & 0x80) != 0);
                shift = false;

                // Is CapsLock on?
                //bool capslock = (GetKeyState(VK_CAPITAL) != 0);
                //capslock = false;

                // Create event using keycode and control/shift/alt values found above
                KeyEventArgs e = new KeyEventArgs(
                    (Keys)(
                        keyboardHookStruct.vkCode |
                        //(control ? (int)Keys.Control : 0) |
                        (shift ? (int)Keys.Shift : 0)// |
                        //(alt ? (int)Keys.Alt : 0)
                        ));

                // Handle KeyDown and KeyUp events
                switch (wParam)
                {
                    case WM_KEYDOWN:
                        if (KeyDown != null)
                        {
                            KeyDown(this, e);
                            handled = handled || e.Handled;
                        }
                        break;
                    case WM_KEYUP:
                        if (KeyUp != null)
                        {
                            KeyUp(this, e);
                            handled = handled || e.Handled;
                        }
                        break;
                    //default:
                    //    return 1;
                }
            }
            else
            {
                return 1;
            }

            if (handled)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(_handleToHook, nCode, wParam, lParam);
            }
        }
        #endregion
    }
}
