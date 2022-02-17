using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud;

[Route("api/[controller]")]
[ApiController]
public sealed class HomeApplianceHttpResponseMessageReceiverController : ControllerBase
{
    private readonly IHomeApplianceTunneledHttpMessageHandler _homeApplianceTunneledHttpMessageHandler;

    public HomeApplianceHttpResponseMessageReceiverController(IHomeApplianceTunneledHttpMessageHandler homeApplianceTunneledHttpMessageHandler) => 
        _homeApplianceTunneledHttpMessageHandler = homeApplianceTunneledHttpMessageHandler;

    [HttpPost]
    public void ReceiveHttpResponse([FromBody] ReceiveHttpResponseRequest receiveHttpResponseRequest)
    {
        _homeApplianceTunneledHttpMessageHandler.ReceiveHttpResponse(receiveHttpResponseRequest);
    }
}