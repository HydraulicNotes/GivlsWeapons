using System;
using GivlsWeapons.Content.Projectiles.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace GivlsWeapons.Content.Items.Weapons;

// ExampleCustomSwingSword is an example of a sword with a custom swing using a held projectile
// This is great if you want to make melee weapons with complex swing behaviour
public class HFMuramasa : ModItem
{
    public int attackType = 0; // keeps track of which attack it is

    public override void SetDefaults()
    {
        // Common Properties
        Item.width = 58;
        Item.height = 72;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Yellow;

        // Use Properties
        // Note that useTime and useAnimation for this item don't actually affect the behavior because the held projectile handles that. 
        // Each attack takes a different amount of time to execute
        // Conforming to the item useTime and useAnimation makes it much harder to design
        // It does, however, affect the item tooltip, so don't leave it out.
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.UseSound = SoundID.Item131 with
        {
            Volume = 0.6f,
            Pitch = 0.7f + attackType * 0.1f,
            PitchVariance = 0.05f,
            MaxInstances = 4,
        };

        // Weapon Properties
        Item.knockBack = 2.7f;  // The knockback of your sword, this is dynamically adjusted in the projectile code.
        Item.autoReuse = true; // This determines whether the weapon has autoswing
        Item.damage = 85; // The damage of your sword, this is dynamically adjusted in the projectile code.
        Item.DamageType = DamageClass.Melee; // Deals melee damage
        Item.noMelee = true;  // This makes sure the item does not deal damage from the swinging animation
        Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand

        // Projectile Properties
        Item.shoot = ModContent.ProjectileType<HFMuramasaProjectile>();
    }

    public override void UpdateInventory(Player player)
    {
        if (player.GetModPlayer<HFMuramasaPlayer>().quickdrawTimer >= 110)
        {
            attackType = 9;
        }
        if (player.HeldItem != Item)
        {
            attackType = 0;
        }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Using the shoot function, we override the swing projectile to set ai[0] (which attack it is)
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
        if (attackType == 9)
        {
            SoundEngine.PlaySound(SoundID.Item38 with { Volume = 1.3f, Pitch = 0.7f }, player.position);
            SoundEngine.PlaySound(SoundID.Item131 with
            {
                Volume = 1.2f,
                Pitch = 0.7f,
                PitchVariance = 0.1f,
                MaxInstances = 4,
            });
        }
        attackType = (attackType + 1) % 9; // Increment attackType to move the combo forward
        return false;
    }
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<HFMuramasaPlayer>().quickdrawTimer = 0;
        return null;
    }

    public override bool MeleePrefix()
    {
        return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Muramasa)
            .AddIngredient(ItemID.MartianConduitPlating, 50)
            .AddIngredient(ItemID.ExplosivePowder, 5)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class HFMuramasaPlayer : ModPlayer
{
    public int quickdrawTimer = 0; // Keep track of the charge's progress with this. Above the max, used to track animations

    public override void PreUpdate()
    {
        if (Player.HeldItem.type == ModContent.ItemType<HFMuramasa>() && !Player.dead)
        {
            float maxDust = Main.rand.NextFloat() - 0.75f + Math.Clamp(quickdrawTimer, 0f, 110f) * 0.01667f;
            //if player is not pressing movement keys. Pushing into a wall doesn't count as pressing movement keys.
            if ((!Player.controlLeft && !Player.controlRight || Player.velocity.X == 0f) && !Player.controlUp && !Player.controlDown && !Player.controlHook && !Player.controlJump && !Player.controlMount && !Player.controlUseItem)
            {
                if (quickdrawTimer < 110)
                {
                    quickdrawTimer++; //increase the timer while the player is holding the item and not moving
                    for (float i = 0; i < maxDust; i += 1f)
                    {
                        float dustAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                        Vector2 dustPos1 = Player.MountedCenter + dustAngle.ToRotationVector2() * Player.Hitbox.Width;
                        Dust particle = Dust.NewDustPerfect(dustPos1, DustID.DungeonWater, -dustAngle.ToRotationVector2() + Player.velocity);
                        particle.noGravity = true;
                    }
                    if (quickdrawTimer % 4 == 1)
                    {
                        Vector2 dustPos2 = Player.Center + Vector2.Lerp(new Vector2(-8f, 11f), new Vector2(-72f * Player.direction, 36f), MathHelper.SmoothStep(0, 1, quickdrawTimer * 0.0091f));
                        Dust particle2 = Dust.NewDustPerfect(dustPos2, DustID.DungeonWater, new Vector2(0f, -1.5f) + Player.velocity, Scale: 1.5f);
                        particle2.noGravity = true;
                    }
                }
            }
            else if (quickdrawTimer < 110)
            {
                quickdrawTimer = 0; //reset the charging progress if the player moves
            }
            if (quickdrawTimer >= 110) //draw dusts regardless of whether the player is moving
            {
                if (quickdrawTimer == 110)
                {
                    SoundEngine.PlaySound(SoundID.Item149 with { Volume = 2f }, Player.position);
                    /*                     for(int i = 0; i < 20; i++)
                                        {
                                            float dustAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                                            Vector2 dustPos = new Vector2(-72f * Player.direction, 36f);
                                            Dust particle = Dust.NewDustPerfect(dustPos, DustID.DungeonWater, dustAngle.ToRotationVector2());
                                            particle.noGravity = true;
                                        } */ //For some reason this doesn't do anything. I have no idea why, because the sound still plays.
                }
                quickdrawTimer++;
                for (float i = 0; i < maxDust; i += 1f)
                {
                    float dustAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    Vector2 dustPos = Player.MountedCenter + dustAngle.ToRotationVector2() * Player.Hitbox.Width;
                    Dust particle = Dust.NewDustPerfect(dustPos, DustID.DungeonWater, -dustAngle.ToRotationVector2() + Player.velocity);
                    particle.noGravity = true;
                }
                //full charge VFX goes here
            }
        }
        else
        {
            quickdrawTimer = 0;
        }
    }
}

public class HFMuramasaHold : PlayerDrawLayer
{
    private Asset<Texture2D> swordTexture;
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<HFMuramasa>() && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.ItemAnimationActive;
    }
    public override Position GetDefaultPosition()
    {
        Multiple layer = new Multiple();
        layer.Add(PlayerDrawLayers.BeforeFirstVanillaLayer, FacingLeft);
        layer.Add(new Between(PlayerDrawLayers.Shield, PlayerDrawLayers.SolarShield), FacingRight);
        return layer;
    }
    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (swordTexture == null)
        {
            swordTexture = ModContent.Request<Texture2D>("GivlsWeapons/Content/Items/Weapons/HFMuramasa");
        }

        if (drawInfo.shadow == 0f)
        {
            var position = drawInfo.Center + new Vector2(-30f * drawInfo.drawPlayer.direction, 18f) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y); // to avoid quivering
            drawInfo.DrawDataCache.Add(new DrawData(
                swordTexture.Value,
                position,
                null, //sourcerect
                Lighting.GetColor(drawInfo.Center.ToTileCoordinates()),
                MathHelper.ToRadians(90f + 15f * drawInfo.drawPlayer.direction),
                swordTexture.Size() * 0.5f,
                1f, // Scale.
                drawInfo.drawPlayer.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
                0
                ));
        }
    }
    public bool FacingLeft(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.direction == -1;
    }
    public bool FacingRight(PlayerDrawSet drawInfo)
    {
        return !FacingLeft(drawInfo);
    }
}

public class HFMuramasaSheath : PlayerDrawLayer
{
    private Asset<Texture2D> sheathTexture;
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<HFMuramasa>() && !drawInfo.drawPlayer.dead;
    }
    public override Position GetDefaultPosition()
    {
        Multiple layer = new Multiple();
        layer.Add(PlayerDrawLayers.BeforeFirstVanillaLayer, FacingLeft);
        layer.Add(new Between(PlayerDrawLayers.SolarShield, PlayerDrawLayers.HeldItem), FacingRight);
        return layer;
    }
    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (sheathTexture == null)
        {
            sheathTexture = ModContent.Request<Texture2D>("GivlsWeapons/Content/Items/Weapons/HFMuramasaSheath");
        }

        if (drawInfo.shadow == 0f)
        {
            var position = drawInfo.Center + new Vector2(-30f * drawInfo.drawPlayer.direction, 18f) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y); // to avoid quivering

            drawInfo.DrawDataCache.Add(new DrawData(
            sheathTexture.Value,
            position,
            null, //sourcerect
            Lighting.GetColor(drawInfo.Center.ToTileCoordinates()),
            MathHelper.ToRadians(90f + 15f * drawInfo.drawPlayer.direction),
            sheathTexture.Size() * 0.5f,
            1f, // Scale.
            drawInfo.drawPlayer.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
            0
            ));

            int timer = drawInfo.drawPlayer.GetModPlayer<HFMuramasaPlayer>().quickdrawTimer;
            int clampTimer = Math.Clamp(timer, 0, 110);
            if (timer > 0)
            {
                //Lerps between the start and end, SmoothStep is used to add some easing to it
                var starPosition = drawInfo.Center + Vector2.Lerp(new Vector2(-8f, 11f), new Vector2(-72f * drawInfo.drawPlayer.direction, 36f), MathHelper.SmoothStep(0, 1, clampTimer * 0.0091f)) - Main.screenPosition;
                //starPosition = new Vector2((int)starPosition.X, (int)starPosition.Y);
                for (int i = 0; i < 2; i++)
                {
                    drawInfo.DrawDataCache.Add(new DrawData(
                    TextureAssets.Extra[98].Value,
                    starPosition,
                    null, //sourcerect
                    new Color(0, 30, 179, 0),
                    MathHelper.ToRadians(35f + 90f * i),
                    TextureAssets.Extra[98].Size() * 0.5f,
                    0.2f + clampTimer * 0.005f + MathF.Sin((timer - clampTimer) * 0.04f) * 0.1f, // Scale slowly increases, then locks to max size, but oscillates using the sine of timer, -clampTimer to simultaneously delay oscillating and start it at 0
                    drawInfo.drawPlayer.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    0
                    ));
                    drawInfo.DrawDataCache.Add(new DrawData(
                    TextureAssets.Extra[98].Value,
                    starPosition,
                    null, //sourcerect
                    Color.White,
                    MathHelper.ToRadians(35f + 90f * i),
                    TextureAssets.Extra[98].Size() * 0.5f,
                    0.1f + clampTimer * 0.0025f + MathF.Sin((timer - clampTimer) * 0.04f) * 0.05f, // Verbatim the previous scaling line, but everything is halved
                    drawInfo.drawPlayer.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    0
                    ));
                }
            }
        }
    }
    public bool FacingLeft(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.direction == -1;
    }
    public bool FacingRight(PlayerDrawSet drawInfo)
    {
        return !FacingLeft(drawInfo);
    }
}