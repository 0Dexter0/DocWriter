using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace DocWriter.Client.Pages;

public partial class Home
{
    private string _markdownValue;

    private string HtmlContent { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [JSInvokable]
    public void StateHasChangedFromJs()
    {
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        _markdownValue =
            "```mermaid\nstateDiagram\n    [*] --> Still\n    Still --> [*]\n\n    Still --> Moving\n    Moving --> Still\n    Moving --> Crash\n    Crash --> [*]\n```";
        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
        NavigateToElement();
    }

    private void NavigationManagerOnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        NavigateToElement();
    }

    private void NavigateToElement()
    {
        var url = NavigationManager.Uri;
        var fragment = new Uri(url).Fragment;

        if (string.IsNullOrEmpty(fragment))
        {
            return;
        }

        var elementId = fragment.StartsWith("#") ? fragment.Substring(1) : fragment;

        if (string.IsNullOrEmpty(elementId))
        {
            return;
        }

        ScrollToElementId(elementId);
    }

    private Task OnMarkdownValueChangedAsync(string value)
    {
        _markdownValue = value;
        return Task.CompletedTask;
    }

    private Task OnMarkdownValueHTMLChanged(string value)
    {
        HtmlContent = value;
        return Task.CompletedTask;
    }

    private void ScrollToElementId(object elementId)
    {
        JsRuntime.InvokeVoidAsync("scrollToElementId", elementId).GetAwaiter().GetResult();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IJSObjectReference mermaidModule;
        if(firstRender)
        {
            mermaidModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");
            await mermaidModule.InvokeVoidAsync("Initialize");
            await mermaidModule.InvokeVoidAsync("updateState", DotNetObjectReference.Create(this));
            // await _mermaidModule.InvokeVoidAsync("Render", "mermaid");
        }
        else
        {
            mermaidModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");
            await mermaidModule.InvokeVoidAsync("RenderAll");
        }
    }
}