/* Copyright (C) 2014 John Scott
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
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.IO;
using System.Net;
using System.Text;
using UOMachine.IPC;
using UOMachine.Resources;
using EasyHook;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace UOMachine.Misc
{
    internal unsafe static class SteamLauncher
    {
        [DllImport("Loader.dll")]
        private static unsafe extern uint Load(string exe, string path, string dll, string dllpath, string func, string param, out uint pid);

        private static object myLock = new object();

        public static bool Launch( OptionsData options, out int index )
        {
            uint pid;
            StringBuilder param = new StringBuilder();
            param.Append(String.Format("-uo \"{0}\" ", options.UOFolder));
            param.Append(String.Format("-core \"{0}\" ", options.UOSFolder));
            param.Append(String.Format("-shard \"{0}\" ", options.Server));
            param.Append(String.Format("-port \"{0}\" ", options.Port));
            if (options.PatchClientEncryption) param.Append("-encryption \"Yes\"");

            Load(options.UOClientPath, options.UOFolder, Path.Combine(options.UOSFolder, "UOS.dll"), options.UOSFolder, "Install", param.ToString(), out pid);
            return ClientLauncher.Attach(pid, options, true, out index);
        }
    }
}
