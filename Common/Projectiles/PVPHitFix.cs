using System;
using System.Collections.Generic;
using GivlsWeapons.Content.Items.Weapons;
using Terraria.WorldBuilding;

namespace GivlsWeapons.Common.Projectiles;
public static class PVPHitDictionaries //Since OnHitPlayer and ModifyHitPlayer don't work in PvP, these classes use ModPlayer code to replace their functionality
{
    //Contains the IDs of projectiles and their 
    public static Dictionary<int, OnHurtDelegate> onHurtFix = new Dictionary<int, OnHurtDelegate>();
    public static Dictionary<int, ModifyHurtDelegate> modifyHurtFix = new Dictionary<int, ModifyHurtDelegate>();
    public delegate void OnHurtDelegate(Player player, Projectile source, Player.HurtInfo info);
    public delegate void ModifyHurtDelegate(Player player, Projectile source, Player.HurtModifiers modifiers);

    /// <summary>
    /// Adds a delegate to the on hit dictionary with the Type of T as the key. Call in SetStaticDefaults to add equivalent OnHitPlayer code that functions in PvP
    /// </summary>
    /// <param name="onHitAction">The delegate to add. Uses (Player, Projectile, Player.HurtInfo) as parameters.</param>
    public static void RegisterOnHitAction<T>(OnHurtDelegate onHitAction) where T : ModProjectile
    {
        int projectileType = ModContent.ProjectileType<T>();
        onHurtFix[projectileType] = onHitAction;
    }
    /// <summary>
    /// Adds a delegate to the modify hit dictionary with the Type of T as the key. Call in SetStaticDefaults to add equivalent ModifyHitPlayer code that functions in PvP
    /// </summary>
    /// <param name="modifyHitAction">The delegate to add. Uses (Player, Projectile, Player.HurtModifiers) as parameters.</param>
    public static void RegisterModifyHitAction<T>(ModifyHurtDelegate modifyHitAction) where T : ModProjectile
    {
        int projectileType = ModContent.ProjectileType<T>();
        modifyHurtFix[projectileType] = modifyHitAction;
    }
}
public class PVPHitFix : ModPlayer
{
    public override void OnHurt(Player.HurtInfo info)
    {
        if (PVPHitDictionaries.onHurtFix.TryGetValue(info.DamageSource.SourceProjectileType, out var action))
        {
            action(Player, Main.projectile[info.DamageSource.SourceProjectileLocalIndex], info);
        }
    }
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if(PVPHitDictionaries.modifyHurtFix.TryGetValue(modifiers.DamageSource.SourceProjectileType, out var action))
        {
            action(Player, Main.projectile[modifiers.DamageSource.SourceProjectileLocalIndex], modifiers);
        }
    }
}