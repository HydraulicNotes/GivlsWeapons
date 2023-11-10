using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ID;
using GivlsWeapons.Content.Projectiles.Weapons;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System.Linq;
using GivlsWeapons.Content.Globals;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class TeleportingAxes : ModItem
    {
        public int attackType = 0;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 30;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Pink;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 72;
            Item.crit = 8;
            Item.knockBack = 3f;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ModContent.ProjectileType<TeleportingAxeSwingProjectile>();
            Item.shootSpeed = 7f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int axeType = player.altFunctionUse == 2 ? 1 : 0; //player.altFunctionUse is either 0 or 2 because redigit is just on another level man. This fixes that.
            if ((player.GetModPlayer<TeleportingAxePlayer>().blueAxeReady && axeType == 0) || (player.GetModPlayer<TeleportingAxePlayer>().pinkAxeReady && axeType == 1))
            {
                bool axeOfTypeNotFound = true;

                foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == player.whoAmI && x.type == ModContent.ProjectileType<TeleportingAxeProjectile>() && x.ai[0] == axeType))
                { //look for an axe of the correct type that's owned by the player. If one is found then teleport, kill it, and don't spawn the projectile
                    player.Teleport(new Vector2(p.Top.X, p.Top.Y - player.height * 0.5f), TeleportationStyleID.RodOfDiscord);
                    player.AddImmuneTime(ImmunityCooldownID.General, 8); //Prevents unexpected hits right after teleporting
                    p.Kill();
                    axeOfTypeNotFound = false;
                    if (axeType == 0) //finally, reset the player's teleport for that axe
                    {
                        player.GetModPlayer<TeleportingAxePlayer>().blueAxeReady = false;
                    }
                    else
                    {
                        player.GetModPlayer<TeleportingAxePlayer>().pinkAxeReady = false;
                    }
                }
                if (axeOfTypeNotFound && player.GetModPlayer<ReuseTimer>().Timer < 10) //checks for continuous uses so holding the mouse button won't trigger a throw
                { //Throw an axe of the respective type. Spawns slightly higher so it doesn't hit the ground instantly.
                    Projectile.NewProjectile(source, new Vector2(position.X, position.Y - 15f), velocity, ModContent.ProjectileType<TeleportingAxeProjectile>(), damage, knockback, player.whoAmI, axeType);
                }
                else
                {
                    //altFunctionUse is added to attackType, so that the projectile's attackType will be moved into the range for the correct axe, and still indicate the correct combo strike
                    int axeSwing = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, attackType + player.altFunctionUse);
                    Main.projectile[axeSwing].localAI[2] = 2; //The rune glow is set from the beginning. This may change if I feel like it would look better.
                    attackType = (attackType + 1) % 2;
                }
            }
            else
            {
                //
                Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, attackType + player.altFunctionUse);
                attackType = (attackType + 1) % 2;
            }



            return false;
        }

        /* public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int axeType = player.altFunctionUse == 2 ? 1 : 0; 
            bool axeOfTypeNotFound = true;

            foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == player.whoAmI && x.type == ModContent.ProjectileType<TeleportingAxeProjectile>() && x.ai[0] == axeType))
            { //look for an axe of the correct type that's owned by the player. If one is found, teleport, kill it, and don't spawn the projectile
                player.Teleport(p.Top, TeleportationStyleID.RodOfDiscord);
                p.Kill();
                axeOfTypeNotFound = false;
            }
            if (axeOfTypeNotFound)
            { //Identical to what would happen if we returned true, except we set the type of axe through ai0
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, axeType);
            }
            return false;
        } */

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MythrilWaraxe)
                .AddIngredient(ItemID.OrichalcumWaraxe)
                .AddIngredient(ItemID.HallowedBar, 6)
                .AddIngredient(ItemID.SoulofLight, 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    internal class TeleportingAxePlayer : ModPlayer
    {
        public bool blueAxeReady;
        public bool pinkAxeReady;
    }
}