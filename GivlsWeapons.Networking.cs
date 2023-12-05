using System.IO;
using GivlsWeapons.Content.Items.Accessories;
using Terraria;
using Terraria.ID;

namespace GivlsWeapons
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	partial class GivlsWeapons
	{
		internal enum MessageType : byte
		{
			DiscordDashStart
		}
        //OOP packets coming someday apparently, looking forward to that
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				// This message syncs ExampleStatIncreasePlayer.exampleLifeFruits and ExampleStatIncreasePlayer.exampleManaCrystals
				case MessageType.DiscordDashStart:
					byte playernumber = reader.ReadByte();
					DiscordDash playerDash = Main.player[playernumber].GetModPlayer<DiscordDash>();
					playerDash.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the other clients
						playerDash.SyncPlayer(-1, whoAmI, false);
					}
					break;
				default:
					Logger.WarnFormat("GivlsWeapons: Unknown Message type: {DiscordDashStart}", msgType);
					break;
			}
		}
	}
}