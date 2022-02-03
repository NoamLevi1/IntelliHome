
using Microsoft.AspNetCore.SignalR.Client;

namespace IntelliHome.HomeAppliance.CommunicationManager ;
using Microsoft.AspNetCore.SignalR;

public class SignalRClient
    {
        private readonly ILogger<SignalRClient> _logger;
        private readonly HubConnection _connection;

    SignalRClient(ILogger<SignalRClient> logger )
        {
            _logger = logger;
            _connection = new HubConnectionBuilder().WithUrl("/Connectionhub").Build();
        ConfigureOnDisconnect();
        }

    private void ConfigureOnDisconnect()
    {
        _connection.Closed += async (exception) =>
        {
            if (exception == null)
            {
                await EstablishConnection(new CancellationToken());
            }
            else
            {
                _logger.LogInformation($"{nameof(EstablishConnection)} Exception {exception}");
            }
        };
    }

    private void configureOnHttpRequest()
    { /*
        _connection.On("ReceiveHttpRequest",
            (httpRequestMessage) =>
            {
                //fix me :( 
                
         */   }
    

        public async Task EstablishConnection(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(EstablishConnection)} started");
       
            try
            {
                await _connection.StartAsync(cancellationToken);
               _logger.LogInformation($"{nameof(EstablishConnection)} SignalR Connected");
        }
            catch (Exception e)
            {
            _logger.LogInformation($"{nameof(EstablishConnection)} Exception {e}" );
            
            }

            _logger.LogInformation($"{nameof(EstablishConnection)} Completed");
    }


    




    }