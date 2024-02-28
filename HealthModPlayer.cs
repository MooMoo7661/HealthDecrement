using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace HealthDecrement
{
	public class HealthModPlayer : ModPlayer
	{
        public int healthRemoved = 0;
        public bool locked = false;
       
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (Player.statLifeMax > 0 && !locked)
            {
                healthRemoved += ModContent.GetInstance<PlayerConfig>().HealthDecreaseCount;
            }
        }

        public override void PostUpdate()
        {
            Player.statLifeMax2 -= healthRemoved;
            if (Player.statLife > Player.statLifeMax2 || Player.statLife < 0)
            {
                Player.statLife = Player.statLifeMax2;
            }

            if (ModContent.GetInstance<PlayerConfig>().Ghosting && Player.statLifeMax2 <= 0)
            {
                Player.ghost = true;
                Player.Ghost();
            }
        }
    }

    public class PlayerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(false)]
        public bool Ghosting { get; set; }

        [DefaultValue(20)]
        [Range(0, 500)]
        public int HealthDecreaseCount { get; set; }
    }

    public class ResetHP : ModCommand   
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "resetHP".ToLower();

        public override string Description => "Resets the hp removal counter";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Reset " + caller.Player.name + "'s HP deduction amount"), Color.CornflowerBlue);
            caller.Player.GetModPlayer<HealthModPlayer>().healthRemoved = 0;
        }
    }

    public class LockHP : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "lockhp";

        public override string Description => "Locks max hp from decreasing on death";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Locked " + caller.Player.name + "'s max HP from decreasing"), Color.Red);
            caller.Player.GetModPlayer<HealthModPlayer>().locked = true;
        }
    }

    public class UnlockHP : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "unlockhp";

        public override string Description => "Unlocks max hp from decreasing on death";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Unlocked " + caller.Player.name + "'s max HP from decreasing"), Color.Orange);
            caller.Player.GetModPlayer<HealthModPlayer>().locked = false;
        }
    }

    public class SetCounter : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "settakenhp";

        public override string Description => "Sets the amount of taken HP. Useful for debugging or fixing values";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 1)
            {
                throw new UsageException("Expected one parameter, instead given " + args.Length);
            }

            if (!int.TryParse(args[0], out var counter))
            {
                throw new UsageException("Parameter must be an integer");
            }

            if (int.Parse(args[0]) < 0)
            {
                throw new UsageException("Parameter cannot be negative.");
            }

            if (int.Parse(args[0]) > caller.Player.statLifeMax)
            {
                throw new UsageException("Given value cannot be greater than your maximum health.");
            }

            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Set " + caller.Player.name + "'s taken HP to " + args[0]), Color.MediumPurple);
            caller.Player.GetModPlayer<HealthModPlayer>().healthRemoved = int.Parse(args[0]);
        }
    }
}