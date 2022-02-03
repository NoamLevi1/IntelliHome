using IntelliHome.Common;
using Microsoft.AspNetCore.SignalR;

namespace IntelliHome.Cloud ;

    public sealed class ConnectionHub : Hub
    {
        private readonly ILogger<ConnectionHub> _logger;

        public ConnectionHub(ILogger<ConnectionHub> logger)
        {
            _logger = logger;
        }

        public async  Task sendMessageAsync(HttpRequestMessage httpRequestMessage)
        {
           _logger.LogInformation($"{nameof(sendMessageAsync)} started");
        Ensure.NotNull(httpRequestMessage);
            await  Clients.Caller.SendAsync("RevieveHttpMessage", httpRequestMessage);
            _logger.LogInformation($"{nameof(sendMessageAsync)} started");
        }
    }
