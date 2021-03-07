using Discord.WebSocket;
using System.Threading.Tasks;

namespace FezBotRedux.Common.Extensions {
    public static class SocketMessageExtensions {
        public static async Task DeleteAfter(this SocketMessage m, int seconds) {
            await Task.Delay(seconds * 1000).ConfigureAwait(false);
            await m.DeleteAsync().ConfigureAwait(false);
        }
    }
}
