using TransitWorker.Models;
using TransitWorker.Position;
using TransitWorker.Route;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        //services.AddHostedService<TransitWorker.Position.Mississauga>();
        //services.AddHostedService<TransitWorker.Position.GoTransit>();
        //services.AddHostedService<TransitWorker.Position.YRT>();
        services.AddHostedService<TransitWorker.Route.Mississauga>();
    })
    .Build();

host.Run();
