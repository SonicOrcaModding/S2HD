package org.libsdl.app;

public class SDLControllerManager {
    public static native int nativeSetupJNI();
    public static native int nativeAddJoystick(int device_id, String name, String desc,
                                               int vendor_id, int product_id,
                                               boolean is_accelerometer, int button_mask,
                                               int naxes, int axis_mask, int nhats, int nballs);
    public static native int nativeRemoveJoystick(int device_id);
    public static native int nativeAddHaptic(int device_id, String name);
    public static native int nativeRemoveHaptic(int device_id);
    public static native int onNativePadDown(int device_id, int keycode);
    public static native int onNativePadUp(int device_id, int keycode);
    public static native void onNativeJoy(int device_id, int axis, float value);
    public static native void onNativeHat(int device_id, int hat_id, int x, int y);
}

