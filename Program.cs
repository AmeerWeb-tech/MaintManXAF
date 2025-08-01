﻿using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.DesignTime;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.Utils;
using Microsoft.Extensions.DependencyInjection;
using MaintManXAF.Module.Services.Equipment;
using MaintManXAF.Module.Services.Category;
using MaintManXAF.Module.Services.Group;
using MaintManXAF.Module.Services.Location;
using MaintManXAF.Module.Services.Workstation;
using MaintManXAF.Module.Services.Tool;
using MaintManXAF.Module.Services.Priority;
using MaintManXAF.Module.Services.DowntimeGroup;
using MaintManXAF.Module.Services.MaintenanceGroup;
using MaintManXAF.Module.Services.Cost;

namespace MaintManXAF.Blazor.Server;

public class Program : IDesignTimeApplicationFactory {
    private static bool ContainsArgument(string[] args, string argument) {
        return args.Any(arg => arg.TrimStart('/').TrimStart('-').ToLower() == argument.ToLower());
    }
    public static int Main(string[] args) {
        if(ContainsArgument(args, "help") || ContainsArgument(args, "h")) {
            Console.WriteLine("Updates the database when its version does not match the application's version.");
            Console.WriteLine();
            Console.WriteLine($"    {Assembly.GetExecutingAssembly().GetName().Name}.exe --updateDatabase [--forceUpdate --silent]");
            Console.WriteLine();
            Console.WriteLine("--forceUpdate - Marks that the database must be updated whether its version matches the application's version or not.");
            Console.WriteLine("--silent - Marks that database update proceeds automatically and does not require any interaction with the user.");
            Console.WriteLine();
            Console.WriteLine($"Exit codes: 0 - {DBUpdaterStatus.UpdateCompleted}");
            Console.WriteLine($"            1 - {DBUpdaterStatus.UpdateError}");
            Console.WriteLine($"            2 - {DBUpdaterStatus.UpdateNotNeeded}");
        }
        else {
            DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
            DevExpress.ExpressApp.Security.SecurityStrategy.AutoAssociationReferencePropertyMode = DevExpress.ExpressApp.Security.ReferenceWithoutAssociationPermissionsMode.AllMembers;
            IHost host = CreateHostBuilder(args).Build();
            if(ContainsArgument(args, "updateDatabase")) {
                using(var serviceScope = host.Services.CreateScope()) {
                    return serviceScope.ServiceProvider.GetRequiredService<DevExpress.ExpressApp.Utils.IDBUpdater>().Update(ContainsArgument(args, "forceUpdate"), ContainsArgument(args, "silent"));
                }
            }
            else {
                host.Run();
            }
        }
        return 0;
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices((hostContext, services) => {
                // Register all services
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IGroupService, GroupService>();
                services.AddScoped<ILocationService, LocationService>();
                services.AddScoped<IWorkstationService, WorkstationService>();
                services.AddScoped<IToolService, ToolService>();
                services.AddScoped<IPriorityService, PriorityService>();
                services.AddScoped<IDowntimeGroupService, DowntimeGroupService>();
                services.AddScoped<IMaintenanceGroupService, MaintenanceGroupService>();
                services.AddScoped<ICostService, CostService>();
                services.AddScoped<IEquipmentService, EquipmentService>();
            });
    XafApplication IDesignTimeApplicationFactory.Create() {
        IHostBuilder hostBuilder = CreateHostBuilder(Array.Empty<string>());
        return DesignTimeApplicationFactoryHelper.Create(hostBuilder);
    }
}
