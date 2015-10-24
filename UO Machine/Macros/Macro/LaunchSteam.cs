/* Copyright (C) 2015 John Scott
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


namespace UOMachine.Macros
{
    public static partial class Macro
    {

        /// <summary>
        /// Launch a new UOSteam instance.
        /// </summary>
        /// <param name="clientIndex">Client index of launched client.</param>
        /// <returns>True on success.</returns>
        public static bool LaunchSteam( out int clientIndex )
        {
            return SteamLauncher.Launch( MainWindow.CurrentOptions, out clientIndex );
        }

        /// <summary>
        /// Launch a new UOSteam instance.
        /// </summary>
        /// <param name="options">UOMachine.OptionsData to use for launching Razor.</param>
        /// <param name="clientIndex">Client index of launched client.</param>
        /// <returns>True on success.</returns>
        public static bool LaunchSteam( OptionsData options, out int clientIndex )
        {
            return SteamLauncher.Launch( options, out clientIndex );
        }
    }
}