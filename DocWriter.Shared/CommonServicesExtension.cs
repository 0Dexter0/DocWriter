using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DocWriter.Shared;

public static class CommonServicesExtension
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddTransient<IFolderTreeRepository, FolderTreeRepository>();
        services.AddSingleton<IEditorFullScreenModeValueHolder, EditorFullScreenModeValueHolder>();
    }
}