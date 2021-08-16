using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;



[assembly: CommandClass(typeof(AutoCADLearning.SearchEntities))]

namespace AutoCADLearning
{
    public class SearchEntities
    {

        [CommandMethod("ListEntities")]
        public static void ListEntities()
        {
            //获取当前数据库，启动事务
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())

            {
                //以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                //以读模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as
                    BlockTableRecord;
                int nCnt = 0;
                acDoc.Editor.WriteMessage("\nModel space objects: ");

                LayerTable layerTable = acTrans.GetObject(acCurDb.LayerTableId,OpenMode.ForRead) as LayerTable;
                 
                //遍历模型空间里的每个对象，并显示找到的对象的类型
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    acDoc.Editor.WriteMessage("\n DXF name: " + acObjId.ObjectClass.DxfName);
                    acDoc.Editor.WriteMessage("\n" + layerTable);
                    nCnt = nCnt + 1;
                }

                //如果没发现对象则显示提示信息
                if (nCnt == 0)
                {
                    acDoc.Editor.WriteMessage("\n No objects found");
                }
                //关闭事务
            }
        }
    }
}