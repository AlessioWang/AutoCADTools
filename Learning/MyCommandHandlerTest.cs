using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;


namespace AutoCADLearning
{
    class MyCommandHandlerTest : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Application.ShowAlertDialog("点击按键");
        }
    }
}

    class Chp
    {
        private ApplicationMenuItem applicationMenuItem = null;

        [CommandMethod("AddZeroDocEvent")]
        public void AddEvent()
        {
            DocumentCollection docMgr = Application.DocumentManager;
            
        }

    }
