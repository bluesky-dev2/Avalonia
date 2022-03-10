namespace Avalonia.Input.TextInput
{
    public interface ITextInputMethodImpl
    {
        void SetActive(ITextInputMethodClient? client);
        void SetCursorRect(Rect rect);
        void SetOptions(TextInputOptions options);
        void Reset();
    }
    
    public interface ITextInputMethodRoot : IInputRoot
    {
        ITextInputMethodImpl? InputMethod { get; }
    }
}
