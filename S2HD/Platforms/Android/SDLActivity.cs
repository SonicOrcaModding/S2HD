using Android.App;
using Android.Runtime;

namespace Org.Libsdl.App
{
    [Register("org/libsdl/app/SDLActivity", DoNotGenerateAcw = true)]
    public class SDLActivity : Activity
    {
        protected SDLActivity(System.IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected SDLActivity()
        {
        }
    }
}