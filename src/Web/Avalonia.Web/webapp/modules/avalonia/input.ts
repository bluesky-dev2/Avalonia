enum RawInputModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Meta = 8,

    LeftMouseButton = 16,
    RightMouseButton = 32,
    MiddleMouseButton = 64,
    XButton1MouseButton = 128,
    XButton2MouseButton = 256,
    KeyboardMask = Alt | Control | Shift | Meta,

    PenInverted = 512,
    PenEraser = 1024,
    PenBarrelButton = 2048
}

export class InputHelper {
    public static subscribeKeyboardEvents(
        element: HTMLInputElement,
        keyDownCallback: (code: string, key: string, modifiers: RawInputModifiers) => boolean,
        keyUpCallback: (code: string, key: string, modifiers: RawInputModifiers) => boolean,
    ) {
        const keyDownHandler = (args: KeyboardEvent) => {
            if (keyDownCallback(args.code, args.key, this.getModifiers(args))) {
                args.preventDefault();
            }
        };
        element.addEventListener("keydown", keyDownHandler);

        const keyUpHandler = (args: KeyboardEvent) => {
            if (keyUpCallback(args.code, args.key, this.getModifiers(args))) {
                args.preventDefault();
            }
        };
        element.addEventListener("keyup", keyUpHandler);

        return () => {
            element.removeEventListener("keydown", keyDownHandler);
            element.removeEventListener("keyup", keyUpHandler);
        };
    }

    public static subscribePointerEvents(
        element: HTMLInputElement,
        pointerMoveCallback: (args: PointerEvent) => boolean,
        pointerDownCallback: (args: PointerEvent) => boolean,
        pointerUpCallback: (args: PointerEvent) => boolean,
        wheelCallback: (args: WheelEvent) => boolean,
    ) {
        const pointerMoveHandler = (args: PointerEvent) => {
            
            if (pointerMoveCallback(args)) {
                args.preventDefault();
            }
        };

        const pointerDownHandler = (args: PointerEvent) => {

            if (pointerDownCallback(args)) {
                args.preventDefault();
            }
        };

        const pointerUpHandler = (args: PointerEvent) => {

            if (pointerUpCallback(args)) {
                args.preventDefault();
            }
        };

        const wheelHandler = (args: WheelEvent) => {

            if (wheelCallback(args)) {
                args.preventDefault();
            }
        };

        element.addEventListener("pointermove", pointerMoveHandler);
        element.addEventListener("pointerdown", pointerDownHandler);
        element.addEventListener("pointerup", pointerUpHandler);
        element.addEventListener("wheel", wheelHandler);

       

        return () => {
            element.removeEventListener("pointerover", pointerMoveHandler);
            element.removeEventListener("pointerdown", pointerDownHandler);
            element.removeEventListener("pointerup", pointerUpHandler);
            element.removeEventListener("wheel", wheelHandler);
        };
    }
    
    public static subscribeInputEvents(
        element: HTMLInputElement,
        inputCallback: (value: string) => boolean
    ) {
        const inputHandler = (args: Event) => {
            if (inputCallback((args as any).value)) {
                args.preventDefault();
            }
        };
        element.addEventListener("input", inputHandler);

        return () => {
            element.removeEventListener("input", inputHandler);
        };
    }

    public static clearInput(inputElement: HTMLInputElement) {
        inputElement.value = "";
    }

    public static isInputElement(element: HTMLInputElement | HTMLElement): element is HTMLInputElement {
        return (element as HTMLInputElement).setSelectionRange !== undefined;
    }

    public static focusElement(inputElement: HTMLElement) {
        inputElement.focus();

        if (this.isInputElement(inputElement)) {
            (inputElement as HTMLInputElement).setSelectionRange(0, 0);
        }
    }

    public static setCursor(inputElement: HTMLInputElement, kind: string) {
        inputElement.style.cursor = kind;
    }

    public static hide(inputElement: HTMLInputElement) {
        inputElement.style.display = 'none';
    }

    public static show(inputElement: HTMLInputElement) {
        inputElement.style.display = 'block';
    }

    private static getModifiers(args: KeyboardEvent): RawInputModifiers {
        var modifiers = RawInputModifiers.None;

        if (args.ctrlKey)
            modifiers |= RawInputModifiers.Control;
        if (args.altKey)
            modifiers |= RawInputModifiers.Alt;
        if (args.shiftKey)
            modifiers |= RawInputModifiers.Shift;
        if (args.metaKey)
            modifiers |= RawInputModifiers.Meta;

        return modifiers;
    }
}
