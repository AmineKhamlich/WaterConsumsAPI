using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WConsumsAPI.Hubs
{
    public class NotificacioHub : Hub
    {
        // En aquesta aplicació el Hub només fa d'altaveu (d'API a Android).
        // No cal que tinguem mètodes receptors aquí, ja que ho enviem tot
        // des del Controller via el IHubContext.
        
        public override async Task OnConnectedAsync()
        {
            // Quan un mòbil Android es connecta...
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Quan la connexió es perd...
            await base.OnDisconnectedAsync(exception);
        }
    }
}
