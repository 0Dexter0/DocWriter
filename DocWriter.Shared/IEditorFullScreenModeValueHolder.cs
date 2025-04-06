namespace DocWriter.Shared;

public interface IEditorFullScreenModeValueHolder
{
    event EventHandler<bool> IsFullScreenChanged;

    void InvokeFullScreenChanged(bool isFullScreen);
}