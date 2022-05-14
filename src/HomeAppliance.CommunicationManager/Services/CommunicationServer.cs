using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface ICommunicationServer
{
    Task ServeRequestAsync(ICommunicationRequest communicationRequest, CancellationToken cancellationToken);
}

public sealed class CommunicationServer : ICommunicationServer
{
    private readonly ILogger<CommunicationServer> _logger;
    private readonly ICommunicationHandler _communicationHandler;
    private readonly ICommunicationResponseSender _communicationResponseSender;

    public CommunicationServer(
        ILogger<CommunicationServer> logger,
        ICommunicationHandler communicationHandler,
        ICommunicationResponseSender communicationResponseSender)
    {
        _communicationHandler = communicationHandler;
        _communicationResponseSender = communicationResponseSender;
        _logger = logger;
    }

    public async Task ServeRequestAsync(ICommunicationRequest communicationRequest, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(ServeRequestAsync)} started [{nameof(communicationRequest)}={communicationRequest}]");

        ICommunicationResponse? response = null;
        try
        {
            response = await _communicationHandler.HandleRequestAsync(communicationRequest);
        }
        catch (Exception exception)
        {
            response = new ExceptionResponse(exception);
            throw;
        }
        finally
        {
            if (response != null)
            {
                response.RequestId = communicationRequest.Id;
                await _communicationResponseSender.SendResponseAsync(response, cancellationToken);
            }

            _logger.LogDebug($"{nameof(ServeRequestAsync)} finished [{nameof(response)}={response}]");
        }
    }
}