
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TransitAPI.Models;

namespace TransitAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration["Transit:ConnectionString"];

            var contextOptions = new DbContextOptionsBuilder<TransitContext>()
                .UseSqlServer(connectionString).Options;

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy(MyAllowSpecificOrigins,
            //                          policy =>
            //                          {
            //                              policy.AllowAnyHeader()
            //                                    .AllowAnyMethod()
            //                                    .AllowAnyOrigin();
            //                          });
            //});

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Using reverse proxy
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/entities", (HttpContext httpContext) =>
            {
                List<Entity> entities = new List<Entity>();
                var w = httpContext.Request.Query["bounds"];

                if (w.Count > 0)
                {
                    var bounds = JsonSerializer.Deserialize<Bounds>(w);

                    if (bounds != null)
                    {
                        using (var x = new TransitContext(contextOptions))
                        {
                            var y = x.GetProcedures();
                            // https://localhost:7239/entities?bounds={%22South%22:43.5,%22West%22:-79.6,%22North%22:43.6,%22East%22:-79.5}
                            //var z = y.GetRelevantEntitiesAsync(43.6, -79.5, 43.5, -79.6).GetAwaiter().GetResult();
                            entities = y.GetRelevantEntitiesAsync(bounds.North, bounds.East, bounds.South, bounds.West)
                                .GetAwaiter().GetResult();
                        }
                    }
                    return entities;
                }
                else
                {
                    return entities;
                }
            })
            .WithName("GetEntities")
            .WithOpenApi();

            app.Run();
        }
    }
}