using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace FezBotRedux.Common.Utility
{
    public class TupleList<T1,T2> : List<Tuple<T1,T2>>
    {
        public void Add (T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
    public static class NeoConsole
    {
        /// <summary> Write a string to the console on an existing line. </summary>
        /// <param name="text">String written to the console.</param>
        /// <param name="foreground">The text color in the console.</param>
        /// <param name="background">The background color in the console.</param>
        public static async Task Append(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            await Console.Out.WriteAsync(text);

        }

        internal static async Task NewLineArt(string line, ConsoleColor magenta)
        {
            await NewLine(line, magenta);
        }

        /// <summary> Write a string to the console on an new line. </summary>
        /// <param name="text">String written to the console.</param>
        /// <param name="foreground">The text color in the console.</param>
        /// <param name="background">The background color in the console.</param>
        public static async Task NewLine(string text = "", ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            await Console.Out.WriteAsync(Environment.NewLine + text);
        }

        private static readonly TupleList<string, ConsoleColor> ColorQueue = new TupleList<string, ConsoleColor>
        {
            {"§black§"      ,ConsoleColor.Black},
            {"§blue§"       ,ConsoleColor.Blue},
            {"§cyan§"       ,ConsoleColor.Cyan},
            {"§darkblue§"   ,ConsoleColor.DarkBlue},
            {"§darkcyan§"   ,ConsoleColor.DarkCyan},
            {"§darkgray§"   ,ConsoleColor.DarkGray},
            {"§darkgreen§"  ,ConsoleColor.DarkGreen},
            {"§darkmagenta§",ConsoleColor.DarkMagenta},
            {"§darkred§"    ,ConsoleColor.DarkRed},
            {"§gray§"       ,ConsoleColor.Gray},
            {"§green§"      ,ConsoleColor.Green},
            {"§magenta§"    ,ConsoleColor.Magenta},
            {"§red§"        ,ConsoleColor.Red},
            {"§white§"      ,ConsoleColor.White},
            {"§yellow§"     ,ConsoleColor.Yellow}
        };

        private static async Task ColorNewLine(string text = "")
        {
            var rx = new Regex(@"([§])(?:(?=(\\?))\2.)*?\1", RegexOptions.CultureInvariant);
            var regexlist = new TupleList<int, string>();
            foreach (Match match in rx.Matches(text))
            {
                var i = match.Index;
                var x = match.Captures[0].Value;
                regexlist.Add(i, x);
            }
            await Append(text.Substring(0, text.IndexOf('§')));
            foreach (var tuple in regexlist)
            {
                var extext = text;
                var colorstring = extext.Substring(tuple.Item1, tuple.Item2.Length);
                extext = extext.Replace(extext.Substring(0, tuple.Item1 + tuple.Item2.Length), "");
                await Append(
                    extext.Substring(
                        0, extext.IndexOf('§') == -1 ? extext.Length : extext.IndexOf('§')
                    )
                    , ColorQueue.Find(x => x.Item1 == colorstring).Item2);

                //text = text.Substring(tuple.Item1 + tuple.Item2);
            }
            await NewLine();
        }

        public static async Task Log(LogSeverity severity, string source, string message)
        {
            var x = new StringBuilder();
            x.Append($"§gray§{DateTime.Now,-10:hh:mm:ss} ");
            x.Append($"§darkgreen§{severity,8} | ");
            x.Append($"§red§{source,8}: ");
            x.Append("§white§" + message);
            await ColorNewLine(x.ToString());
        }

        public static async Task Log(IUserMessage msg)
        {
            var x = new StringBuilder();
            var channel = (msg.Channel as IGuildChannel);
            x.Append($"§gray§{DateTime.Now,-10:hh:mm:ss} ");

            x.Append(channel?.Guild == null
                ? "§magenta§[PM] "
                : $"§darkmagenta§[{channel.Guild.Name,-8} #{channel.Name,-8}] ");

            x.Append($"§green§{msg.Author,-10}: ");
            x.Append("§white§" + msg.Content);
            await ColorNewLine(x.ToString());
        }

        public static async Task Log(CommandContext c)
        {
            var x = new StringBuilder();
            var channel = (c.Channel as SocketGuildChannel);
            x.Append($"§gray§{DateTime.Now,-10:hh:mm:ss} ");

            x.Append(channel == null ? "§magenta§[PM] " : $"§darkmagenta§[{c.Guild.Name,-8} #{channel.Name,-8}] ");

            x.Append($"§green§{c.User,-10}: ");
            x.Append("§white§" + c.Message.Content);
            await ColorNewLine(x.ToString());
        }
    }

}
