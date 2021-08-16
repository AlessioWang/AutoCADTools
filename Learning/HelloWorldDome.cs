using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;


[assembly: CommandClass(typeof(AutoCADLearning.HelloWorldDemo))]


namespace AutoCADLearning
{
    public class HelloWorldDemo
    {
        // public static void Main(string[] args)
        // {
        //     Console.WriteLine("Hello World!");
        // }


        [CommandMethod("helloWorld", CommandFlags.Session)]
        public void HelloWorld()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n Hello World");
        }


        [CommandMethod("newMethod", CommandFlags.Session)]
        public void Method1()
        {

        }
    }
}