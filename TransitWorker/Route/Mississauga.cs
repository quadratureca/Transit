﻿using Microsoft.Data.SqlClient;
using ProtoBuf;
using System;
using System.Data;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using TransitRealtime;
using TransitWorker.Models;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace TransitWorker.Route;

public class Mississauga : BackgroundService
{
    private readonly ILogger<Mississauga> _logger;

    public Mississauga(ILogger<Mississauga> logger)
    {
        _logger = logger;
    }

    public class Route
    {
        public Guid id { get; set; }
        public string route_id { get; set; }
        public string agency_id { get; set; }
        public string route_short_name { get; set; }
        public string route_long_name { get; set; }
        public string route_desc { get; set; }
        public string route_type { get; set; }
        public string route_url { get; set; }
        public string route_color { get; set; }
        public string route_text_color { get; set; }
        public int route_sort_order { get; set; }
        public int continuous_pickup { get; set; }
        public int continuous_drop_off { get; set; }
    }

    static void WriteRoutes(List<Route> routes, string databaseConnection)
    {
        
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string databaseConnection = @"Data Source=quadrature.ca;Initial Catalog=Transit;Persist Security Info=True;User ID=sa;Password=M155155auga?;TrustServerCertificate=true";
        Stream data;
        List<Route> routes = new List<Route>();
        string line;

        try
        {
            HttpClient client = new HttpClient();

            _logger.LogInformation("Route.Mississauga running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string mississauga = "https://www.miapp.ca/GTFS/google_transit.zip";

                    try
                    {
                        var x = client.GetStreamAsync(mississauga);
                        data = x.Result;

                        Stream unzippedEntryStream; // Unzipped data from a file in the archive

                        ZipArchive archive = new ZipArchive(data);
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.Equals("routes.txt", StringComparison.OrdinalIgnoreCase))
                            {
                                unzippedEntryStream = entry.Open(); // .Open will return a stream
                                                                    // Process entry data here
                                using (StreamReader sr = new StreamReader(unzippedEntryStream))
                                {
                                    if (sr.Peek() != 0)
                                    {
                                        // Read and discard header line
                                        line = sr.ReadLine();
                                    }
                                    while (sr.Peek() != 0)
                                    {
                                        line = sr.ReadLine();
                                        string[] fields = line.Split(",");

                                        Route route = new Route();
                                        
                                        route.id = Guid.NewGuid();
                                        route.route_id = fields[0];
                                        route.agency_id = fields[1];
                                        route.route_short_name = fields[2];
                                        route.route_long_name = fields[3];
                                        route.route_desc = fields[4];
                                        route.route_type = fields[5];   
                                        route.route_url = fields[6];    
                                        route.route_color = fields[7];
                                        route.route_text_color = fields[8];
                                        int route_sort_order;
                                        if (int.TryParse(fields[9], out route_sort_order) && route_sort_order != 0)
                                        {
                                            route.route_sort_order = route_sort_order;
                                        }
                                        else
                                        {
                                            route.route_sort_order = 0;
                                        }
                                        int continuous_pickup;
                                        if (int.TryParse(fields[10], out continuous_pickup) && continuous_pickup != 0)
                                        {
                                            route.continuous_pickup = continuous_pickup;
                                        }
                                        else
                                        {
                                            route.continuous_pickup = 0;
                                        }
                                        int continuous_drop_off;
                                        if (int.TryParse(fields[11], out continuous_drop_off) && continuous_drop_off != 0)
                                        {
                                            route.continuous_drop_off = continuous_drop_off;
                                        }
                                        else
                                        {
                                            route.continuous_drop_off = 0;
                                        }
                                        routes.Add(route);
                                    }
                                }
                            }
                            var xx = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex.Message, new object[0]);
                }
                finally
                {
                    await Task.Delay(86400000, stoppingToken); // Run Daily
                }
            } // end while
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message, new object[0]);
        }
    }
}
