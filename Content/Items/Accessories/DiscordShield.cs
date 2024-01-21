using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria.ModLoader.IO;

namespace GivlsWeapons.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    internal class DiscordShield : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.damage = 80;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.crit = 4;
            Item.value = 150000;
            Item.expert = true;
            Item.defense = 4;
            Item.knockBack = 9f;

            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DiscordDash>().DashAccessoryEquipped = true;
        }

        public override bool MeleePrefix()
        {
            return false;
        }
    }

    internal class DiscordDash : ModPlayer
    { //Finally fixed all MP bugs with this thing, also increased the stats a little bit and fixed the damage for real
        //public const int DashDown = 0;
        //public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;
        public const int DashCooldown = 45; // Time (frames) between starting dashes. If this is shorter than DashDuration you can start a new dash before an old one has finished
        public const int DashDuration = 15; // Duration of the dash afterimage effect in frames

        public const float DashVelocity = 10.5f;

        public int DashDir = -1;

        public bool DashAccessoryEquipped;
        //public int DashDelay = 0; // frames remaining till we can dash again
        //public int DashTimer = 0; // frames remaining in the dash
        //public int DiscordHit = -1; //Entity.whoAmI of the enemy hit

        public override void ResetEffects()
        {
            DashAccessoryEquipped = false;
            // ResetEffects is called not long after player.doubleTapCardinalTimer's values have been set
            // When a directional key is pressed and released, vanilla starts a 15 tick (1/4 second) timer during which a second press activates a dash
            // If the timers are set to 15, then this is the first press just processed by the vanilla logic.  Otherwise, it's a double-tap
            if (Main.myPlayer == Player.whoAmI)
            {
                if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
                {
                    DashDir = DashRight;
                }
                else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
                {
                    DashDir = DashLeft;
                }
                else
                {
                    DashDir = -1;
                }
                /* if (DashTimer < 1)
                    DiscordHit = -1; */ //Unnecessary if using eocDash, as it is already set to -1 the same way
            }
        }
        public override void PreUpdateMovement()
        {
            // if the player can use our dash, has double tapped in a direction, and our dash isn't currently on cooldown
            if (CanUseDash() && DashDir != -1 && Player.dashDelay == 0)
            {
                Vector2 newVelocity = Player.velocity;

                switch (DashDir)
                {
                    case DashLeft when Player.velocity.X > -DashVelocity:
                    case DashRight when Player.velocity.X < DashVelocity:
                        {
                            // X-velocity is set here
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        }
                    default:
                        return; // not moving fast enough, so don't start our dash
                }

                // start our dash
                Player.dashDelay = DashCooldown;
                Player.eocDash = DashDuration;
                Player.dash = -1; //Set to -1 to prevent conflict with vanilla logic.
                Player.velocity = newVelocity;

                // Here you'd be able to set an effect that happens when the dash first activates
                // Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
            }
            /* if (DashTimer > 0)
                DashTimer--; */
            if (Player.eocDash > 0 && CanUseDash())
            { // dash is active
              // This is where we set the afterimage effect.  You can replace these two lines with whatever you want to happen during the dash
              // Some examples include:  spawning dust where the player is, adding buffs, making the player immune, etc.
              // Here we take advantage of "player.eocDash" and "player.armorEffectDrawShadowEOCShield" to get the Shield of Cthulhu's afterimage effect
                Player.armorEffectDrawShadowEOCShield = true;

                if (Player.eocHit < 0 && Main.myPlayer == Player.whoAmI)
                {
                    Rectangle dashHitbox = new((int)(Player.position.X + Player.velocity.X * 0.5 - 6.0), (int)(Player.position.Y + Player.velocity.Y * 0.5 - 6.0), Player.width + 12, Player.height + 12);
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        NPC target = Main.npc[i];
                        Rectangle enemyHitBox = target.getRect();
                        if (!target.active || target.dontTakeDamage || target.friendly || (target.aiStyle == 112 && !(target.ai[2] <= 1f)))
                        {
                            continue;
                        }
                        if (dashHitbox.Intersects(enemyHitBox))
                        {
                            float dashDamage = Player.GetDamage(DamageClass.Melee).ApplyTo(80);
                            float dashKb = Player.GetKnockback(DamageClass.Melee).ApplyTo(9f);
                            bool dashCrit = false;
                            int kbDirection = 0;
                            if (Player.velocity.X < 0f)
                            {
                                kbDirection = -1;
                            }
                            if (Player.velocity.X > 0f)
                            {
                                kbDirection = 1;
                            }
                            if (Player.HasBuff(BuffID.ChaosState) || target.HasBuff(BuffID.ChaosState))
                            {
                                dashDamage *= 2.5f;
                            }
                            target.AddBuff(BuffID.ChaosState, 480);
                            if (Main.rand.Next(100) < Player.GetCritChance<MeleeDamageClass>() + 4)
                            {
                                dashCrit = true;
                                dashDamage *= 2f;
                            }

                            NPC.HitInfo dashAttack = new()
                            {
                                Damage = (int)dashDamage - target.defense,
                                Knockback = dashKb * target.knockBackResist,
                                HitDirection = kbDirection,
                                Crit = dashCrit
                            };
                            target.StrikeNPC(dashAttack);
                            NetMessage.SendStrikeNPC(target, dashAttack);
                            /*                             DashDelay = DashCooldown;
                            DashDir = -1;
                            DashTimer = 1; */
                            Player.eocHit = i; //Set eocHit, granting immunity against the enemy and disabling the dash's ability to hit until a new one starts
                            /* Player.eocDash = 1; */
                            Player.immune = true;
                            Player.immuneNoBlink = true;
                            Player.GiveImmuneTimeForCollisionAttack(20);
                            Vector2 mousePos = Main.MouseWorld;
                            Player.LimitPointToPlayerReachableArea(ref mousePos);
                            if (!Collision.SolidCollision(mousePos, Player.Hitbox.Width, Player.Hitbox.Height))
                            {
                                Player.Teleport(mousePos, TeleportationStyleID.RodOfDiscord);
                                NetMessage.SendData(65, -1, -1, null, 0, Player.whoAmI, mousePos.X, mousePos.Y, 1);
                            }
                        }
                    }
                }

                // count down frames remaining
                //DashTimer--; //unnecessary if using eocDash, as it is already decreased on every tick
                //Player.eocDash = DashTimer;
            }
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)GivlsWeapons.MessageType.DiscordDashStart);
            packet.Write((byte)Player.whoAmI);
            packet.Write((byte)DashDir);
            packet.Send(toWho, fromWho);
        }

        // Called in ExampleMod.Networking.cs
        public void ReceivePlayerSync(BinaryReader reader)
        {
            DashDir = reader.ReadByte();
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            DiscordDash clone = (DiscordDash)targetCopy;
            clone.DashDir = DashDir;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            DiscordDash clone = (DiscordDash)clientPlayer;

            if (DashDir != clone.DashDir)
                SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        }
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            return Player.eocDash <= 0 || !DashAccessoryEquipped; //Returns false if you have the accessory and are dashing
        }

        private bool CanUseDash()
        {
            return DashAccessoryEquipped
                            && Player.dashType == 0 // player doesn't have Tabi or EoCShield equipped (give priority to those dashes)
                            && !Player.setSolar // player isn't wearing solar armor
                            && !Player.mount.Active; // player isn't mounted, since dashes on a mount look weird
        }
    }
}