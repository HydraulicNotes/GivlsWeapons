using System;
using System.Collections.Generic;
using GivlsWeapons.Content.Items.Weapons;
using Terraria.WorldBuilding;

namespace GivlsWeapons.Common.Projectiles;
public static class PVPHitDictionaries //Since OnHitPlayer and ModifyHitPlayer don't work in PvP, these classes use ModPlayer code to replace their functionality
{
    /// <summary>
    /// The methods called for a given type when hitting a player. Assign a delegate method to your projectile's type to add code equivalent to OnHitPlayer that functions in PvP.
    /// The parameters must be (Player, Projectile, Player.HurtInfo), and can be named as you wish
    /// </summary>
    public static Dictionary<int, OnHurtDelegate> onHurtFix = new Dictionary<int, OnHurtDelegate>();
    /// <summary>
    /// The methods called for a given type to modify projectile hits on players. Assign a delegate method to your projectile's type to add code equivalent to ModifyHitPlayer that functions in PvP.
    /// The parameters must be (Player, Projectile, ref Player.HurtModifiers), and can be named as you wish
    /// </summary>
    public static Dictionary<int, ModifyHurtDelegate> modifyHurtFix = new Dictionary<int, ModifyHurtDelegate>();
    public delegate void OnHurtDelegate(Player player, Projectile source, Player.HurtInfo info);
    public delegate void ModifyHurtDelegate(Player player, Projectile source, ref Player.HurtModifiers modifiers);
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
            action(Player, Main.projectile[modifiers.DamageSource.SourceProjectileLocalIndex], ref modifiers);
        }
    }
}