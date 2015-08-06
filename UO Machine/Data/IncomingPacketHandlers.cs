﻿/* Copyright (C) 2015 John Scott
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
using System.IO;
using System.Text;
using UOMachine.Events;
using UOMachine.Utility;

namespace UOMachine.Data
{
    internal static class IncomingPacketHandlers
    {
        private static PacketHandler[] m_Handlers;
        private static PacketHandler[] m_ExtendedHandlers;

        internal static void Initialize()
        {
            m_Handlers = new PacketHandler[0x100];
            m_ExtendedHandlers = new PacketHandler[0x100];

            Register(0x01,  5, new OnPacketReceive(OnLoggedOut));
            Register(0x0B,  7, new OnPacketReceive(OnDamage));
            Register(0x11,  0, new OnPacketReceive(OnMobileStatus));
            Register(0x1A,  0, new OnPacketReceive(OnWorldItem));
            Register(0x1B, 37, new OnPacketReceive(OnInitializePlayer));
            Register(0x1C,  0, new OnPacketReceive(OnASCIIMessage));
            Register(0x1D,  5, new OnPacketReceive(OnItemDeleted));
            Register(0x20, 19, new OnPacketReceive(OnMobileUpdated));
            Register(0x21,  8, new OnPacketReceive(OnMoveRejected));
            Register(0x22,  3, new OnPacketReceive(OnMoveAccepted));
            Register(0x24,  9, new OnPacketReceive(OnStandardGump));
            Register(0x25, 21, new OnPacketReceive(OnItemAddedToContainer));
            Register(0x2C,  2, new OnPacketReceive(OnPlayerDeath));
            Register(0x2E, 15, new OnPacketReceive(OnItemEquipped));
            Register(0x2F, 10, new OnPacketReceive(OnAttackSwing));
            Register(0x30,  5, new OnPacketReceive(OnAttackGranted));
            Register(0x3A,  0, new OnPacketReceive(OnSkillsList));
            Register(0x3C,  0, new OnPacketReceive(OnContainerContents));
            Register(0x54, 12, new OnPacketReceive(OnSound));
            Register(0x6C, 19, new OnPacketReceive(OnTarget));
            Register(0x77, 17, new OnPacketReceive(OnMobileMoving));
            Register(0x78,  0, new OnPacketReceive(OnMobileIncoming));
            Register(0x98,  0, new OnPacketReceive(OnMobileName));
            Register(0xA1,  9, new OnPacketReceive(OnHealthUpdated));
            Register(0xA2,  9, new OnPacketReceive(OnManaUpdated));
            Register(0xA3,  9, new OnPacketReceive(OnStaminaUpdated));
            Register(0xA8,  0, new OnPacketReceive(OnServerList));
            Register(0xA9,  0, new OnPacketReceive(OnCharacterList));
            Register(0xAA,  5, new OnPacketReceive(OnAttackTarget));
            Register(0xAE,  0, new OnPacketReceive(OnUnicodeText));
            Register(0xAF, 13, new OnPacketReceive(OnMobileDeath));
            Register(0xB0,  0, new OnPacketReceive(OnGenericGump));
            Register(0xBF,  0, new OnPacketReceive(OnExtendedCommand));
            Register(0xC1,  0, new OnPacketReceive(OnLocalizedText));
            Register(0xD1,  2, new OnPacketReceive(OnLoggedOut));
            Register(0xD6,  0, new OnPacketReceive(OnProperties));
            Register(0xDD,  0, new OnPacketReceive(OnCompressedGump));
            Register(0xF3, 26, new OnPacketReceive(OnSAWorldItem));

            RegisterExtended(0x04, 0, new OnPacketReceive(OnCloseGump));
            RegisterExtended(0x06, 0, new OnPacketReceive(OnPartyCommand));
            RegisterExtended(0x08, 0, new OnPacketReceive(OnMapChanged));
            RegisterExtended(0x14, 0, new OnPacketReceive(OnContextMenu));
            RegisterExtended(0x19, 0, new OnPacketReceive(OnMiscStatus));
        }

        private static void Register(int packetId, int length, OnPacketReceive onReceive)
        {
            m_Handlers[packetId] = new PacketHandler(packetId, length, onReceive);
        }

        private static void RegisterExtended(int packetId, int length, OnPacketReceive onReceive)
        {
            m_ExtendedHandlers[packetId] = new PacketHandler(packetId, length, onReceive);
        }

        internal static PacketHandler GetHandler(int packetId)
        {
            return m_Handlers[packetId];
        }

        private static PacketHandler GetExtendedHandler(int packetId)
        {
            return m_ExtendedHandlers[packetId];
        }

        private static void OnLoggedOut(int client, PacketReader pvSrc)
        {
            IncomingPackets.OnLoggedOut(client);
        }

        private static void OnDamage(int client, PacketReader reader)
        {
            int serial = reader.ReadInt32();
            int damage = reader.ReadInt16();
            IncomingPackets.OnDamage(client, serial, damage);
        }

        private static void OnMobileStatus(int client, PacketReader reader)
        {
            int length11 = reader.Size;
            int serial = reader.ReadInt32();
            string name = reader.ReadString(30);
            int hits = reader.ReadInt16();
            int maxHits = reader.ReadInt16();
            reader.ReadByte(); // Allow Name Change
            byte features = reader.ReadByte();
            int sex = 2;
            if (length11 > 43) sex = reader.ReadByte();
            if (length11 <= 44)
            {
                IncomingPackets.OnShortStatus(client, serial, name, hits, maxHits, sex);
            }
            else
            {
                PlayerStatus ps = new PlayerStatus();
                ps.Name = name;
                ps.Health = hits;
                ps.MaxHealth = maxHits;
                ps.Sex = sex;
                ps.Features = features;
                ps.Str = reader.ReadInt16();
                ps.Dex = reader.ReadInt16();
                ps.Int = reader.ReadInt16();
                ps.Stamina = reader.ReadInt16();
                ps.MaxStamina = reader.ReadInt16();
                ps.Mana = reader.ReadInt16();
                ps.MaxMana = reader.ReadInt16();
                ps.Gold = reader.ReadInt32();
                ps.PhysicalResist = reader.ReadInt16();
                ps.Weight = reader.ReadInt16();
                switch (features)
                {
                    case 3:
                        ps.StatCap = reader.ReadInt16();
                        ps.Followers = reader.ReadByte();
                        ps.MaxFollowers = reader.ReadByte();
                        break;
                    case 4:
                        ps.StatCap = reader.ReadInt16();
                        ps.Followers = reader.ReadByte();
                        ps.MaxFollowers = reader.ReadByte();
                        ps.FireResist = reader.ReadInt16();
                        ps.ColdResist = reader.ReadInt16();
                        ps.PoisonResist = reader.ReadInt16();
                        ps.EnergyResist = reader.ReadInt16();
                        ps.Luck = reader.ReadInt16();
                        ps.MinDamage = reader.ReadInt16();
                        ps.MaxDamage = reader.ReadInt16();
                        ps.TithingPoints = reader.ReadInt32();
                        break;
                    case 5:
                        ps.MaxWeight = reader.ReadInt16();
                        ps.Race = reader.ReadByte();
                        ps.StatCap = reader.ReadInt16();
                        ps.Followers = reader.ReadByte();
                        ps.MaxFollowers = reader.ReadByte();
                        ps.FireResist = reader.ReadInt16();
                        ps.ColdResist = reader.ReadInt16();
                        ps.PoisonResist = reader.ReadInt16();
                        ps.EnergyResist = reader.ReadInt16();
                        ps.Luck = reader.ReadInt16();
                        ps.MinDamage = reader.ReadInt16();
                        ps.MaxDamage = reader.ReadInt16();
                        ps.TithingPoints = reader.ReadInt32();
                        break;
                    case 6:
                        ps.MaxWeight = reader.ReadInt16();
                        ps.Race = reader.ReadByte();
                        ps.StatCap = reader.ReadInt16();
                        ps.Followers = reader.ReadByte();
                        ps.MaxFollowers = reader.ReadByte();
                        ps.FireResist = reader.ReadInt16();
                        ps.ColdResist = reader.ReadInt16();
                        ps.PoisonResist = reader.ReadInt16();
                        ps.EnergyResist = reader.ReadInt16();
                        ps.Luck = reader.ReadInt16();
                        ps.MinDamage = reader.ReadInt16();
                        ps.MaxDamage = reader.ReadInt16();
                        ps.TithingPoints = reader.ReadInt32();
                        ps.MaxPhysicalResist = reader.ReadInt16();
                        ps.MaxFireResist = reader.ReadInt16();
                        ps.MaxColdResist = reader.ReadInt16();
                        ps.MaxPoisonResist = reader.ReadInt16();
                        ps.MaxEnergyResist = reader.ReadInt16();
                        ps.DefenseChanceIncrease = reader.ReadInt16();
                        reader.ReadInt16();
                        ps.HitChanceIncrease = reader.ReadInt16();
                        ps.SwingSpeedIncrease = reader.ReadInt16();
                        ps.DamageIncrease = reader.ReadInt16();
                        ps.LowerReagentCost = reader.ReadInt16();
                        ps.SpellDamageIncrease = reader.ReadInt16();
                        ps.FasterCastRecovery = reader.ReadInt16();
                        ps.FasterCasting = reader.ReadInt16();
                        ps.LowerManaCost = reader.ReadInt16();
                        break;
                }
                IncomingPackets.OnLongStatus(client, serial, ps);
            }
        }

        private static void OnWorldItem(int client, PacketReader reader)
        {
            //TODO: fix to use reader
            byte[] packet = reader.Data;
            Item item1a;
            uint serial1a = (uint)(packet[3] << 24 | packet[4] << 16 | packet[5] << 8 | packet[6]);
            int offset1a = 9;
            if ((serial1a & 0x80000000) != 0)
            {
                serial1a ^= 0x80000000;
                item1a = new Item((int)serial1a);
                item1a.Count = packet[offset1a] << 8 | packet[offset1a + 1];
                offset1a += 2;
            }
            else item1a = new Item((int)serial1a);
            int id1a = packet[7] << 8 | packet[8];
            if ((id1a & 0x8000) != 0)
            {
                id1a ^= 0x8000;
                id1a += packet[offset1a]; // stack id
                offset1a++;
            }
            item1a.myID = id1a;
            int x1a = packet[offset1a] << 8 | packet[offset1a + 1];
            int y1a = packet[offset1a + 2] << 8 | packet[offset1a + 3];
            offset1a += 4;
            if ((x1a & 0x8000) != 0)
            {
                x1a ^= 0x8000;
                item1a.myDirection = packet[offset1a];
                offset1a++;
            }
            item1a.myX = x1a;
            item1a.Z = (sbyte)packet[offset1a];
            offset1a++;
            if ((y1a & 0x8000) != 0)
            {
                y1a ^= 0x8000;
                item1a.myHue = packet[offset1a] << 8 | packet[offset1a + 1];
                offset1a += 2;
            }
            if ((y1a & 0x4000) != 0)
            {
                y1a ^= 0x4000;
                item1a.Flags = packet[offset1a];  // ???
            }
            item1a.myY = y1a;
            IncomingPackets.OnWorldItemAdded(client, item1a);
        }

        private static void OnInitializePlayer(int client, PacketReader reader)
        {
            int serial = reader.ReadInt32();
            PlayerMobile mobile = new PlayerMobile(serial, client);
            reader.ReadInt32(); // DWORD 0
            mobile.myID = reader.ReadInt16();
            mobile.myX = reader.ReadInt16();
            mobile.myY = reader.ReadInt16();
            mobile.myZ = reader.ReadInt16();
            mobile.myDirection = reader.ReadByte();
            IncomingPackets.OnPlayerInitialized(client, mobile);
        }

        private static void OnASCIIMessage(int client, PacketReader reader)
        {
            JournalEntry je1c = new JournalEntry();
            je1c.serial = reader.ReadInt32();
            je1c.id = reader.ReadInt16();
            je1c.speechType = (JournalSpeech)reader.ReadByte();
            je1c.speechHue = reader.ReadInt16();
            je1c.speechFont = reader.ReadInt16();
            je1c.name = reader.ReadString(30);
            je1c.text = reader.ReadString();
            IncomingPackets.OnText(client, je1c);
            General.OnJournalEntry(client, je1c);
        }

        private static void OnItemDeleted(int client, PacketReader reader)
        {
            int serial = reader.ReadInt32();
            IncomingPackets.OnItemDeleted(client, serial);
        }

        private static void OnMobileUpdated(int client, PacketReader reader)
        {
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            reader.ReadByte(); // BYTE 0x00;
            int hue = reader.ReadInt16();
            int status = reader.ReadByte();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            reader.ReadInt16(); // WORD 0x00;
            int direction = reader.ReadByte() & 0x07;
            int z = reader.ReadSByte();
            IncomingPackets.OnMobileUpdated(client, serial, id, hue, status, x, y, z, direction);
        }

        private static void OnMoveRejected(int client, PacketReader reader)
        {
            int sequence21 = reader.ReadByte();
            int x21 = reader.ReadInt16();
            int y21 = reader.ReadInt16();
            int direction21 = reader.ReadByte();
            int z21 = reader.ReadSByte();
            IncomingPackets.OnMoveRejected(client, sequence21, x21, y21, z21, direction21);
        }

        private static void OnMoveAccepted(int client, PacketReader reader)
        {
            int sequence22 = reader.ReadByte();
            int status22 = reader.ReadByte();
            IncomingPackets.OnMoveAccepted(client, sequence22, status22);
        }

        private static void OnStandardGump(int client, PacketReader reader)
        {
            int serial24 = reader.ReadInt32();
            int id24 = reader.ReadInt16();
            IncomingPackets.OnStandardGump(client, serial24, id24);
        }

        private static void OnItemAddedToContainer(int client, PacketReader reader)
        {
            const int expectedLen25a = 0x14;
            const int expectedLen25b = 0x15;
            if (reader.Size != expectedLen25a && reader.Size != expectedLen25b)
                return;
            int serial25 = reader.ReadInt32();
            int id25 = reader.ReadInt16();
            reader.ReadByte();
            int count25 = reader.ReadInt16();
            int x = reader.ReadInt16();
            int y = reader.ReadInt16();
            if (reader.Size == expectedLen25b)
                reader.ReadByte(); // BYTE Grid Location
            int containerSerial25 = reader.ReadInt32();
            int hue25 = reader.ReadInt16();
            Item item25 = new Item(serial25, containerSerial25);
            item25.ID = id25;
            item25.Count = count25;
            item25.Hue = hue25;
            item25.X = x;
            item25.Y = y;
            IncomingPackets.OnItemAddedToContainer(client, item25);
        }

        private static void OnPlayerDeath(int client, PacketReader reader)
        {
            IncomingPackets.OnPlayerDeath(client);
        }

        private static void OnItemEquipped(int client, PacketReader reader)
        {
            int serial = reader.ReadInt32();
            int id = reader.ReadInt16();
            reader.ReadByte(); // BYTE 0x00
            int layer = reader.ReadByte();
            int mobileSerial = reader.ReadInt32();
            int hue = reader.ReadInt16();

            IncomingPackets.OnItemEquipped(client, serial, id, (Layer)layer, mobileSerial, hue);
        }

        private static void OnAttackSwing(int client, PacketReader reader)
        {
            reader.ReadByte(); // BYTE 0x00;
            int attacker2f = reader.ReadInt32();
            int defender2f = reader.ReadInt32();
            IncomingPackets.OnAttackSwing(client, attacker2f, defender2f);
        }

        private static void OnAttackGranted(int client, PacketReader reader)
        {
            int serial30 = reader.ReadInt32();
            IncomingPackets.OnAttackGranted(client, serial30);
        }

        private static void OnSkillsList(int client, PacketReader reader)
        {
            byte type = reader.ReadByte();
            int id = reader.ReadInt16();
            int value = reader.ReadInt16();
            int baseValue = reader.ReadInt16();
            LockStatus lockStatus = (LockStatus)reader.ReadByte();
            int skillCap = 1000;                            //
            if (reader.Size != 11)                          // UOSteam doesn't send skillcap on changing locks in their UI
                skillCap = reader.ReadInt16();              // So we kludge it and default to 100 skillcap
            if (reader.Size == 11 || reader.Size == 13)     // 11 from UOSteam on lock change
            {
                IncomingPackets.OnSkillUpdate(client, id, (float)value / 10, (float)baseValue / 10, lockStatus, (float)skillCap / 10);
            }
            else
            {
                SkillInfo si = new SkillInfo();
                si.Value = (float)value / 10;
                si.BaseValue = (float)baseValue / 10;
                si.LockStatus = lockStatus;
                si.SkillCap = (float)skillCap / 10;
                si.ID = id - 1;

                List<SkillInfo> skillInfoList = new List<SkillInfo>(128);

                skillInfoList.Add(si);

                for (; ; )
                {
                    id = reader.ReadInt16();
                    if (id == 0) break;
                    value = reader.ReadInt16();
                    baseValue = reader.ReadInt16();
                    lockStatus = (LockStatus)reader.ReadByte();
                    skillCap = reader.ReadInt16();

                    si = new SkillInfo();
                    si.Value = (float)value / 10;
                    si.BaseValue = (float)baseValue / 10;
                    si.LockStatus = lockStatus;
                    si.SkillCap = (float)skillCap / 10;
                    si.ID = id - 1;
                    skillInfoList.Add(si);
                }

                IncomingPackets.OnSkillList(client, skillInfoList.ToArray());
            }
        }

        private static void OnContainerContents(int client, PacketReader reader)
        {
            if (reader.Size == 5)
                return;

            bool oldStyle = false;
            int count = reader.ReadInt16();

            if (((reader.Size - 5) / 20) != count)
                oldStyle = true;

            ItemCollection container = null;

            for (int i = 0; i < count; i++)
            {
                int serial = reader.ReadInt32();
                int id = reader.ReadInt16();
                reader.ReadByte(); // Item ID Offset
                int amount = reader.ReadInt16();
                int x = reader.ReadInt16();
                int y = reader.ReadInt16();
                int grid = 0;
                if (!oldStyle)
                    grid = reader.ReadByte();
                int containerSerial = reader.ReadInt32();
                int hue = reader.ReadInt16();

                if (container == null) container = new ItemCollection(client, containerSerial, count);

                Item item3c = new Item(serial, containerSerial);
                item3c.ID = id;
                item3c.Count = amount;
                item3c.Hue = hue;
                item3c.Grid = grid;
                item3c.X = x;
                item3c.Y = y;
                container.Add(item3c);
            }

            if (container != null)
                IncomingPackets.OnContainerContents(client, container);
        }

        private static void OnSound(int client, PacketReader reader)
        {
            byte flags = reader.ReadByte();
            int effect54 = reader.ReadInt16();
            int vol54 = reader.ReadInt16();
            int x54 = reader.ReadInt16();
            int y54 = reader.ReadInt16();
            int z54 = reader.ReadInt16();
            IncomingPackets.OnSound(client, flags, effect54, vol54, x54, y54, z54);
        }

        private static void OnTarget(int client, PacketReader reader)
        {
            byte type = reader.ReadByte();
            IncomingPackets.OnTarget(client, type);
        }

        private static void OnMobileMoving(int client, PacketReader reader)
        {
            int serial77 = reader.ReadInt32();
            int id77 = reader.ReadInt16();
            int x77 = reader.ReadInt16();
            int y77 = reader.ReadInt16();
            int z77 = reader.ReadSByte();
            int direction77 = reader.ReadByte() & 0x07;
            int hue77 = reader.ReadInt16();
            int status77 = reader.ReadByte();
            int noto77 = reader.ReadByte();
            IncomingPackets.OnMobileMoving(client, serial77, id77, x77, y77, z77, direction77, hue77, status77, noto77);
        }

        private static void OnMobileIncoming(int client, PacketReader reader)
        {
            int serial78 = reader.ReadInt32();
            ItemCollection container78 = new ItemCollection(serial78, 125);
            Mobile mob78 = new Mobile(serial78, client);
            mob78.myID = reader.ReadInt16();
            mob78.myX = reader.ReadInt16();
            mob78.myY = reader.ReadInt16();
            mob78.myZ = reader.ReadSByte();
            mob78.myDirection = reader.ReadByte() & 0x07;
            mob78.myHue = reader.ReadInt16();
            mob78.myStatus = reader.ReadByte();
            mob78.myNotoriety = reader.ReadByte();
            Item item;
            for (; ; )
            {
                int itemSerial = reader.ReadInt32();
                if (itemSerial == 0)
                    break;
                item = new Item(itemSerial);
                item.Owner = serial78;
                item.ID = reader.ReadInt16();
                item.Layer = (Layer)reader.ReadByte();
                ClientInfo ci;
                ClientInfoCollection.GetClient(client, out ci);
                if (ci.UseNewMobileIncoming)
                {
                    item.myHue = reader.ReadInt16();
                }
                else
                {
                    if ((item.myID & 0x8000) != 0)
                    {
                        item.myID ^= 0x8000;
                        item.myHue = reader.ReadInt16();
                    }
                }
                container78.Add( item );
            }
            IncomingPackets.OnEquippedMobAdded(client, mob78, container78);
        }

        private static void OnMobileName(int client, PacketReader reader)
        {
            int serial98 = reader.ReadInt32();
            string name98 = reader.ReadString();
            IncomingPackets.OnMobileName(client, serial98, name98);
        }

        private static void OnHealthUpdated(int client, PacketReader reader)
        {
            int seriala1 = reader.ReadInt32();
            int maxHealth = reader.ReadInt16();
            int health = reader.ReadInt16();
            IncomingPackets.OnHealthUpdated(client, seriala1, maxHealth, health);
        }

        private static void OnManaUpdated(int client, PacketReader reader)
        {
            int seriala2 = reader.ReadInt32();
            int maxMana = reader.ReadInt16();
            int mana = reader.ReadInt16();
            IncomingPackets.OnManaUpdated(client, seriala2, maxMana, mana);
        }

        private static void OnStaminaUpdated(int client, PacketReader reader)
        {
            int seriala3 = reader.ReadInt32();
            int maxStamina = reader.ReadInt16();
            int stamina = reader.ReadInt16();
            IncomingPackets.OnStaminaUpdated(client, seriala3, maxStamina, stamina);
        }

        private static void OnServerList(int client, PacketReader reader)
        {
            reader.ReadByte(); // BYTE 0x00
            int count = reader.ReadInt16();
            ServerInfo[] si = new ServerInfo[count];

            for (int i = 0; i < count; i++)
            {
                reader.ReadInt16(); // index
                si[i] = new ServerInfo();
                si[i].Name = reader.ReadString(32);
                si[i].PercentFull = reader.ReadByte();
                si[i].Timezone = reader.ReadByte();
                si[i].IP = reader.ReadUInt32();
            }

            IncomingPackets.OnServerList(client, si);
        }

        private static void OnCharacterList(int client, PacketReader reader)
        {
            byte count = reader.ReadByte();
            string[] charsa9 = new string[count];
            for (int i = 0; i < count; i++)
            {
                charsa9[i] = reader.ReadString(30);
                reader.ReadString(30); // Password
            }
            IncomingPackets.OnCharacterList(client, charsa9);
        }

        private static void OnAttackTarget(int client, PacketReader reader)
        {
            int serialaa = reader.ReadInt32();
            IncomingPackets.OnAttackTarget(client, serialaa);
        }

        private static void OnUnicodeText(int client, PacketReader reader)
        {
            JournalEntry jeae = new JournalEntry();
            jeae.serial = reader.ReadInt32();
            jeae.id = reader.ReadInt16();
            jeae.speechType = (JournalSpeech)reader.ReadByte();
            jeae.speechHue = reader.ReadInt16();
            jeae.speechFont = reader.ReadInt16();
            jeae.speechLanguage = reader.ReadString(4);
            jeae.name = reader.ReadString(30);
            jeae.text = reader.ReadUnicodeString();
            IncomingPackets.OnUnicodeText(client, jeae);
            General.OnJournalEntry(client, jeae);
        }

        private static void OnMobileDeath(int client, PacketReader reader)
        {
            int serialaf = reader.ReadInt32();
            int corpseaf = reader.ReadInt32();
            IncomingPackets.OnMobileDeath(client, serialaf, corpseaf);
        }

        private static void OnGenericGump(int client, PacketReader reader)
        {
            int serialb0 = reader.ReadInt32();
            int idb0 = reader.ReadInt32();
            int xb0 = reader.ReadInt32();
            int yb0 = reader.ReadInt32();
            int layoutLenb0 = reader.ReadInt16();
            string layoutb0 = reader.ReadString(layoutLenb0);
            int linesb0 = reader.ReadInt16();
            string[] textb0 = new string[linesb0];
            int textLenb0;
            for (int x = 0; x < linesb0; x++)
            {
                textLenb0 = reader.ReadInt16() * 2;
                textb0[x] = reader.ReadUnicodeString(textLenb0);
            }
            IncomingPackets.OnGenericGump(client, serialb0, idb0, xb0, yb0, layoutb0, textb0);
        }

        private static void OnExtendedCommand(int client, PacketReader reader)
        {
            int command = reader.ReadInt16();

            PacketHandler handler = GetExtendedHandler(command);
            if (handler != null)
                handler.OnReceive(client, reader);
        }

        private static void OnLocalizedText(int client, PacketReader reader)
        {
            JournalEntry jec1 = new JournalEntry();
            jec1.serial = reader.ReadInt32();
            jec1.id = reader.ReadInt16();
            jec1.speechType = (JournalSpeech)reader.ReadByte();
            if (jec1.speechType == JournalSpeech.Yell)
            {
                Log.LogDataMessage(client, reader.Data, "Incoming encoded C1 packet:\r\n");
                return;
            }
            jec1.speechHue = reader.ReadInt16();
            jec1.speechFont = reader.ReadInt16();
            int messagec1 = reader.ReadInt32();
            jec1.name = reader.ReadString(30);
            //TODO: Fix the below two lines to use reader.ReadUnicodeString();
            string[] argumentsc1 = UnicodeEncoding.Unicode.GetString(reader.Data, 48, reader.Size - 50).Split('\t');
            jec1.text = Cliloc.GetLocalString(messagec1, argumentsc1);
            IncomingPackets.OnLocalizedText(client, jec1);
            General.OnJournalEntry(client, jec1);
        }

        private static void OnProperties(int client, PacketReader reader)
        {
            reader.ReadInt16(); // WORD 0x01
            int serial = reader.ReadInt32();
            reader.ReadInt16(); // WORD 0x00
            int hash = reader.ReadInt32();
            Property p;
            StringBuilder propertyText = new StringBuilder();
            List<Property> propertyList = new List<Property>();
            string named6 = "";
            bool nameSet = false, first = true;
            int lastCliloc = -1;

            for (; ; )
            {
                p = new Property();
                p.Cliloc = reader.ReadInt32();
                if (p.Cliloc == 0)
                    break;
                if (!first) propertyText.Append("\r\n");
                int len = reader.ReadInt16();
                if (len > 0)
                {
                    //TODO: Fix the below two lines to use reader.ReadUnicodeString();
                    p.Arguments = UnicodeEncoding.Unicode.GetString(reader.Data, reader.Index, len).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    reader.Seek(len, SeekOrigin.Current);

                    p.Text = Cliloc.GetLocalString(p.Cliloc, p.Arguments);
                    if (!nameSet)
                    {
                        named6 = p.Text;
                        nameSet = true;
                    }
                }
                else
                {
                    p.Text = Cliloc.GetProperty(p.Cliloc);
                    if (!nameSet)
                    {
                        named6 = p.Text;
                        nameSet = true;
                    }
                }

                if (lastCliloc != -1)
                {
                    if (lastCliloc != p.Cliloc)
                    {
                        propertyList.Add(p);
                        propertyText.Append(p.Text);
                    }
                }
                else
                {
                    propertyList.Add(p);
                    propertyText.Append(p.Text);
                }
                lastCliloc = p.Cliloc;
                first = false;
            }

            IncomingPackets.OnProperties(client, serial, named6, propertyList.ToArray(), propertyText.ToString());
        }

        private static void OnCompressedGump(int client, PacketReader reader)
        {
            int serialdd = reader.ReadInt32();
            int iddd = reader.ReadInt32();
            int xdd = reader.ReadInt32();
            int ydd = reader.ReadInt32();
            int compressLendd = reader.ReadInt32();
            if (compressLendd <= 4) return;
            else compressLendd -= 4;
            int decompressLendd = reader.ReadInt32() + 1;
            byte[] decompresseddd = new byte[decompressLendd];
            byte[] compresseddd = new byte[compressLendd];
            Buffer.BlockCopy(reader.Data, reader.Index, compresseddd, 0, compressLendd);
            reader.Seek(compressLendd, SeekOrigin.Current);
            int success;
            //if (IntPtr.Size == 8) success = NativeMethods.uncompress64(decompresseddd, ref decompressLendd, compresseddd, compressLendd);
            success = NativeMethods.uncompress32(decompresseddd, ref decompressLendd, compresseddd, compressLendd);
            if (success != 0)
            {
                Log.LogDataMessage(client, reader.Data, "*** Error decompressing gump layout:");
                return;
            }

            string layoutdd = ASCIIEncoding.ASCII.GetString(decompresseddd).TrimEnd('\0');
            int offsetdd = 27 + compressLendd;
            int linesdd = reader.ReadInt32();
            compressLendd = reader.ReadInt32();
            string[] textdd = new string[linesdd];
            if (compressLendd > 4)
            {
                compressLendd -= 4;
                compresseddd = new byte[compressLendd];
                decompressLendd = reader.ReadInt32() + 1;
                decompresseddd = new byte[decompressLendd];
                Buffer.BlockCopy(reader.Data, reader.Index, compresseddd, 0, compressLendd);
                reader.Seek(compressLendd, SeekOrigin.Current);
                //if (IntPtr.Size == 8) success = NativeMethods.uncompress64(decompresseddd, ref decompressLendd, compresseddd, compressLendd);
                success = NativeMethods.uncompress32(decompresseddd, ref decompressLendd, compresseddd, compressLendd);
                if (success != 0)
                {
                    Log.LogDataMessage(client, reader.Data, "*** Error decompressing gump strings:");
                    return;
                }
                offsetdd = 0;
                int lendd = 0;
                for (int x = 0; x < linesdd; x++)
                {
                    lendd = (decompresseddd[offsetdd] << 8 | decompresseddd[offsetdd + 1]) * 2;
                    offsetdd += 2;
                    textdd[x] = UnicodeEncoding.BigEndianUnicode.GetString(decompresseddd, offsetdd, lendd);
                    offsetdd += lendd;
                }
            }
            IncomingPackets.OnGenericGump(client, serialdd, iddd, xdd, ydd, layoutdd, textdd);
        }

        private static void OnSAWorldItem(int client, PacketReader reader)
        {
            reader.ReadInt16(); // WORD 0x01
            byte type = reader.ReadByte(); // Data Type (0x00 = use TileData, 0x01 = use BodyData, 0x02 = use MultiData)
            int serialf3 = reader.ReadInt32();
            Item itemf3 = new Item(serialf3);
            itemf3.ArtDataID = type;
            itemf3.ID = reader.ReadInt16();
            itemf3.Direction = reader.ReadByte();
            itemf3.Count = reader.ReadInt16();
            reader.ReadInt16(); // Second Amount?
            itemf3.X = reader.ReadInt16();
            itemf3.Y = reader.ReadInt16();
            itemf3.Z = reader.ReadSByte();
            itemf3.Light = reader.ReadByte();
            itemf3.Hue = reader.ReadInt16();
            itemf3.Flags = reader.ReadByte();
            IncomingPackets.OnWorldItemAdded(client, itemf3);
        }

        private static void OnCloseGump(int client, PacketReader reader)
        {
            int gumpID = reader.ReadInt32();
            int buttonID = reader.ReadInt32();
            IncomingPackets.OnCloseGump(client, gumpID, buttonID);
        }

        private static void OnPartyCommand(int client, PacketReader reader)
        {
            int subcommand = reader.ReadByte();

            switch (subcommand)
            {
                case 4: // Party Chat
                    JournalEntry jebf = new JournalEntry();
                    jebf.serial = reader.ReadInt32();
                    jebf.text = reader.ReadUnicodeString();
                    IncomingPackets.OnPartyText(client, jebf);
                    General.OnJournalEntry(client, jebf);
                    break;
            }
        }

        private static void OnMapChanged(int client, PacketReader reader)
        {
            byte map = reader.ReadByte();
            IncomingPackets.OnMapChanged(client, map);
        }

        private static void OnContextMenu(int client, PacketReader reader)
        {
            int type = reader.ReadInt16();
            int serial = reader.ReadInt32();
            int len = reader.ReadByte();

            ContextEntry[] ce = new ContextEntry[len];
            int entry, cliloc, flags, hue;

            switch (type)
            {
                case 1: // Old Type
                    for (int x = 0; x < len; x++)
                    {
                        entry = reader.ReadInt16();
                        cliloc = reader.ReadInt16() + 3000000;
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ((flags & 0x20) == 0x20)
                            hue = reader.ReadInt16();

                        string text = Cliloc.GetProperty(cliloc);
                        ce[x] = new ContextEntry(client, entry, serial, text, flags, hue);
                    }
                    IncomingPackets.OnContextMenu(client, ce);
                    break;
                case 2: // KR -> SA3D -> 2D post 7.0.0.0
                    for (int x = 0; x < len; x++)
                    {
                        cliloc = reader.ReadInt32();
                        entry = reader.ReadInt16();
                        flags = reader.ReadInt16();
                        hue = 0;

                        if ((flags & 0x20) == 0x20)
                            hue = reader.ReadInt16();

                        string text = Cliloc.GetProperty(cliloc);
                        ce[x] = new ContextEntry(client, entry, serial, text, flags, hue);
                    }
                    IncomingPackets.OnContextMenu(client, ce);
                    break;
            }
        }

        private static void OnMiscStatus(int client, PacketReader reader)
        {
            int subcommand = reader.ReadByte();

            switch (subcommand)
            {
                case 0: // Bonded status (old)
                    {
                        int serial = reader.ReadInt32();
                        int dead = reader.ReadByte();
                        IncomingPackets.OnBondedStatus(client, serial, dead == 1);
                    }
                    break;
                case 2: // Stat lock info
                    {
                        int serial = reader.ReadInt32();
                        reader.ReadByte(); // BYTE 0x00
                        int lockFlags = reader.ReadByte();
                        IncomingPackets.OnStatLockStatus(client, serial, lockFlags);
                    }
                    break;
                case 5: //subcommand 5, KR / SA stat lock status, bonded status, mobile status
                    {
                        int serial = reader.ReadInt32();
                        int dead = reader.ReadByte();
                        int flags = reader.ReadByte();
                        if (flags == 0xFF)
                            IncomingPackets.OnBondedStatus(client, serial, dead == 1);
                        else
                            IncomingPackets.OnStatLockStatus(client, serial, flags);
                    }
                    break;
            }
        }
    }
}
