using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DeadLetterQueueHelper;
using Stl.Fusion;
using DeadLetterQueueHelper.State;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var fusion = builder.Services.AddFusion(Stl.Rpc.RpcServiceMode.None);
fusion.AddBlazor();

fusion.AddComputedGraphPruner(_ => new() { CheckPeriod = TimeSpan.FromSeconds(30) });
fusion.AddFusionTime();

builder.Services.AddScoped<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.2));

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .RegisterStateServices();

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://servicebus.azure.net/.default");
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

await builder.Build().RunAsync();
