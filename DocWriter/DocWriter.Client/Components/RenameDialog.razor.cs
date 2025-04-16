using DocWriter.Shared.Models;
using DocWriter.Shared.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace DocWriter.Client.Components;

public partial class RenameDialog : ComponentBase
{
    private string _updated = String.Empty;
    private bool _disabled;

    [Inject]
    private ISnackbar Snackbar { get; init; }

    [Inject]
    private IFolderTreeRepository FolderTreeRepository { get; init; }

    [CascadingParameter]
    private IMudDialogInstance Dialog { get; init; }

    [Parameter]
    public string Type { get; init; }

    [Parameter]
    public TreeItemData<FolderTreeItem> Item { get; set; }

    protected override void OnParametersSet()
    {
        _updated = Item.Text;
    }

    private void Cancel() => Dialog.Cancel();

    private async Task RenameAsync()
    {
        if (_updated == Item.Text)
        {
            return;
        }

        var result = await FolderTreeRepository.UpdateFolderTreeItemAsync(new(_updated, Item.Value!.Name, Item.Value.Path));

        if (result == null)
        {
            Snackbar.Add("Unable to rename", Severity.Error);
            Dialog.Close();

            return;
        }

        Item.Text = _updated;
        Snackbar.Add("Successfully renamed", Severity.Success);

        Dialog.Close();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs arg)
    {
        if (arg.Key is "Enter" or "NumpadEnter")
        {
            await RenameAsync();
        }
    }

    private void StillDisabled()
    {
        _disabled = _updated == Item.Text || string.IsNullOrWhiteSpace(_updated);
    }
}