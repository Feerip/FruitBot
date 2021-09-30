using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FruitBot
{
    public class RaidsSignup
    {
        public static class Roles
        {
            public enum BM
            {
                Base,
                Backup,
                PT13,
                PT2,
                NC,

                DPS1,
                DPS2,
                DPS3,

                Learner1,
                Learner2,

            }
            public enum Yaka
            {
                Base,
                MainStun,
                PT_Stun52,
                CPR_Stun0,
                DBL_JW,

                Shark10,
                Stun51,
                NT,

                Learner1,
                Learner2,
            }
            public static class Strings
            {
                public static class BM
                {
                    public static string Base = "Base";
                    public static string Backup = "Backup";
                    public static string PT13 = "PT 1/3";
                    public static string PT2 = "PT 2";
                    public static string NC = "NC";
                    public static string DPS1 = "DPS 1";
                    public static string DPS2 = "DPS 2";
                    public static string DPS3 = "DPS 3";
                    public static string Learner1 = "Learner 1";
                    public static string Learner2 = "Learner 2";
                }
                public static class Yaka
                {
                    public static string Base = "Base";
                    public static string MainStun = "Main Stun";
                    public static string PT_Stun52 = "Poison Tank + Stun 5";
                    public static string CPR_Stun0 = "CPR + Stun 0";
                    public static string DBL_JW = "Double + Jelly Wrangler";
                    public static string Shark10 = "Shark 10";
                    public static string Stun51 = "Stun 5";
                    public static string NT = "North Tank";
                    public static string Learner1 = "Learner 1";
                    public static string Learner2 = "Learner 2";
                }
            }
            public static class Emojis
            {
                public static class BM
                {
                    public static string Base = "🛡";
                    public static string Backup = "🇧";
                    public static string PT13 = ":one:";
                    public static string PT2 = ":two:";
                    public static string NC = "🐶";
                    public static string DPS1 = "⚔️";
                    public static string DPS2 = "<:Attack:787451644794896425>";
                    public static string DPS3 = "<:Drygore_longsword:782685476800823296>";
                    public static string Learner1 = "🦉";
                    public static string Learner2 = "🐸";
                }
                public static class Yaka
                {
                    public static string Base = "<:ShieldEx:882127693284507678>";
                    public static string MainStun = "💥";
                    public static string PT_Stun52 = "🤢";
                    public static string CPR_Stun0 = "❤️";
                    public static string DBL_JW = "🇩";
                    public static string Shark10 = "🦈";
                    public static string Stun51 = ":five:";
                    public static string NT = "🐍";
                    public static string Learner1 = "🐔";
                    public static string Learner2 = "🐱";
                }
            }



        }

        private List<SocketGuildUser> _inviteList = new();

        private Dictionary<SocketGuildUser, Roles.BM> _BM_Roles = new();
        private Dictionary<Roles.BM, SocketGuildUser> _BM_Players = new();
        private Dictionary<SocketGuildUser, Roles.BM> _BM_Player_Signup_Roles = new();

        private Dictionary<SocketGuildUser, Roles.Yaka> _Yaka_Roles = new();
        private Dictionary<Roles.Yaka, SocketGuildUser> _Yaka_Players = new();
        private Dictionary<SocketGuildUser, Roles.Yaka> _Yaka_Player_Signup_Roles = new();


        public Embed GenerateEmbed()
        {
            string empty = "Empty";
            EmbedBuilder builder = new();

            builder.Title = "Daily Raids Signup";
            builder.Description = "<t:1630458000>\n\n**[bm]**";
            builder.Color = new Color(0, 255, 0);

            //builder.AddField(null, "[bm]");

            //BM Roles
            builder.AddField($"{Roles.Emojis.BM.Base} {Roles.Strings.BM.Base}:", _BM_Players.ContainsKey(Roles.BM.Base) ? _BM_Players[Roles.BM.Base] : empty);
            builder.AddField($"{Roles.Emojis.BM.Backup} {Roles.Strings.BM.Backup}:", _BM_Players.ContainsKey(Roles.BM.Backup) ? _BM_Players[Roles.BM.Backup] : empty);
            builder.AddField($"{Roles.Emojis.BM.PT13} {Roles.Strings.BM.PT13}:", _BM_Players.ContainsKey(Roles.BM.PT13) ? _BM_Players[Roles.BM.PT13] : empty);
            builder.AddField($"{Roles.Emojis.BM.PT2} {Roles.Strings.BM.PT2}:", _BM_Players.ContainsKey(Roles.BM.PT2) ? _BM_Players[Roles.BM.PT2] : empty);
            builder.AddField($"{Roles.Emojis.BM.NC} {Roles.Strings.BM.NC}:", _BM_Players.ContainsKey(Roles.BM.NC) ? _BM_Players[Roles.BM.NC] : empty);
            builder.AddField($"{Roles.Emojis.BM.DPS1} {Roles.Strings.BM.DPS1}:", _BM_Players.ContainsKey(Roles.BM.DPS1) ? _BM_Players[Roles.BM.DPS1] : empty, true);
            builder.AddField($"{Roles.Emojis.BM.DPS2} {Roles.Strings.BM.DPS2}:", _BM_Players.ContainsKey(Roles.BM.DPS2) ? _BM_Players[Roles.BM.DPS2] : empty, true);
            builder.AddField($"{Roles.Emojis.BM.DPS3} {Roles.Strings.BM.DPS3}:", _BM_Players.ContainsKey(Roles.BM.DPS3) ? _BM_Players[Roles.BM.DPS3] : empty, true);
            builder.AddField($"{Roles.Emojis.BM.Learner1} {Roles.Strings.BM.Learner1}:", _BM_Players.ContainsKey(Roles.BM.Learner1) ? _BM_Players[Roles.BM.Learner1] : empty, true);
            builder.AddField($"{Roles.Emojis.BM.Learner2} {Roles.Strings.BM.Learner2}:", _BM_Players.ContainsKey(Roles.BM.Learner2) ? _BM_Players[Roles.BM.Learner2] : empty, true);


            return builder.Build();
        }

        public void BMSignup(SocketGuildUser discordUser, Roles.BM role)
        {
            if (AlreadySignedUp(discordUser))
                throw new Exception("You have already signed up for raids.");
            if (SignedUpForBM(discordUser))
                throw new Exception("You can only sign up for 1 role at BM.");

            //Add to the non-final signup sheet for BM
            _BM_Player_Signup_Roles.Add(discordUser, role);

            //Already signed up for Yaka, time to finalize
            if (SignedUpForYaka(discordUser))
            {
                FinalizeSignup(discordUser);
            }



        }

        public void YakaSignup(SocketGuildUser discordUser, Roles.Yaka role)
        {
            if (AlreadySignedUp(discordUser))
                throw new Exception("You have already signed up for raids.");
            if (SignedUpForYaka(discordUser))
                throw new Exception("You can only sign up for 1 role at BM.");

            //Add to the non-final signup sheet for Yaka
            _Yaka_Player_Signup_Roles.Add(discordUser, role);

            //Already signed for BM, time to finalize
            if (SignedUpForBM(discordUser))
            {
                FinalizeSignup(discordUser);
            }
        }

        public bool RoleTaken(Roles.BM role)
        {
            if (_BM_Players.ContainsKey(role))
                return true;
            else
                return false;
        }
        public bool RoleTaken(Roles.Yaka role)
        {
            if (_Yaka_Players.ContainsKey(role))
                return true;
            else
                return false;
        }
        public bool SignedUpForBM(SocketGuildUser discordUser)
        {
            if (_BM_Player_Signup_Roles.ContainsKey(discordUser))
                return true;
            else return false;
        }
        public bool SignedUpForYaka(SocketGuildUser discordUser)
        {
            if (_BM_Player_Signup_Roles.ContainsKey(discordUser))
                return true;
            else
                return false;
        }


        public bool AlreadySignedUp(SocketGuildUser discordUser)
        {
            if (_inviteList.Contains(discordUser))
                return true;
            else
                return false;
        }

        private int FinalizeSignup(SocketGuildUser discordUser)
        {
            Roles.BM Role_BM = _BM_Player_Signup_Roles[discordUser];
            Roles.Yaka Role_Yaka = _Yaka_Player_Signup_Roles[discordUser];

            if (_BM_Players.ContainsKey(Role_BM) || _Yaka_Players.ContainsKey(Role_Yaka))
            {
                _BM_Player_Signup_Roles.Remove(discordUser);
                _Yaka_Player_Signup_Roles.Remove(discordUser);
                throw new Exception("You were too slow, and one of your roles was already reserved for someone else. Please pick a new set of roles.");
            }
            else
            {
                _BM_Players.Add(Role_BM, discordUser);
                _BM_Roles.Add(discordUser, Role_BM);

                _Yaka_Players.Add(Role_Yaka, discordUser);
                _Yaka_Roles.Add(discordUser, Role_Yaka);
            }
            return 0;

        }

    }
}
