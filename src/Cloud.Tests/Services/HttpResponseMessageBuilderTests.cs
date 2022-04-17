using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public class HttpResponseMessageBuilderTests
{
    private readonly HttpResponseMessageBuilder _builder;

    public HttpResponseMessageBuilderTests() =>
        _builder = new HttpResponseMessageBuilder(new Mock<ICommunicationClient>().Object);

    [TestMethod]
    [DynamicData(nameof(GetHttpStatusCodes), DynamicDataSourceType.Method)]
    public async Task TestStatusCodeStaysSame(HttpStatusCode httpStatusCode) =>
        Assert.AreEqual(httpStatusCode, (await BreakAndBuildAsync(new HttpResponseMessage(httpStatusCode))).StatusCode);

    [TestMethod]
    [DataRow("24.11.41.85")]
    [DataRow("45.85.32.33")]
    [DataRow("37.18.30.59")]
    [DataRow("69.69.22.8")]
    [DataRow("27.48.55.41")]
    [DataRow("59.58.91.60")]
    [DataRow("58.17.38.8")]
    [DataRow("53.44.54.94")]
    [DataRow("65.72.76.85")]
    [DataRow("46.13.87.49")]
    [DataRow("76.98.37.48")]
    [DataRow("42.3.78.92")]
    [DataRow("49.28.91.12")]
    [DataRow("8.33.32.59")]
    [DataRow("6.23.30.12")]
    [DataRow("17.33.12.41")]
    [DataRow("94.44.49.81")]
    [DataRow("7.12.49.2")]
    [DataRow("75.70.63.5")]
    [DataRow("31.2.17.30")]
    [DataRow("9.24.96.22")]
    public async Task TestVersionStaysSame(string versionString)
    {
        var version = Version.Parse(versionString);

        Assert.AreEqual(version, (await BreakAndBuildAsync(new HttpResponseMessage {Version = version})).Version);
    }

    [TestMethod]
    [DataRow("daa4040c-d2e2-46b0-a65f-43090de4bb03", "d64b27ea-2749-4bf3-8766-4104fb81bb70")]
    [DataRow("0c0debb8-59e1-4f56-9ba3-a49b08c32e60", "b0cdddcc-ef9e-4703-a1e8-f53fbafcf9ad")]
    [DataRow("f9b6916d-b114-48c3-b42c-bc087a2aa980", "906fc015-8a0c-43d5-bcd6-1f755b8bac84")]
    [DataRow("433b9df9-272c-42bd-84d2-7cb4743da43d", "5c0b062b-df8a-4c3e-a7fc-04038b0074fb")]
    [DataRow("8a914492-86e6-41a4-b451-c2cc63097a7b", "c1533e8e-eda2-40e0-b741-15378a748f41")]
    [DataRow("f6755343-8a10-44d3-b3bf-356a498018af", "3b9926ec-19e8-45ac-8076-470e3abb4ace")]
    [DataRow("67914301-53b4-42db-997f-eaf29a8b8b9d", "8e69733c-c4b3-4036-a67d-fe4374373520")]
    [DataRow("52821d6a-ee9d-4057-8f2f-95b6bce990ab", "5eb1b9dc-c3f2-418f-8607-521f7a88d81e")]
    [DataRow("a33b6b98-3a74-4f24-a5b0-69b0c962cc79", "232da13b-138b-4d6d-bcfe-ab731bcd79b3")]
    [DataRow("7867a113-4757-475c-9e8d-00163557418a", "5675392e-eab8-4b1a-9402-08d5095518ed")]
    [DataRow("7b75efc2-39ce-4ff7-a728-cdb9ae9e0680", "3f8c4053-baf4-4410-9f8b-c8c6ad8b2779")]
    [DataRow("d55bf50d-4f95-486d-a33a-ff9ac5c301e8", "92f304fa-d075-40c3-a64b-10eb86860196")]
    [DataRow("e2d9f440-0484-4a3e-9fb3-00c0d5cef9ce", "50bbe908-66e6-4b87-9502-32bc68217e9d")]
    [DataRow("6859a46a-c5a2-47a8-8a8e-4c5bd3c71ef1", "f50ef40e-64e3-49a6-839c-2f7ef17b657a")]
    [DataRow("52d12af7-e811-41e7-8fcd-e99d7b8645fb", "ee0c7760-658b-480b-a407-9451bbf286c5")]
    [DataRow("95e2f434-3671-40ad-997c-90a646d32b23", "2d9e5e73-2ba3-4b91-bf51-8d453a50a021")]
    [DataRow("ce898077-1a5b-4f05-a8d5-e1d24c955400", "b9884912-52fa-4bb4-80f7-ff194d8f3db9")]
    [DataRow("2d66f607-409f-45e8-aae4-ca50c80d8303", "6c09564e-787c-4c53-93a6-acc8065060b7")]
    [DataRow("5fb900cc-b3bc-4d7d-8de9-d2e70838aa59", "d8b0e104-bf60-4968-b880-4bbf411c0eac")]
    [DataRow("30cae5ab-2aca-4797-aa83-d07704dfaed9", "d9b82c8f-e0e2-475b-992b-64c3c7ad7862")]
    [DataRow("92712216-d4ec-4734-9e70-165dfa057b09", "ecb692d8-b69a-4a24-af4f-0e805eb8a484")]
    public async Task TestHeadersStaysSame(string headerName, string headerValue)
    {
        var response = new HttpResponseMessage();
        response.Headers.Add(headerName, headerValue);

        CollectionAssert.AreEquivalent(
            response.Headers.Select(kvp => $"Key={kvp.Key}:Value={string.Join(',', kvp.Value)}").ToList(),
            (await BreakAndBuildAsync(response)).Headers.Select(kvp => $"Key={kvp.Key}:Value={string.Join(',', kvp.Value)}").ToList());
    }

    public static IEnumerable<object[]> GetHttpStatusCodes() =>
        Enum.GetValues<HttpStatusCode>().Select(
            item => new object[]
            {
                item
            });

    private async Task<HttpResponseMessage> BreakAndBuildAsync(HttpResponseMessage httpResponseMessage) =>
        _builder.Build(new HttpResponseData
        {
            ContentId = Guid.Empty,
            ContentHeaders = httpResponseMessage.Content.Headers,
            Headers = httpResponseMessage.Headers,
            ReasonPhrase = httpResponseMessage.ReasonPhrase,
            RequestData = httpResponseMessage.RequestMessage is null
                ? null
                : await HttpRequestData.FromHttpRequestMessageAsync(httpResponseMessage.RequestMessage, CancellationToken.None),
            Version = httpResponseMessage.Version,
            StatusCode = httpResponseMessage.StatusCode
        });
}