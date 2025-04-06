namespace DocWriter.Shared;

internal class EditorFullScreenModeValueHolder : IEditorFullScreenModeValueHolder
{
    public event EventHandler<bool> IsFullScreenChanged;

    public void InvokeFullScreenChanged(bool isFullScreen)
    {
        IsFullScreenChanged?.Invoke(this, isFullScreen);
    }
}