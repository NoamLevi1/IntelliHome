﻿using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHost(args).Run();
    }

    private static IHost CreateHost(string[] args) =>
        Host.
            CreateDefaultBuilder(args).
            ConfigureServices(
                (hostBuilderContext, services) =>
                    services.
                        AddConfigurationManager<CommunicationManagerConfiguration>().
                        AddEnvironmentDependentSingleton<IIdGenerator, IdGenerator, DevelopmentIdGenerator>(hostBuilderContext.HostingEnvironment).
                        AddSingleton<ICloudUrlBuilder, CloudUrlBuilder>().
                        AddSingleton<ICommunicationResponseSender, CommunicationResponseSender>().
                        AddSingleton<ICommunicationHandler, CommunicationHandler>().
                        AddSingleton<ICommunicationServer, CommunicationServer>().
                        AddSingleton<IHttpResponseMessageDisassembler, HttpResponseMessageDisassembler>().
                        AddSingleton<IHomeAssistantClient, HomeAssistantClient>().
                        AddSingleton<IRemoteStreamManager, RemoteStreamManager>().
                        AddSingleton<IRemoteContentManager, RemoteContentManager>().
                        AddHostedService<CommunicationRequestReceiver>()).
            Build();
}