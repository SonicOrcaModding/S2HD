package org.libsdl.app;

public class SDLAudioManager {
    public static native int nativeSetupJNI();
    public static native void removeAudioDevice(boolean isCapture, int deviceId);
    public static native void addAudioDevice(boolean isCapture, int deviceId);
}

