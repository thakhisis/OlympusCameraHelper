using WebWindows.Blazor;
using System;

namespace OlympusCameraHelper
{
    public class Program
    {
        static void Main(string[] args)
        {
            ComponentsDesktop.Run<Startup>("Olympus Camera Helper", "wwwroot/index.html");
        }
    }
}
