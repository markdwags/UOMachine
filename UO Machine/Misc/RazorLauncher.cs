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

using System.Diagnostics;
//using System.Windows.Forms;
using System.Windows;
using System.IO;
using UOMachine.Resources;

namespace UOMachine
{
    internal static class RazorLauncher
    {
        public static bool Launch( OptionsData options, out int index )
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = MainWindow.CurrentOptions.UOFolder;
            startInfo.FileName = MainWindow.CurrentOptions.UOClientPath;
            NativeMethods.SafeProcessHandle hProcess;
            NativeMethods.SafeThreadHandle hThread;
            uint pid, tid;
            UOM.SetStatusLabel( Strings.Launchingclient );
            index = -1;
            if (NativeMethods.CreateProcess( startInfo, true, out hProcess, out hThread, out pid, out tid ))
            {
                UOM.SetStatusLabel( Strings.Patchingclient );

                if (!ClientPatcher.MultiPatch( hProcess.DangerousGetHandle() ))
                {
                    UOM.SetStatusLabel( Strings.MultiUOpatchfailed );
                    hProcess.Dispose();
                    hThread.Dispose();
                    return false;
                }

                if (NativeMethods.ResumeThread( hThread.DangerousGetHandle() ) == -1)
                {
                    UOM.SetStatusLabel( Strings.ResumeThreadfailed );
                    hProcess.Dispose();
                    hThread.Dispose();
                    return false;
                }

                hProcess.Close();
                hThread.Close();
                startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = options.RazorFolder;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = Path.Combine( UOM.StartupPath, "RazorLoader.exe" );
                string args = "--server " + options.Server + "," + options.Port.ToString();
                args += " --path " + options.RazorFolder;
                if (!options.PatchClientEncryption)
                    args += " --clientenc";
                if (options.EncryptedServer)
                    args += " --serverenc";
                args += " --pid " + pid.ToString();
                startInfo.Arguments = args;
                Process p = new Process();
                p.StartInfo = startInfo;
                p.Start();

                if (ClientLauncher.Attach( pid, options, true, out index ))
                {
                    UOM.SetStatusLabel( Strings.Razorsuccessfullylaunched );
                    return true;
                }
                else
                {
                    UOM.SetStatusLabel( Strings.ErrorattachingtoRazorclient );
                    MessageBox.Show( Strings.ErrorattachingtoRazorclient, Strings.Error );
                }
            }
            return false;
        }
    }
}