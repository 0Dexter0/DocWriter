using DocWriter.Shared.Repositories;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DocWriter.Client.Components;

public partial class Preview : ComponentBase
{
    private string _content = string.Empty;
    private bool _showProgressBar;

    [Inject]
    private IFileContentRepository FileContentRepository { get; init; }

    [Inject]
    private IJSRuntime JsRuntime { get; init; }

    [Parameter]
    public string FilePath { get; init; }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            _content = string.Empty;

            return;
        }

        _showProgressBar = true;
        string raw = await FileContentRepository.GetFileContentAsync(FilePath);

        _content = Markdown.ToHtml(
            raw,
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseDiagrams()
                .Build());

        _showProgressBar = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_content is not "")
        {
            IJSObjectReference mermaidModule =
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");
            await mermaidModule.InvokeVoidAsync("Initialize");
        }
    }
}