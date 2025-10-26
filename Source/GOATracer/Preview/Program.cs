using GOATracer.Importer.Obj;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace GOATracer.Preview
{
    public static class Program
    {
        public static void Launch(ImportedSceneDescription sceneDescription)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Scene Preview",
                // This is needed to run on macOS
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings, sceneDescription))
            {
                window.Run();
            }
        }
    }
}
