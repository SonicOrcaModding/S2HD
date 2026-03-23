package org.libsdl.app;

public class SDLInputConnection {
    public static native void nativeCommitText(String text, int newCursorPosition);
    public static native void nativeGenerateScancodeForUnichar(char c);
}