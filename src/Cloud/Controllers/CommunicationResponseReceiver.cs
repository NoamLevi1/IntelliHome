using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud;

[Route("api/[controller]")]
[ApiController]
public sealed class CommunicationResponseReceiver : ControllerBase
{
    private readonly ICommunicationClient _communicationClient;

    public CommunicationResponseReceiver(ICommunicationClient communicationClient) =>
        _communicationClient = communicationClient;

    [HttpPost]
    public void ReceiveResponse([FromBody] ICommunicationResponse communicationResponse)
    {
        _communicationClient.SetResponse(communicationResponse);
    }
}