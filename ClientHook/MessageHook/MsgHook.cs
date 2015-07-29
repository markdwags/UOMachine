/* Copyright (C) 2009 Matthew Geyer
 * 
 * This file is part of UO Machine.
 * 
 * UO Machine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * UO Machine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with UO Machine.  If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Windows.Forms;

namespace ClientHook
{
    static unsafe partial class MessageHook
    {
        private static uint myLastButton = 0;
        private static DateTime myLastMessage = DateTime.Now;
        private const double myMessageDelay = 100;
        private static int myWindowX = 0, myWindowY = 0;

        private static void NormalizePoint(ref NativeMethods.POINT pt)
        {
            pt.x -= myWindowX;
            pt.y -= myWindowY;
        }

        public static IntPtr WindowMsgHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //CWPSTRUCT cwps = (CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPSTRUCT));
                NativeMethods.CWPSTRUCT cwps = *(NativeMethods.CWPSTRUCT*)lParam;
                switch (cwps.message)
                {
                    case NativeMethods.WM_MOVE:
                        int lp = cwps.lparam.ToInt32();
                        myWindowX = lp & 0xFFFF;
                        myWindowY = (int)(lp & 0xFFFF0000) >> 16;
                        break;
                }

            }
            return NativeMethods.CallNextHookEx(myMsgHookHandle, nCode, wParam, lParam);
        }

        public static IntPtr MsgHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)//&& wParam == PM_NOREMOVE)
            {
                //MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                NativeMethods.MSG msg = *(NativeMethods.MSG*)lParam;
                switch (msg.message)
                {
                    case NativeMethods.WM_KEYDOWN:
                        OnKeyDown((Keys)msg.wParam);
                        break;
                    case NativeMethods.WM_KEYUP:
                        OnKeyUp((Keys)msg.wParam);
                        break;
                    case NativeMethods.WM_SYSKEYDOWN:
                        if (msg.wParam.ToInt32() == 0x12) OnKeyDown(Keys.Alt);
                        else OnKeyDown((Keys)msg.wParam);
                        break;
                    case NativeMethods.WM_SYSKEYUP:
                        if (msg.wParam.ToInt32() == 0x12) OnKeyUp(Keys.Alt);
                        else OnKeyUp((Keys)msg.wParam);
                        break;
                    case NativeMethods.WM_MOUSEMOVE:
                        DateTime now = DateTime.Now;
                        TimeSpan elapsed = now - myLastMessage;
                        if (elapsed.TotalMilliseconds < myMessageDelay) break;
                        NormalizePoint(ref msg.pt);
                        myLastMessage = now;
                        OnMouseMove(msg.pt.x, msg.pt.y);
                        break;
                    case NativeMethods.WM_LBUTTONDOWN:
                        myLastButton = msg.message;
                        NormalizePoint(ref msg.pt);
                        OnMouseDown(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Left);
                        break;
                    case NativeMethods.WM_LBUTTONUP:
                        NormalizePoint(ref msg.pt);
                        OnMouseUp(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Left);
                        if (myLastButton == NativeMethods.WM_LBUTTONDOWN) OnMouseClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Left);
                        myLastButton = 0;
                        break;
                    case NativeMethods.WM_LBUTTONDBLCLK:
                        myLastButton = 0;
                        NormalizePoint(ref msg.pt);
                        OnMouseDoubleClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Left);
                        break;
                    case NativeMethods.WM_RBUTTONDOWN:
                        myLastButton = msg.message;
                        NormalizePoint(ref msg.pt);
                        OnMouseDown(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Right);
                        break;
                    case NativeMethods.WM_RBUTTONUP:
                        NormalizePoint(ref msg.pt);
                        OnMouseUp(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Right);
                        if (myLastButton == NativeMethods.WM_RBUTTONDOWN) OnMouseClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Right);
                        myLastButton = 0;
                        break;
                    case NativeMethods.WM_RBUTTONDBLCLK:
                        myLastButton = 0;
                        NormalizePoint(ref msg.pt);
                        OnMouseDoubleClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Right);
                        break;
                    case NativeMethods.WM_MBUTTONDOWN:
                        myLastButton = msg.message;
                        NormalizePoint(ref msg.pt);
                        OnMouseDown(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Middle);
                        break;
                    case NativeMethods.WM_MBUTTONUP:
                        NormalizePoint(ref msg.pt);
                        OnMouseUp(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Middle);
                        if (myLastButton == NativeMethods.WM_MBUTTONDOWN) OnMouseClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Middle);
                        myLastButton = 0;
                        break;
                    case NativeMethods.WM_MBUTTONDBLCLK:
                        myLastButton = 0;
                        NormalizePoint(ref msg.pt);
                        OnMouseDoubleClick(msg.pt.x, msg.pt.y, UOMachine.IPC.MouseButton.Middle);
                        break;
                    case NativeMethods.WM_MOUSEWHEEL:
                        short delta = (short)((msg.wParam.ToInt32() >> 16) & 0xFFFF);
                        delta /= NativeMethods.WHEEL_DELTA;
                        myLastButton = 0;
                        NormalizePoint(ref msg.pt);
                        OnMouseWheel(msg.pt.x, msg.pt.y, (byte)delta);
                        break;
                }
            }
            return NativeMethods.CallNextHookEx(myMsgHookHandle, nCode, wParam, lParam);
        }

    }
}