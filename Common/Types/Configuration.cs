using Newtonsoft.Json;
using System;
using System.IO;

namespace FezBotRedux.Common.Types {
    /// <summary> 
    /// A file that contains information you either don't want public
    /// or will want to change without having to compile another bot.
    /// </summary>
    public class Configuration {
        /// <summary> The location of your bot's dll, ignored by the json parser. </summary>
        [JsonIgnore]
        private static readonly string Appdir = AppContext.BaseDirectory;

        /// <summary> Your bot's command prefix. Please don't pick `!`. </summary>
        public string Prefix { get; set; }
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; }
        /// <summary> Your bot's osuapi key. </summary>


        public Configuration() {
            Prefix = "f!";
            Owners = new ulong[] { 111476801762537472 };
            Token = null;
        }

        /// <summary> Save the configuration to the specified file location. </summary>
        public void Save(string dir = "data/configuration.json") {
            var file = Path.Combine(Appdir, dir);
            File.WriteAllText(file, ToJson());
        }

        /// <summary> Load the configuration from the specified file location. </summary>
        public static Configuration Load(string dir = "data/configuration.json") {
            var file = Path.Combine(Appdir, dir);
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
        }

        /// <summary> Convert the configuration to a json string. </summary>
        private string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}
