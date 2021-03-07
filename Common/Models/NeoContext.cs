using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FezBotRedux.Common.Models {
    public class NeoContext : DbContext {
        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<DbUser> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Afk> Afks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Playing> Playings { get; set; }
        public DbSet<NeoHub> NeoHubSettings { get; set; }
        public DbSet<NeoBet> NeoBet { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite("Data Source=EFCore.db");
            optionsBuilder.UseLazyLoadingProxies();

        }
    }
    public class NeoBet {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public ulong msgID { get; set; }
        public string BetName { get; set; }
        public ulong ChannelId { get; set; }
        public bool open { get; set; }
        public virtual HashSet<Bet> Bets { get; set; } = new HashSet<Bet>();
        public virtual HashSet<NeoBets> userBets { get; set; } = new HashSet<NeoBets>();
    }

    public class Bet {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public string BetName { get; set; }
        public double BetRate { get; set; }
    }

    public class NeoBets {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public virtual DbUser User { get; set; }
        public int BetAmount { get; set; }
        public int BetLoc { get; set; }
    }




    public class NeoHub {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identitiy { get; set; }
        public ulong MsgId { get; set; }
        public ulong ChannelId { get; set; }

    }

    public class Blacklist {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public virtual DbUser User { get; set; }
        public DateTime Creation { get; set; }
        public string reason { get; set; }
    }

    public class DbUser {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public ulong Id { get; set; }
        public int Cash {
            get { return _cash; }
            set {
                _cash = value;
            }
        }
        [NotMapped]
        public int _cash { get; set; }
    }
    public class Guild {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public virtual Settings Settings { get; set; }
    }
    public class Settings {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public bool IsBlockled { get; set; }
        //more to come
    }
    public class Afk {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public virtual DbUser User { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
    }

    public class Tag {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public virtual DbUser Owner { get; set; }
        public virtual Guild Guild { get; set; }
        public DateTime Creation { get; set; }
        public int Uses { get; set; }
        public string Trigger { get; set; }
        public string Value { get; set; }
        public bool IfAttachment { get; set; } = false;
    }
    public class Playing {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identitiy { get; set; }
        public string Name { get; set; }
    }
}
