using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud ;

    public sealed class SignalRHub : Hub
    {
        private readonly ILogger<SignalRHub> _logger;

        public SignalRHub(ILogger<SignalRHub> logger)
        {
            _logger = logger;
        }

        public async  Task sendMessageAsync(HttpRequestMessage httpRequestMessage, CancellationToken stoppingToken)
        {
           _logger.LogInformation($"{nameof(sendMessageAsync)} started");
        Ensure.NotNull(httpRequestMessage);
            await  Clients.Caller.SendAsync("RecieveHttpMessage", httpRequestMessage, stoppingToken);
            _logger.LogInformation($"{nameof(sendMessageAsync)} started");
        }
    }
