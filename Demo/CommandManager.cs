using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.Runtime;


//程序入口
[assembly: CommandClass(typeof(AutoCADLearning.Demo.CommandManager))]

namespace AutoCADLearning.Demo {
    public class CommandManager {
        public static void Main(string[] args)
        {
            new CommandManager();
        }

        private ParkingDemo parking;

        public CommandManager()
        {
            parking = new ParkingDemo();
        }


        [CommandMethod("SimpleLineParking")]
        public void SimpleLineParking()
        {
            parking.SimpleLineUnit();
        }

        [CommandMethod("MultiLineParking")]
        public void MulitiLineParking()
        {
            parking.MultiLineUnit();
        }

        [CommandMethod("CalculateUnit")]
        public void CalculateUnit()
        {
            parking.CalculateUnits();
        }

        

    }
}