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

using System.Text;
using System.IO;

namespace UOMachine.Data
{
    public static class TileData
    {
        private static InternalLandTile[] myLandTiles;
        private static InternalStaticTile[] myStaticTiles;
        private static bool oldFormat = false;

        private static void LoadLandTiles(byte[] data, InternalLandTile[] landTiles)
        {
            MemoryStream ms = new MemoryStream(data, false);
            BinaryReader bin = new BinaryReader(ms);

            // TODO: Is this robust enough?
            ms.Seek(36, SeekOrigin.Begin);
            string name = ASCIIEncoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
            if (name == "VOID!!!!!!")
                oldFormat = true;
            ms.Seek(0, SeekOrigin.Begin);

            if (oldFormat)
            {
                for (int i = 0; i < 0x4000; ++i)
                {
                    if (i == 0 || (i > 0 && (i & 0x1f) == 0))
                    {
                        bin.ReadInt32(); // block header
                    }

                    landTiles[i].Flags = (TileFlags)bin.ReadInt32();
                    landTiles[i].ID = bin.ReadInt16();
                    landTiles[i].Name = ASCIIEncoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
                }
            }
            else
            {
                for (int i = 0; i < 0x4000; ++i)
                {
                    if (i == 1 || (i > 0 && (i & 0x1f) == 0))
                    {
                        bin.ReadInt32(); // block header
                    }

                    landTiles[i].Flags = (TileFlags)bin.ReadInt64();
                    landTiles[i].ID = bin.ReadInt16();
                    landTiles[i].Name = ASCIIEncoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
                }
            }

            ms.Close();
        }

        private static void LoadStaticTiles(byte[] data, InternalStaticTile[] staticTiles)
        {
            MemoryStream ms = new MemoryStream(data, false);
            BinaryReader bin = new BinaryReader(ms);

            if (oldFormat)
            {
                int offset = 428032;

                ms.Seek(offset, SeekOrigin.Begin);

                for (int i = 0; i < 0x8000; ++i)
                {
                    if ((i & 0x1F) == 0)
                    {
                        bin.ReadInt32(); // header
                    }

                    staticTiles[i].ID = (ushort)i;
                    staticTiles[i].Flags = (TileFlags)bin.ReadInt32();
                    staticTiles[i].Weight = bin.ReadByte();
                    int quality = bin.ReadByte();
                    bin.ReadInt16();
                    bin.ReadByte();
                    staticTiles[i].Quantity = bin.ReadByte();
                    bin.ReadInt16();
                    bin.ReadByte();
                    bin.ReadByte();
                    bin.ReadInt16();
                    int height = bin.ReadByte();
                    staticTiles[i].Name = ASCIIEncoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
                }
            }
            else
            {
                int offset = 493568;

                ms.Seek(offset, SeekOrigin.Begin);

                for (int i = 0; i < 0x10000; ++i)
                {
                    if ((i & 0x1F) == 0)
                    {
                        bin.ReadInt32(); // header
                    }

                    staticTiles[i].ID = (ushort)i;
                    staticTiles[i].Flags = (TileFlags)bin.ReadInt64();
                    staticTiles[i].Weight = bin.ReadByte();
                    int quality = bin.ReadByte();
                    bin.ReadInt16();
                    bin.ReadByte();
                    staticTiles[i].Quantity = bin.ReadByte();
                    bin.ReadInt32();
                    bin.ReadByte();
                    int value = bin.ReadByte();
                    int height = bin.ReadByte();
                    staticTiles[i].Name = ASCIIEncoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
                }
            }

            ms.Close();
        }

        /// <summary>
        /// Load tiledata.mul from specified UO installation into memory.
        /// </summary>
        internal static void Initialize(string dataFolder)
        {
            string fileName = Path.Combine(dataFolder, "tiledata.mul");
            if (!File.Exists(fileName)) throw new FileNotFoundException(string.Format("File {0} doesn't exist!", fileName));
            byte[] fileBytes = File.ReadAllBytes(fileName);
            myLandTiles = new InternalLandTile[16384];
            myStaticTiles = new InternalStaticTile[(fileBytes.Length - 428032) / 1188 * 32];
            oldFormat = false;
            LoadLandTiles(fileBytes, myLandTiles);
            LoadStaticTiles(fileBytes, myStaticTiles);
        }

        internal static void Dispose()
        {
            myLandTiles = null;
            myStaticTiles = null;
        }

        /// <summary>
        /// Get specified land tile.
        /// </summary>
        internal static InternalLandTile GetLandTile(int index)
        {
            return myLandTiles[index];
        }

        /// <summary>
        /// Get specified static tile.
        /// </summary>
        internal static InternalStaticTile GetStaticTile(int index)
        {
            return myStaticTiles[index];
        }

        public static void DumpAllTiles(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Land tiles:");
            for (int x = 0; x < myLandTiles.Length; x++)
            {
                sb.AppendLine("index " + x + " = ID " + myLandTiles[x].ID + " = " + myLandTiles[x].Name);
            }
            sb.AppendLine("\r\n***********************************************************");
            sb.AppendLine("Static tiles:");
            for (int x = 0; x < myStaticTiles.Length; x++)
            {
                sb.AppendLine( x + " = " + myStaticTiles[x].Name);
            }
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}