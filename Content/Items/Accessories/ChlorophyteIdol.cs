using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Projectiles.Accessories;
using Terraria.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GivlsWeapons.Content.Items.Accessories
{
    internal class ChlorophyteIdol : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 38;
            Item.value = Item.sellPrice(gold: 7, silver: 50);
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 80;
            Item.knockBack = 8f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) //Since the leaf crystals can't crit, we don't display a crit chance
        {
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "CritChance");

            if (line != null)
            {
                //Being safe in case some other mod messes with crit tooltips
                tooltips.Remove(line);
            }
        }

        public override bool MeleePrefix()
        {
            return false;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ChlorophyteIdolPlayer>().AccessoryEquipped = true;
            player.GetModPlayer<ChlorophyteIdolPlayer>().Accessory = Item;
        }
    }

    internal class ChlorophyteIdolPlayer : ModPlayer
    {
        public bool AccessoryEquipped = false;
        public Item Accessory;
        private int CooldownTimer = DURATION;
        const int DURATION = 54; //The time in ticks it takes for a crystal to be able to be fired. I decided to experiment with putting non-round numbers in things to see how it felt
        public override void PreUpdate()
        {
            if (Main.myPlayer == Player.whoAmI && AccessoryEquipped && CooldownTimer < 2 * DURATION)
            {
                CooldownTimer++;
            }
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if ((hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed) && target.type != NPCID.TargetDummy)
            {
                TrySpawningCrystal(target);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        { //Check for melee, then true melee by checking if heldProj is set, then distance to account for new excalibur-types, then for harpoon and flairon ai. Distance is wider than needed to account for other modders' potential bad code.
            if ((hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed) && (Player.heldProj == proj.whoAmI || proj.Distance(Player.Center) <= 8f || proj.aiStyle == ProjAIStyleID.Harpoon || proj.aiStyle == ProjAIStyleID.Flairon) && target.type != NPCID.TargetDummy)
            { //True melee projectiles that don't set Player.heldProj, and don't anchor themselves to the player's center or use the Flail or Flairon ai style won't work. Projectile melee weapons at extremely close range will, but that hardly matters.
                TrySpawningCrystal(target);
            }
        }
        public override void ResetEffects()
        {
            AccessoryEquipped = false;
            Accessory = null;
        }
        void TrySpawningCrystal(NPC target)
        {
            if (Main.myPlayer == Player.whoAmI && AccessoryEquipped)
            {
                if (CooldownTimer >= DURATION)
                {
                    //Vector2 spawnPos = (Player.Center + target.Center) * 0.5f;
                    Vector2 spawnVel = target.Center.AngleTo(Player.Center).ToRotationVector2() * target.Center.Distance(Player.Center) * 0.064f;
                    Projectile.NewProjectile(Player.GetSource_Accessory(Accessory), target.Center, spawnVel, ModContent.ProjectileType<AltLeafCrystal>(), 100, 10f, Player.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item8);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustPerfect(target.Center, DustID.ChlorophyteWeapon, Main.rand.NextFloat(0, MathF.Tau).ToRotationVector2(), Scale: 0.7f);
                    }
                    if (CooldownTimer >= DURATION * 2)
                    {
                        CooldownTimer = 0;
                    }
                    else
                    {
                        CooldownTimer -= DURATION;
                    }
                }
            }
        }
    }
}