using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace GivlsWeapons.Common.Configs
{
	public class BuffConfig : ModConfig //This config is for people with other mods that give these buffs effects to disable this mod's effects to prevent them from overlapping
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("Debuffs")]
		[DefaultValue(true)]
		public bool ChaosStateAffectsNPCs;
		[DefaultValue(true)]
		public bool ShadowflameAffectsPlayers;
	}
}