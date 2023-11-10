using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Dusts;
using Terraria.ID;
using System;

namespace GivlsWeapons.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shoes)]
    internal class QuicksilverBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 17;
            Item.height = 14;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Green;
        }

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.accRunSpeed = 6f; // The player's maximum run speed with accessories
            player.moveSpeed += 0.05f; // The acceleration multiplier of the player's movement speed
            
            if(Math.Abs(player.velocity.X) > 5f && player.velocity.Y == 0 && Collision.SolidCollision(player.Bottom, 1, 1))
            {
                player.statMana += 1;
                
                if (Main.rand.NextBool(2))
                {
                    int numToSpawn = Main.rand.Next(3);
                    Vector2 dustSpawnSpot = player.direction == 1 ? player.BottomLeft : player.BottomRight;
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        Dust.NewDust(dustSpawnSpot, 10, 10, ModContent.DustType<QuicksilverDust>(), player.velocity.X * 0.1f, player.velocity.Y * 0.1f,
                            0, default, 1f);
                    }
                }
            }
        }

        public override void UpdateVanity(Player player)
        {
            if (Math.Abs(player.velocity.X) > 5f && player.velocity.Y == 0 && Collision.SolidCollision(player.Bottom, 1, 1))
            {
                if (Main.rand.NextBool(2))
                {
                    int numToSpawn = Main.rand.Next(3);
                    Vector2 dustSpawnSpot = player.direction == 1 ? player.BottomLeft : player.BottomRight;
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        Dust.NewDust(dustSpawnSpot, 10, 10, ModContent.DustType<QuicksilverDust>(), player.velocity.X * 0.1f, player.velocity.Y * 0.1f,
                            0, default, 1f);
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SilverGreaves)
                .AddIngredient(ItemID.Diamond, 2)
                .AddTile(TileID.Anvils)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.TungstenGreaves)
                .AddIngredient(ItemID.Diamond, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
