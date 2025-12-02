using Microsoft.AspNetCore.SignalR;

namespace GlobalDisasterManagement.Hubs
{
    public class DisasterAlertHub : Hub
    {
        private readonly ILogger<DisasterAlertHub> _logger;

        public DisasterAlertHub(ILogger<DisasterAlertHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinCityGroup(string cityId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"City_{cityId}");
            _logger.LogInformation("Client {ConnectionId} joined city group {CityId}", Context.ConnectionId, cityId);
        }

        public async Task LeaveCityGroup(string cityId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"City_{cityId}");
            _logger.LogInformation("Client {ConnectionId} left city group {CityId}", Context.ConnectionId, cityId);
        }

        public async Task JoinLGAGroup(string lgaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"LGA_{lgaId}");
            _logger.LogInformation("Client {ConnectionId} joined LGA group {LgaId}", Context.ConnectionId, lgaId);
        }

        public async Task LeaveLGAGroup(string lgaId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"LGA_{lgaId}");
            _logger.LogInformation("Client {ConnectionId} left LGA group {LgaId}", Context.ConnectionId, lgaId);
        }
    }
}
