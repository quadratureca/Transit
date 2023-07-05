using TransitWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Mississauga>();
        services.AddHostedService<GoTransit>();
        //services.AddHostedService<RealTime>();
        //services.AddHostedService<Route>();
    })
    .Build();

host.Run();
