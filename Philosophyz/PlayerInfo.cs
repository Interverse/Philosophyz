﻿using System;
using Terraria;
using TShockAPI;

namespace Philosophyz
{
	public class PlayerInfo
	{
		public const string InfoKey = "pz-info";

		private readonly TSPlayer _player;

		private bool? _fakeSscStatus;

		public bool? FakeSscStatus
		{
			get { return _fakeSscStatus; }
			internal set
			{
				if (value == _fakeSscStatus)
					return;
				Philosophyz.SendInfo(_player, value ?? Main.ServerSideCharacter);
				_fakeSscStatus = value;
			}
		}

		public bool InSscRegion { get; internal set; }

		public bool BypassChange { get; internal set; }

		public PlayerData OriginData { get; internal set; }

		private PlayerInfo(TSPlayer player)
		{
			_player = player;

			FakeSscStatus = false;
		}

		public void SetBackupPlayerData()
		{
			var data = new PlayerData(_player);
			data.CopyCharacter(_player);

			OriginData = data;
		}

		public void ChangeSlot(int slot, int netId, byte prefix, int stack)
		{
			if (slot > Main.maxInventory)
			{
				return;
			}

			if (!InSscRegion) // 服务器现状为伪装无ssc并在区域内有ssc
			{
				return;
			}

			_player.TPlayer.inventory[slot].netDefaults(netId);

			if (_player.TPlayer.inventory[slot].netID != 0)
			{
				_player.TPlayer.inventory[slot].stack = stack;
				_player.TPlayer.inventory[slot].prefix = prefix;
			}
			
			NetMessage.SendData(5, -1, -1, Main.player[_player.Index].inventory[slot].name, _player.Index, slot, Main.player[_player.Index].inventory[slot].prefix);
			NetMessage.SendData(5, _player.Index, -1, Main.player[_player.Index].inventory[slot].name, _player.Index, slot, Main.player[_player.Index].inventory[slot].prefix);
		}

		public void ChangeInventory(NetItem[] items)
		{
			if (!InSscRegion) // 服务器现状为伪装无ssc并在区域内有ssc
			{
				return;
			}

			var max = Math.Min(items.Length, Main.maxInventory) + 1;

			for (var i = 0; i < max; i++)
			{
				_player.TPlayer.inventory[i].netDefaults(items[i].NetId);

				if (_player.TPlayer.inventory[i].netID != 0)
				{
					_player.TPlayer.inventory[i].stack = items[i].Stack;
					_player.TPlayer.inventory[i].prefix = items[i].PrefixId;
				}
			}

			for (var k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, -1, -1, Main.player[_player.Index].inventory[k].name, _player.Index, k, Main.player[_player.Index].inventory[k].prefix);
			}

			for (var k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, _player.Index, -1, Main.player[_player.Index].inventory[k].name, _player.Index, k, Main.player[_player.Index].inventory[k].prefix);
			}
		}

		public void ChangeCharacter(PlayerData data)
		{
			data.RestoreCharacter(_player);
		}

		public void RestoreCharacter()
		{
			ChangeCharacter(OriginData);
		}

		public static PlayerInfo GetPlayerInfo(TSPlayer player)
		{
			var info = player.GetData<PlayerInfo>(InfoKey);
			if (info == null)
			{
				info = new PlayerInfo(player);
				player.SetData(InfoKey, info);
			}
			return info;
		}
	}
}