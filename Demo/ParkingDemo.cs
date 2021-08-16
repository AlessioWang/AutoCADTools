using System;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using Color = Autodesk.AutoCAD.Colors.Color;
using System.IO;
using System.Threading;
using AutoCADLearning.BasicTools;


namespace AutoCADLearning.Demo {
    public class ParkingDemo {
        //获取文档和数据库
        public Document doc;
        public Database database;
        public Transaction transaction;

        // 以读模式打开 Block 表
        public BlockTable blockTable;

        // 以写模式打开 Block 表记录 Model 空间
        public BlockTableRecord blockTableRecord;

        public LayerTable layerTable;

        public AutoGeo geoCreator;

        public AutoTools autoTools;


        public void IniAutoCad()
        {
            doc = Application.DocumentManager.MdiActiveDocument;

            database = doc.Database;

            transaction = database.TransactionManager.StartTransaction();

            blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

            blockTableRecord =
                transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
        }

        public static void Main(string[] args)
        {
            new ParkingDemo();
        }


        //停车位尺寸
        private double pWidth = 2500;

        private double pHeight = 5500;

        private Polyline basicUnit;

        public ParkingDemo()
        {
            //创建指定图层
            // LayerController();
        }


        //基础停车位尺寸
        private List<Point2d> GetBasicSize(double w, double h)
        {
            List<Point2d> pts = new List<Point2d>();
            pts.Add(new Point2d(0, 0));
            pts.Add(new Point2d(w, 0));
            pts.Add(new Point2d(w, h));
            pts.Add(new Point2d(0, h));
            return pts;
        }

        // 计算可以放多少个unit
        private int UnitNum(Point3d pStart, Point3d pEnd, double w)
        {
            double dis = pStart.DistanceTo(pEnd);
            return (int) (dis / w);
        }

        private void IniBasicUnit()
        {
            basicUnit = AutoGeo.CreatePolyline(GetBasicSize(pWidth, pHeight));
            basicUnit.Closed = true;
        }

        private List<Polyline> CreateUnitList(Point3d pStart, Point3d pEnd)
        {
            int num = UnitNum(pStart, pEnd, pWidth);
            Vector3d vec = new Vector3d(pWidth, 0, 0);
            List<Polyline> unitList = new List<Polyline>();
            // unitList.Add(basicUnit);
            // Polyline newPolyline = basicUnit.Clone() as Polyline;
            // unitList.Add(newPolyline);

            for (int i = 0; i < num; i++)
            {
                Polyline originUnit = basicUnit.Clone() as Polyline;
                originUnit.TransformBy(Matrix3d.Displacement(vec * i));
                unitList.Add(originUnit);
            }

            return unitList;
        }

        private List<Polyline> MoveUnits(List<Polyline> units, Point3d pStart, Point3d pEnd)
        {
            Vector3d vec = new Point3d(0, 0, 0).GetVectorTo(pStart);

            foreach (Polyline unit in units)
            {
                unit.TransformBy(Matrix3d.Displacement(vec));
            }

            return units;
        }

        //旋转所有unit
        private List<Polyline> RotateUnitList(List<Polyline> unitList, Point3d origin, double angle)
        {
            List<Polyline> units = new List<Polyline>();
            foreach (Polyline unit in unitList)
            {
                unit.TransformBy(Matrix3d.Rotation(angle, new Vector3d(0, 0, 1), origin));
                Polyline unitRo = unit;
                units.Add(unitRo);
            }

            return units;
        }

        private Point3d GetPointFromUser(PromptPointOptions pPointOptions, string message,
            bool ifUseBasePt, Point3d basePoint)
        {
            pPointOptions.Message = message;
            pPointOptions.UseBasePoint = ifUseBasePt;
            pPointOptions.BasePoint = basePoint;
            PromptPointResult ptsResult = doc.Editor.GetPoint(pPointOptions);
            return ptsResult.Value;
        }

        private double GetAngle(Vector3d vec, Vector3d origin)
        {
            if (origin.CrossProduct(vec).Z > 0)
            {
                return origin.GetAngleTo(vec);
            }
            else
            {
                return -origin.GetAngleTo(vec);
            }
        }

        public void LayerController()
        {
            AutoTools.CreateLayer(layerTable, transaction, "BaseLine", 3);
            AutoTools.CreateLayer(layerTable, transaction, "ParkUnit", 5, "Center");
        }

        public void SimpleLineUnit()
        {
            IniAutoCad();

            IniBasicUnit();

            LayerController();

            PromptPointOptions pPointOptions = new PromptPointOptions("");

            Point3d pStart = AutoGeo.GetPointFromUser(pPointOptions, doc, "请输入第一个点：", false, new Point3d(0, 0, 0));
            Point3d pEnd = AutoGeo.GetPointFromUser(pPointOptions, doc, "请输入第二个点：", true, pStart);

            Line3d baseLine = new Line3d(pStart, pEnd);
            Line baseLineDraw = new Line(pStart, pEnd);

            Vector3d vec = pStart.GetVectorTo(pEnd);
            double angle = GetAngle(vec, new Vector3d(1, 0, 0));

            // 基本的unit，排成一行
            List<Polyline> unitInLine = CreateUnitList(pStart, pEnd);
            //平移units到起点位置
            List<Polyline> unitMove = MoveUnits(unitInLine, pStart, pEnd);
            //旋转相应的角度
            List<Polyline> unitRotate = RotateUnitList(unitMove, pStart, angle);


            AutoTools.AddGeo2Layer(layerTable, "ParkUnit", unitRotate.ToArray());
            AutoTools.AddGeo2Layer(layerTable, "BaseLine", baseLineDraw);

            List<Polyline> targetList = unitRotate;

            string ifShow = AutoTools.GetStrWithOpts(doc, "是否镜像", "N", "Y", "N");
            if (ifShow == "Y")
            {
                List<Polyline> lines = new List<Polyline>();
                foreach (var VARIABLE in targetList)
                {
                    lines.Add(AutoGeo.MirrowGeo(VARIABLE, baseLine) as Polyline);
                }
                targetList.Clear();
                targetList = lines;
            }


            AutoTools.AddGeo(blockTableRecord, transaction, targetList.ToArray());
            AutoTools.AddGeo(blockTableRecord, transaction, baseLineDraw);

            transaction.Commit();
        }



        private List<Point3d[]> GetMultiPts(PromptPointOptions pPointOptions, Document doc)
        {
            PromptPointResult pPtRes = doc.Editor.GetPoint(pPointOptions);
            List<Point3d[]> multiPts = new List<Point3d[]>();
            List<Point3d> originPts = new List<Point3d>();


            while (pPtRes.Status == PromptStatus.OK)
            {
                Point3d pt = AutoGeo.GetPointFromUser(pPointOptions, doc, "请输入基点：", false, new Point3d(0, 0, 0));
                originPts.Add(pt);

                AutoTools.AlertCreator("Num ：" + originPts.Count + "个");
                if (pPtRes.Status == PromptStatus.Cancel)
                {
                    return null;
                }
            }

            AutoTools.AlertCreator("Num ：" + originPts.Count + "个");

            multiPts = TransPtsList2Pair(originPts);

            return multiPts;
        }

        private List<Point3d[]> TransPtsList2Pair(List<Point3d> originList)
        {
            int num = originList.Count;
            List<Point3d[]> targetList = new List<Point3d[]>();

            if (num >= 2)
            {
                for (int i = 0; i < num - 1; i++)
                {
                    Point3d[] pair = new Point3d[2];
                    pair[0] = originList[i];
                    pair[1] = originList[i + 1];
                    targetList.Add(pair);
                }
            }

            return targetList;
        }

        public void MultiLineUnit()
        {
            IniAutoCad();

            IniBasicUnit();

            LayerController();

            PromptPointOptions pPointOptions = new PromptPointOptions("");

            int num = GetMultiPts(pPointOptions, doc).Count;

            // AutoTools.AlertCreator("Num ：" + num);

            transaction.Commit();
        }

        public void CalculateUnits()
        {
            IniAutoCad();

            SelectionFilter filter = AutoTools.SetFilter("ParkUnit");
            PromptSelectionResult ssPrompt = doc.Editor.GetSelection(filter);

            if (ssPrompt.Status == PromptStatus.OK)
            {
                SelectionSet selectionSet = ssPrompt.Value;
                int num = selectionSet.Count;
                AutoTools.AlertCreator("车位数 ：" + num);
            }

            transaction.Commit();
        }


    }
}