using Avalonia.Metadata;

namespace Avalonia.Input.TextInput
{
    [Unstable]
    public interface ITextInputMethodImpl
    {
        ITextInputMethodClient? Client { get; }

        bool IsActive { get; }

        void SetClient(ITextInputMethodClient? client);
        void SetCursorRect(Rect rect);
        void SetOptions(TextInputOptions options);
        void Reset();
    }
    
    [NotClientImplementable]
    public interface ITextInputMethodRoot : IInputRoot
    {
        ITextInputMethodImpl? InputMethod { get; }
    }
}
