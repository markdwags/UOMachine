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

namespace RazorLoader
{
    internal static unsafe class CryptPatcher
    {
        private static bool FindSignatureOffset(byte[] signature, byte* buffer, int bufLen, out int offset)
        {
            bool found = false;
            offset = 0;
            for (int x = 0; x < bufLen - signature.Length; x++)
            {
                for (int y = 0; y < signature.Length; y++)
                {
                    if (buffer[x + y] == signature[y])
                        found = true;
                    else
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    offset = x;
                    break;
                }
            }
            return found;
        }

        public static bool Patch(IntPtr hModule)
        {
            byte* address = (byte*)hModule;

            // Reximis 2012-02-11, Razor 1.0.13
/*
.text:100046D6                 cmp     byte_1002002F, bl
.text:100046DC                 jz      short loc_100046E7 <--------- CHANGE TO JMP
.text:100046DE                 pop     ebp
.text:100046DF                 lea     eax, [ebx+5] 
.text:100046E2                 pop     ebx
.text:100046E3                 add     esp, 28h
.text:100046E6                 retn
 */
            byte[] sig = new byte[] { 0x38, 0x1D, 0x2F, 0x00 };
            int offset = 0;

            if (FindSignatureOffset(sig, address, 0x10000, out offset))
            {
                address[offset + 0x6] = 0xEB;
            }
            else
            {
                byte[] sig2 = new byte[] { 0x57, 0x33, 0xFF, 0x80, 0x3D };
                offset = 0;

                if (FindSignatureOffset(sig2, address, 0x10000, out offset))
                {
                    address[offset + 0x0A] = 0xEB;
                }

                // Tested on 1.0.14.7, 2015-07-21
                //.text:10002F2D                 push    offset aInitializeLibr ; "Initialize library..."
                //.text:10002F32                 call    sub_100016C0
                //.text:10002F37                 add     esp, 4
                //.text:10002F3A                 movzx   eax, byte_1002902F
                //.text:10002F41                 test    eax, eax
                //.text:10002F43                 jz      short loc_10002F4F // Replace JZ with JMP to get to good code
                //.text:10002F45                 mov     eax, 5 // 5 = LIB_DISABLED
                //.text:10002F4A                 jmp     loc_1000321A // Pops stack and RETN's
                byte[] sig3 = new byte[] { 0x83, 0xC4, 0x04, 0x0F, 0xB6, 0x05, 0x2F, 0x90 };
                if (FindSignatureOffset(sig3, address, 0x10000, out offset))
                {
                    if (address[offset + 0x0C] == 0x74) address[offset + 0x0C] = 0xEB;
                }

            }
           
            return true;
        }
    }
}