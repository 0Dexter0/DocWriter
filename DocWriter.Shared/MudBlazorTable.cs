using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace DocWriter.Shared;

public class MudBlazorTable : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
        pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private static void PipelineOnDocumentProcessed(MarkdownDocument document)
    {
        foreach (var node in document.Descendants().OfType<Table>())
        {
            node.GetAttributes().AddClass("mud-table w-100");

            foreach (var child in node)
            {
                if (child is TableRow row)
                {
                    row.GetAttributes().AddClass("mud-table-row");

                    foreach (var rowChild in row)
                    {
                        if (rowChild is TableCell cell)
                        {
                            cell.GetAttributes().AddClass("mud-table-cell");
                        }
                    }
                }
            }
        }
    }
}

public static class MudBlazorTableExtensions
{
    public static MarkdownPipelineBuilder UseMudBlazorTable(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.AddIfNotAlready<MudBlazorTable>();

        return pipeline;
    }
} 