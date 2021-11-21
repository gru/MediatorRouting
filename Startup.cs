using System;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorRouting
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetEntryAssembly());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                var routes = Assembly
                    .GetEntryAssembly()
                    .GetExportedTypes()
                    .Where(t => t.IsAssignableToGenericType(typeof(IRequestHandler<,>)))
                    .Select(t => new
                    {
                        Handler = t,
                        Request = t.BaseType?
                            .GetGenericArguments()
                            .First(),
                        Route = t.GetCustomAttributes()
                            .OfType<RouteAttribute>()
                            .SingleOrDefault()
                    });
                
                foreach (var route in routes)
                {
                    if (route.Route == null) continue;

                    endpoints.MapPost(route.Route.Template, async context =>
                    {
                        var mediator = context.RequestServices
                            .GetRequiredService<IMediator>();
                        var body = await context.Request
                            .ReadFromJsonAsync(route.Request);

                        var response = await mediator.Send(body);
                        
                        await context.Response
                            .WriteAsJsonAsync(response);
                    });
                }
            });
        }
    }

    internal static class TypeEx
    {
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}