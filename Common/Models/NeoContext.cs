using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using System.Collections.Generic;


namespace FezBotRedux.Common.Models
{
    public class NeoContext : DbContext
    {
        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<DbUser> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Afk> Afks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Playing> Playings { get; set; }
        public DbSet<NeoHub> NeoHubSettings { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EFCore.db");
        }
    }
    public class NeoHub
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identitiy { get; set; }
        public ulong MsgId { get; set; }
        public ulong ChannelId { get; set; }

    }

    public class Blacklist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public DbUser User { get; set; }
        public DateTime Creation { get; set; }
        public string reason { get; set; }
    }

    public class DbUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public ulong Id { get; set; }
    }
    public class Guild
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public Settings Settings { get; set; }
    }
    public class Settings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public bool IsBlockled { get; set; }
        //more to come
    }
    public class Afk
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public DbUser User { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
    }

    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }
        public DbUser Owner { get; set; }
        public Guild Guild { get; set; }
        public DateTime Creation { get; set; }
        public int Uses { get; set; }
        public string Trigger { get; set; }
        public string Value { get; set; }
        public bool IfAttachment { get; set; } = false;
    }
    public class Playing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identitiy { get; set; }
        public string Name { get; set; }
    }
}
