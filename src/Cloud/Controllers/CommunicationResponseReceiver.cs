using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud;

[Route("api/[controller]")]
[ApiController]
public sealed class CommunicationResponseReceiver : ControllerBase
{
    private readonly ICommunicationManager _communicationManager;

    public CommunicationResponseReceiver(ICommunicationManager communicationManager) =>
        _communicationManager = communicationManager;

    [HttpPost]
    public void ReceiveResponse([FromBody] ICommunicationResponse communicationResponse)
    {
        _communicationManager.SetResponse(communicationResponse);
    }
}