﻿using MarioLand.Common.Players;
using System.IO;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MarioLand.Common.CustomLoadout;
internal static class LoadoutSyncing
{
    internal const byte SyncLoadoutId = 0;

    internal static void SyncCustomLoadout(Player player, int customLoadoutIndex, int toPlayer, int fromPlayer)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        ModPacket packet = ModContent.GetInstance<MarioLand>().GetPacket();
        packet.Write(SyncLoadoutId);
        packet.Write((byte)player.whoAmI);
        packet.Write((byte)customLoadoutIndex);

        EquipmentLoadout customLoadout = player.GetModPlayer<LoadoutPlayer>().CustomLoadouts[customLoadoutIndex];
        SendLoadoutItemArray(packet, customLoadout.Armor);
        SendLoadoutItemArray(packet, customLoadout.Dye);

        for (int i = 0; i < customLoadout.Hide.Length; i++)
        {
            packet.Write(customLoadout.Hide[i]);
        }

        packet.Send(toPlayer, fromPlayer);
    }

    private static void SendLoadoutItemArray(ModPacket packet, Item[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            Item item = arr[i];
            SendLoadoutItem(packet, item, i);
        }
    }

    private static void SendLoadoutItem(ModPacket packet, Item item, int index)
    {
        if (item.Name == "" || item.stack == 0 || item.type == ItemID.None)
        {
            item.SetDefaults(0, true);
        }

        int stack = Math.Max(item.stack, 0);
        int netId = item.netID;

        packet.Write((byte)index);
        packet.Write((short)stack);
        packet.Write(netId);
        packet.Write((short)item.prefix);
    }

    private static void ReadLoadoutItem(BinaryReader reader, Item[] intoArr)
    {
        byte index = reader.ReadByte();
        short stack = reader.ReadInt16();
        int netId = reader.ReadInt32();
        short prefix = reader.ReadInt16();

        intoArr[index].SetDefaults(netId);
        intoArr[index].stack = stack;
        intoArr[index].Prefix(prefix);
    }

    private static void ReadLoadoutItemArray(BinaryReader reader, Item[] intoArr)
    {
        for (int i = 0; i < intoArr.Length; i++)
        {
            ReadLoadoutItem(reader, intoArr);
        }
    }

    internal static void HandlePacket(BinaryReader reader)
    {
        byte id = reader.ReadByte();
        switch (id)
        {
            case SyncLoadoutId:
                byte whoAmI = reader.ReadByte();
                byte customLoadoutIndex = reader.ReadByte();

                EquipmentLoadout customLoadout = Main.player[whoAmI].GetModPlayer<LoadoutPlayer>().CustomLoadouts[customLoadoutIndex];
                ReadLoadoutItemArray(reader, customLoadout.Armor);
                ReadLoadoutItemArray(reader, customLoadout.Dye);

                for (int i = 0; i < customLoadout.Hide.Length; i++)
                {
                    customLoadout.Hide[i] = reader.ReadBoolean();
                }

                break;
        }
    }
}