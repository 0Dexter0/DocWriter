using DocWriter.Client.StateContainers;
using DocWriter.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddCommonServices();
builder.Services.AddSingleton<IHomeStateContainer, HomeStateContainer>();
builder.Services.AddSingleton<IPresentationStateContainer, PresentationStateContainer>();

await builder.Build().RunAsync();