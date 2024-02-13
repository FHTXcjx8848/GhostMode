using System.Reflection;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System;
using TShockAPI.Hooks;
using TShockAPI.DB;
using System.Data;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GhostMode
{
    [ApiVersion(2, 1)]
    public class GhostModePlugin : TerrariaPlugin
    {
        public GhostModePlugin(Main game) : base(game)
        {
        }
        public override string Name
        {
            get
            {
                return "GhostMode";
            }
        }

        public override Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        public override string Author
        {
            get
            {
                return "Cjx";
            }
        }

        public override string Description
        {
            get
            {
                return "幽灵模式插件";
            }
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerLeave.Register(this, new HookHandler<LeaveEventArgs>(this.OnPlayerLeave));
            Commands.ChatCommands.Add(new Command("ghost.use", PlayerCommand, "ghost","幽灵","观战"));
            Commands.ChatCommands.Add(new Command("ghost.admin",AdminCommand , "tpgall"));
            Commands.ChatCommands.Add(new Command("ghost.use", TpCommand, "tpg"));
        }
        public static List<TSPlayer> plrs = new List<TSPlayer>();
        public static void TpCommand(CommandArgs args)
        {
            if(args.Parameters.Count < 1)
            {
                args.Player.SendInfoMessage("使用/tpg <玩家名>,传送到某一玩家的位置.");
                return;
            }
            if (plrs.Find(p => p.Name == args.Player.Name) == null)
            {
                args.Player.SendErrorMessage("你非处于幽灵状态无法使用此指令.");
                return;
            }
            string name = args.Parameters[0].ToString();
            var tsplr = TSPlayer.FindByNameOrID(name);
            if(tsplr.Count == 0)
            {
                args.Player.SendErrorMessage("此玩家不在线.");
                return;
            }
            args.Player.Teleport(tsplr[0].X, tsplr[0].Y);
            args.Player.SendSuccessMessage("传送成功.");
        }
        public static void AdminCommand(CommandArgs args)
        {
            foreach(var plr in plrs)
            {
                plr.Teleport(args.Player.X,args.Player.Y);
                plr.SendInfoMessage($"你被传送到 {args.Player.Name} 的位置.");
            }
            args.Player.SendSuccessMessage("传送所有幽灵到自己位置.");
        }
        public static void PlayerCommand(CommandArgs args)
        {
            var plr = plrs.Find(p=>p.Name ==args.Player.Name);
            var tsplr = args.Player;
            if (plr != null)
            {
                tsplr.TPlayer.ghost = false;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", tsplr.Index);
                plrs.Remove(plr);
                plr.Teleport(Main.spawnTileX, Main.spawnTileY);
                tsplr.SendSuccessMessage("成功退出幽灵模式.");
            }
            else
            {
                tsplr.TPlayer.ghost = true;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", tsplr.Index);
                plrs.Add(tsplr);
                tsplr.SendSuccessMessage("成功进入幽灵模式.");
            }
        }
        public void OnPlayerLeave(LeaveEventArgs args)
        {
            var plr = plrs.Find(p => p.Name == TShock.Players[args.Who].Name);
            if (plr != null)
            {
                var tsplr = TShock.Players[args.Who];
                tsplr.TPlayer.ghost = false;
                plrs.Remove(plr);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
              
            }
            base.Dispose(disposing);
        }
    }
}