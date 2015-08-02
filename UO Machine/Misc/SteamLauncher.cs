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
using System.IO;
using System.Text;
using UOMachine.Resources;

namespace UOMachine
{
    internal unsafe static class SteamLauncher
    {
        public static bool Launch( OptionsData options, out int index )
        {
            NativeMethods.SafeProcessHandle hProcess = new NativeMethods.SafeProcessHandle();
            NativeMethods.SafeThreadHandle hThread = new NativeMethods.SafeThreadHandle();
            NativeMethods.PROCESS_INFORMATION processInformation = new NativeMethods.PROCESS_INFORMATION();
            NativeMethods.STARTUPINFO startInfo = new NativeMethods.STARTUPINFO();

            Environment.SetEnvironmentVariable( "QT_QPA_PLATFORM_PLUGIN_PATH", Path.Combine( options.UOSFolder, "Platforms" ) );
            Environment.SetEnvironmentVariable( "PATH", options.UOSFolder );

            StringBuilder param = new StringBuilder();
            param.Append( String.Format( "-uo \"{0}\" ", options.UOFolder ) );
            param.Append( String.Format( "-core \"{0}\" ", options.UOSFolder ) );
            param.Append( String.Format( "-shard \"{0}\" ", options.Server ) );
            param.Append( String.Format( "-port \"{0}\" ", options.Port ) );
            param.Append( "-encryption \"Yes\"" );
            if (!options.PatchClientEncryptionUOM) param.Append( "-useEncryption \"Yes\"" );

            index = -1;

            UOM.SetStatusLabel( Strings.Launchingclient );

            if (NativeMethods.CreateProcess( options.UOClientPath, param, null, null, false, NativeMethods.CREATE_SUSPENDED, IntPtr.Zero, options.UOFolder, startInfo, processInformation ))
            {
                hThread.InitialSetHandle( processInformation.hThread );
                hProcess.InitialSetHandle( processInformation.hProcess );

                IntPtr hModule = NativeMethods.LoadLibrary( "kernel32.dll" );
                IntPtr loadLibrary = NativeMethods.GetProcAddress( hModule, "LoadLibraryA" );
                IntPtr getProcAddress = NativeMethods.GetProcAddress( hModule, "GetProcAddress" );

                NativeMethods.CONTEXT context = new NativeMethods.CONTEXT();
                context.ContextFlags = 0x10000 | 0x01 | 0x02 | 0x04;
                NativeMethods.GetThreadContext( hThread.DangerousGetHandle(), ref context );

                uint origEip = context.Eip;

                IntPtr dataAlloc = NativeMethods.VirtualAllocEx( hProcess.DangerousGetHandle(), IntPtr.Zero, (UIntPtr) 256, NativeMethods.AllocationType.Commit | NativeMethods.AllocationType.Reserve, NativeMethods.MemoryProtection.ExecuteReadWrite );
                if (dataAlloc == null)
                {
                    UOM.SetStatusLabel( Strings.Memoryallocationfailed );
                    return false;
                }

                int codePosition = 0;
                IntPtr dllPathAddress = dataAlloc + codePosition;
                string dllPath = Path.Combine( options.UOSFolder, "UOS.dll" ) + '\0';
                codePosition += dllPath.Length;

                Memory.Write( hProcess.DangerousGetHandle(), dllPathAddress, Encoding.ASCII.GetBytes( dllPath ), false );

                IntPtr dllFunctionAddress = dataAlloc + codePosition;
                string dllFunction = "Install\0";
                Memory.Write( hProcess.DangerousGetHandle(), dllFunctionAddress, Encoding.ASCII.GetBytes( dllFunction ), false );
                codePosition += dllFunction.Length;

                codePosition += 5;
                IntPtr codeStart = dataAlloc + codePosition;

                byte[] code = {
                        0x9C						 /* 00:00 PUSHFD */,
                        0x60						 /* 01:01 PUSHAD */,
                        0x68, 0x00, 0x00, 0x00, 0x00 /* 2:6 PUSH dllName */,
                        0xB8, 0x00, 0x00, 0x00, 0x00 /* 7:11 MOV eax, LoadLibraryA */,
                        0xFF, 0xD0					 /* 12:13 CALL eax */,
                        0xBB, 0x00, 0x00, 0x00, 0x00 /* 14:18 MOV ebx, dllFunc */,
                        0x53						 /* 19:19 PUSH ebx */,
                        0x50						 /* 20:20 PUSH eax */,
                        0xB9, 0x00, 0x00, 0x00, 0x00 /* 21:25 MOV ecx, GetProcAddress */,
                        0xFF, 0xD1					 /* 26:27 CALL ecx */,
                        0xFF, 0xD0					 /* 28:29 CALL eax */,
                        0x61						 /* 30:30 POPAD */,
                        0x9D						 /* 31:31 POPFD */,
                        0xE9, 0x00, 0x00, 0x00, 0x00 /* 32:36 JMP origEip */
                };

                IntPtr eipPtr = (IntPtr) origEip - ( (int) dataAlloc + codePosition + code.Length );

                Buffer.BlockCopy( BitConverter.GetBytes( (int) dllPathAddress ), 0, code, 3, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( (int) loadLibrary ), 0, code, 8, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( (int) dllFunctionAddress ), 0, code, 15, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( (int) getProcAddress ), 0, code, 22, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( (int) eipPtr ), 0, code, 33, 4 );

                Memory.Write( hProcess.DangerousGetHandle(), codeStart, code, false );

                context.Eip = (uint) codeStart;
                NativeMethods.SetThreadContext( hThread.DangerousGetHandle(), ref context );
                NativeMethods.ResumeThread( hThread.DangerousGetHandle() );

                hProcess.Dispose();
                hThread.Dispose();

                return ClientLauncher.Attach( processInformation.dwProcessId, options, true, out index );
            }
            UOM.SetStatusLabel( Strings.Errorstartingclient );
            return false;
        }
    }
}
