using TransitWorker.Models;
using TransitWorker.Position;
using TransitWorker.Route;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddHostedService<TransitWorker.Position.Hamilton>();
        services.AddHostedService<TransitWorker.Route.Hamilton>();
        services.AddHostedService<TransitWorker.Route.Mississauga>();
        services.AddHostedService<TransitWorker.Position.Mississauga>();
        services.AddHostedService<TransitWorker.Position.GoTransit>();
        services.AddHostedService<TransitWorker.Route.GoTransit>();
        services.AddHostedService<TransitWorker.Position.YRT>();
        services.AddHostedService<TransitWorker.Route.YRT>();
        services.AddHostedService<TransitWorker.Position.TTC>();
        services.AddHostedService<TransitWorker.Route.TTC>();
    })
    .Build();

host.Run();
