using IntelliHome.Common;

namespace IntelliHome.Cloud.Tests;

public sealed class MockRequest : CommunicationRequest, IRequestWithResponse<MockResponse>
{
}

public sealed class MockResponse : CommunicationResponse
{
}

public sealed class MockVoidRequest : CommunicationRequest, IVoidRequest
{
}