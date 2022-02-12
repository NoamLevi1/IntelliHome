using IntelliHome.Common;
using Microsoft.AspNetCore.Mvc;

namespace IntelliHome.Cloud;

[Route("Api/HomeApplianceHttpResponseMessageReceiver")]
public sealed class HomeApplianceHttpResponseMessageReceiverController : Controller
{
    private readonly IHomeApplianceTunneledHttpMessageHandler _homeApplianceTunneledHttpMessageHandler;

    public HomeApplianceHttpResponseMessageReceiverController(IHomeApplianceTunneledHttpMessageHandler homeApplianceTunneledHttpMessageHandler) =>
        _homeApplianceTunneledHttpMessageHandler = homeApplianceTunneledHttpMessageHandler;

    [HttpPost("/")]
    public void ReceiveHttpResponse([FromBody] ReceiveHttpResponseRequest receiveHttpResponseRequest)
    {
        _homeApplianceTunneledHttpMessageHandler.ReceiveHttpResponse(receiveHttpResponseRequest);
    }
}