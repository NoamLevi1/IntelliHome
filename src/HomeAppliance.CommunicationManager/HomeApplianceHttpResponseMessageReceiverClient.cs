using System.Net.Mime;
using System.Text;
using IntelliHome.Common;
using Newtonsoft.Json;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IHomeApplianceHttpResponseMessageReceiverClient
{
    Task SendAsync(ReceiveHttpResponseRequest receiveHttpResponseRequest, CancellationToken cancellationToken);
}

public sealed class HomeApplianceHttpResponseMessageReceiverClient : IHomeApplianceHttpResponseMessageReceiverClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializer _serializer;

    public HomeApplianceHttpResponseMessageReceiverClient()
    {
        _httpClient = new HttpClient();
        _serializer = JsonSerializer.Create(new JsonSerializerSettings().ConfigureCommon());
    }

    public async Task SendAsync(ReceiveHttpResponseRequest receiveHttpResponseRequest, CancellationToken cancellationToken) =>
        (await _httpClient.PostAsync(
            "https:/host.docker.internal:7050/Api/HomeApplianceHttpResponseMessageReceiver",
            new StringContent(
                _serializer.SerializeToString(receiveHttpResponseRequest),
                Encoding.UTF8,
                MediaTypeNames.Application.Json),
            cancellationToken)).
        EnsureSuccessStatusCode();
}