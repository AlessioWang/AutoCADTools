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
using AutoCADLearning.BasicTools;
using Autodesk.Private.Windows;

[assembly: CommandClass(typeof(AutoCADLearning.MainTest))]


namespace AutoCADLearning {
    public class MainTest {
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

        public static void Main(string[] args)
        {
            new MainTest();
        }

        public MainTest()
        {
            // doc = Application.DocumentManager.MdiActiveDocument;
            //
            // database = doc.Database;
            //
            // transaction = database.TransactionManager.StartTransaction();
            //
            // blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
            //
            // blockTableRecord =
            //     transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //
            // layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
            //
            // geoCreator = new GeoCreator(blockTableRecord, transaction);
        }

        public void IniAutoCad()
        {
            doc = Application.DocumentManager.MdiActiveDocument;

            database = doc.Database;

            transaction = database.TransactionManager.StartTransaction();

            blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

            blockTableRecord =
                transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

            geoCreator = new AutoGeo(blockTableRecord, layerTable, transaction);
        }

        [CommandMethod("AddGeo")]
        public void addCir()
        {
            createGeo();
        }

        [CommandMethod("layerOp")]
        public void layerFrozen()
        {
            layerOP();
        }

        public void layerOP()
        {
            IniAutoCad();
            foreach (ObjectId objectId in layerTable)
            {
                LayerTableRecord layerRecord = transaction.GetObject(objectId, OpenMode.ForRead) as LayerTableRecord;

                if (layerRecord.Name.Equals("hello", StringComparison.OrdinalIgnoreCase))
                {
                    if (layerRecord.ObjectId != database.Clayer)
                    {
                        layerRecord.UpgradeOpen();
                        layerRecord.IsFrozen = true;
                        layerRecord.DowngradeOpen();
                    }
                }
            }

            // transaction.Commit();
        }


        [CommandMethod("createGeo")]
        public void createGeo()
        {
            IniAutoCad();
            Circle circle = AutoGeo.CreateCircle(new Point3d(50, 70, 20), 100);

            Line line1 = AutoGeo.CreateLine(new Point3d(30, 50, 50), new Point3d(100, 70, 0));

            List<Point2d> pts = new List<Point2d>();
            pts.Add(new Point2d(20, 20));
            pts.Add(new Point2d(50, 30));
            pts.Add(new Point2d(80, 100));
            pts.Add(new Point2d(60, 180));
            pts.Add(new Point2d(90, 30));

            Polyline polyline = AutoGeo.CreatePolyline(pts);
            polyline.Closed = true;

            DBPoint dbPoint = AutoGeo.CreatePoint(new Point3d(20, 20, 0));
            database.Pdmode = 35;
            database.Pdsize = 3;


            geoCreator.AddGeo(circle, line1, polyline, dbPoint);

            //将新的geo保存到database
            transaction.Commit();
        }

        [CommandMethod("createRegion")]
        public void RegionTest()
        {
            IniAutoCad();

            DBObjectCollection originCollection = new DBObjectCollection();
            originCollection.Add(AutoGeo.CreateCircle(new Point3d(20, 30, 0), 100));
            originCollection.Add(AutoGeo.CreateCircle(new Point3d(50, 70, 0), 200));

            DBObjectCollection targetCollection = new DBObjectCollection();
            targetCollection = Region.CreateFromCurves(originCollection);

            Region region1 = targetCollection[0] as Region;
            Region region2 = targetCollection[1] as Region;

            if (region1.Area > region2.Area)
            {
                region1.BooleanOperation(BooleanOperationType.BoolSubtract, region2);
                region2.Dispose();
                geoCreator.AddGeo(region1);
            }
            else
            {
                region2.BooleanOperation(BooleanOperationType.BoolSubtract, region1);
                region1.Dispose();
                geoCreator.AddGeo(region2);
            }

            transaction.Commit();
        }

        [CommandMethod("createHatch")]
        public void HatchCreate()
        {
            IniAutoCad();

            Circle circle = AutoGeo.CreateCircle(new Point3d(0, 0, 0), 50);
            geoCreator.AddGeo(circle);

            ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            objectIdCollection.Add(circle.ObjectId);

            Hatch hatch = new Hatch();
            geoCreator.AddGeo(hatch);

            hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            hatch.Associative = true;
            hatch.AppendLoop(HatchLoopTypes.Outermost, objectIdCollection);
            hatch.EvaluateHatch(true);

            transaction.Commit();
        }

        [CommandMethod("CheckPickFirst")]
        public void CheckPickFirst()
        {
            IniAutoCad();

            // 获取当前文档
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

            // 获取PickFirst选择集
            PromptSelectionResult ssPrompt = editor.SelectImplied();

            SelectionSet ssSet;
            //
            // if (ssPrompt.Status == PromptStatus.OK)
            // {
            //     ssSet = ssPrompt.Value;
            //
            //     Application.ShowAlertDialog("Number of selection : " + ssSet.Count.ToString());
            // }
            // else
            // {
            //     Application.ShowAlertDialog("Number of selection: 0" );
            // }

            // 清空PickFirst选择集
            ObjectId[] idarrayEmpty = new ObjectId[0];

            editor.SetImpliedSelection(idarrayEmpty);

            ssPrompt = editor.GetSelection();
            if (ssPrompt.Status == PromptStatus.OK)
            {
                ssSet = ssPrompt.Value;
                Application.ShowAlertDialog("Number of selection : " + ssSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of selection : 0000");
            }

            transaction.Commit();
        }

        [CommandMethod("SelectObjOnScreen")]
        public void SelObjOnScreen()
        {
            IniAutoCad();

            //设置过滤器
            //过滤器只能识别到物体的属性，父图层的属性识别不到
            TypedValue[] typleValues = new TypedValue[2];
            typleValues.SetValue(new TypedValue((int) DxfCode.Color, 5), 0);
            typleValues.SetValue(new TypedValue((int) DxfCode.LayerName, "0"), 0);
            typleValues.SetValue(new TypedValue((int) DxfCode.Start, "CIRCLE"), 1);

            SelectionFilter filter = new SelectionFilter(typleValues);

            // 请求在图形区域选择对象
            // PromptSelectionResult ssPrompt = doc.Editor.SelectWindow(new Point3d(0,0,0),new Point3d(100,100,0));
            // PromptSelectionResult ssPrompt = doc.Editor.SelectAll();
            PromptSelectionResult ssPrompt = doc.Editor.GetSelection(filter);

            // PromptSelectionResult ssPrompt = doc.Editor.GetPoint("1");


            if (ssPrompt.Status == PromptStatus.OK)
            {
                SelectionSet selectionSet = ssPrompt.Value;

                // 遍历选择集内的对象
                foreach (SelectedObject obj in selectionSet)
                {
                    if (obj != null)
                    {
                        // 确认返回的是合法的SelectedObject对象
                        Entity entity = transaction.GetObject(obj.ObjectId, OpenMode.ForWrite) as Entity;
                        if (entity != null)
                        {
                            entity.ColorIndex = 3;
                        }
                    }
                }

                transaction.Commit();
            }
        }

        [CommandMethod("SelectObjInList")]
        public void SelObjInList()
        {
            IniAutoCad();

            // 请求在图形区域选择对象
            PromptSelectionResult ssPrompt = doc.Editor.GetSelection();

            SelectionSet selectionSet;

            ObjectIdCollection objIdCollection = new ObjectIdCollection();

            if (ssPrompt.Status == PromptStatus.OK)
            {
                selectionSet = ssPrompt.Value;
                objIdCollection = new ObjectIdCollection(selectionSet.GetObjectIds());
            }

            ssPrompt = doc.Editor.GetSelection();

            SelectionSet selectionSet2;

            if (ssPrompt.Status == PromptStatus.OK)
            {
                selectionSet2 = ssPrompt.Value;

                if (objIdCollection.Count == 0)
                {
                    objIdCollection = new ObjectIdCollection(selectionSet2.GetObjectIds());
                }
                else
                {
                    foreach (ObjectId id in selectionSet2.GetObjectIds())
                    {
                        objIdCollection.Add(id);
                    }
                }
            }

            Application.ShowAlertDialog("Number = " + objIdCollection.Count.ToString());

            transaction.Commit();
        }

        [CommandMethod("transMove")]
        public void TransMove()
        {
            IniAutoCad();

            Line line = AutoGeo.CreateLine(new Point3d(0, 0, 0), new Point3d(20, 0, 0));

            Vector3d vec = new Point3d(0, 0, 0).GetVectorTo(new Point3d(0, 20, 0));

            line.TransformBy(Matrix3d.Displacement(vec));

            geoCreator.AddGeo(line);

            transaction.Commit();
        }

        [CommandMethod("transRotate")]
        public void transRotate()
        {
            IniAutoCad();

            Line line = AutoGeo.CreateLine(new Point3d(0, 0, 0), new Point3d(20, 0, 0));

            line.TransformBy(Matrix3d.Rotation(Math.PI * 0.25, new Vector3d(0, 0, 1), new Point3d(0, 0, 0)));

            geoCreator.AddGeo(line);

            transaction.Commit();
        }

        [CommandMethod("layerName")]
        public void layerName()
        {
            IniAutoCad();

            string layerName = "";

            foreach (ObjectId id in layerTable)
            {
                LayerTableRecord layerTableRecord = transaction.GetObject(id, OpenMode.ForRead) as LayerTableRecord;

                layerName = layerName + "\n" + layerTableRecord.Name;
            }

            Application.ShowAlertDialog("layers : " + layerName);

            transaction.Commit();
        }

        [CommandMethod("layerAdd")]
        public void layerAdd()
        {
            IniAutoCad();

            string name = "Inst.AAA";

            //创建新图层
            if (layerTable.Has(name) == false)
            {
                LayerTableRecord layerRecord = new LayerTableRecord();

                LinetypeTable linetypeTable =
                    transaction.GetObject(database.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                layerTable.UpgradeOpen();

                layerRecord.Name = name;
                layerRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                layerRecord.LineWeight = LineWeight.LineWeight005;

                if (!linetypeTable.Has("Center"))
                {
                    database.LoadLineTypeFile("Center", "acad.lin");
                }

                if (linetypeTable.Has("Center"))
                {
                    layerRecord.LinetypeObjectId = linetypeTable["Center"];
                }

                layerTable.Add(layerRecord);
                transaction.AddNewlyCreatedDBObject(layerRecord, true);
            }

            Circle circle = AutoGeo.CreateCircle(new Point3d(20, 20, 20), 100);
            circle.Layer = name;
            circle.LinetypeScale = 20;

            geoCreator.AddGeo(circle);

            transaction.Commit();
        }

        [CommandMethod("CreateDim")]
        public void CreateRotateDim()
        {
            IniAutoCad();

            AlignedDimension alignedDimension = new AlignedDimension
            {
                XLine1Point = new Point3d(100, 100, 0),
                XLine2Point = new Point3d(200, 50, 0),
                // 控制线标注距离标注的距离
                DimLinePoint = new Point3d(100, 0, 0),
                //设置样式（读取database里面的样式）
                DimensionStyle = database.Dimstyle
            };

            RotatedDimension rotatedDim = new RotatedDimension
            {
                XLine1Point = new Point3d(0, 0, 0),
                XLine2Point = new Point3d(100, 100, 0),
                Rotation = Math.PI * 0.25,
                DimLinePoint = new Point3d(0, 150, 0),
                DimensionStyle = database.Dimstyle
            };

            RadialDimension radialDimension = new RadialDimension
            {
                Center = new Point3d(0, 0, 0),
                ChordPoint = new Point3d(50, 50, 0),
                //引出线长度
                LeaderLength = 20,
                DimensionStyle = database.Dimstyle
            };

            //<> 代表原本数值的位置
            radialDimension.DimensionText = "Rad : <>";

            geoCreator.AddGeo(rotatedDim);
            geoCreator.AddGeo(alignedDimension);
            geoCreator.AddGeo(radialDimension);

            transaction.Commit();
        }

        [CommandMethod("createUserDim")]
        public void CreateUserDim()
        {
            IniAutoCad();

            AlignedDimension alignedDimension = new AlignedDimension
            {
                XLine1Point = new Point3d(100, 100, 0),
                XLine2Point = new Point3d(200, 50, 0),
                // 控制线标注距离标注的距离
                DimLinePoint = new Point3d(100, 0, 0),
                //设置样式（读取database里面的样式）
                DimensionStyle = database.Dimstyle
            };

            geoCreator.AddGeo(alignedDimension);

            PromptStringOptions pStrOptions = new PromptStringOptions("");
            pStrOptions.Message = "输入后缀 ：";
            pStrOptions.AllowSpaces = true;
            PromptResult pStrRes = doc.Editor.GetString(pStrOptions);

            if (pStrRes.Status == PromptStatus.OK)
            {
                alignedDimension.Suffix = pStrRes.StringResult;
            }

            transaction.Commit();
        }

        [CommandMethod("CreateLeader")]
        public void CreateLeader()
        {
            IniAutoCad();

            Leader leader = new Leader();
            leader.AppendVertex(new Point3d(0, 0, 0));
            leader.AppendVertex(new Point3d(50, 50, 0));
            leader.AppendVertex(new Point3d(70, 50, 0));
            leader.AppendVertex(new Point3d(70, 70, 0));
            leader.HasArrowHead = true;

            geoCreator.AddGeo(leader);
            transaction.Commit();
        }

        [CommandMethod("CreateLeaderByUser")]
        public void CreateLeaderByUser()
        {
            IniAutoCad();

            PromptPointOptions pPointOptions = new PromptPointOptions("指定点 : ");
            pPointOptions.AllowNone = false;

            // PromptResult pResult = doc.Editor.GetPoint();

            transaction.Commit();
        }


        [CommandMethod("CreateLeaderText")]
        public void CreateLeaderText()
        {
            IniAutoCad();

            MText mText = new MText();
            mText.Contents = "Inst.AAA";
            mText.Location = new Point3d(50, 0, 0);
            mText.Width = 10;
            geoCreator.AddGeo(mText);


            Leader leader = new Leader();
            leader.AppendVertex(new Point3d(0, 0, 0));
            leader.AppendVertex(new Point3d(50, 0, 0));
            leader.HasArrowHead = true;

            geoCreator.AddGeo(leader);

            leader.Annotation = mText.ObjectId;
            leader.EvaluateLeader();

            transaction.Commit();
        }

        [CommandMethod("CreateVol")]
        public void CreateVol()
        {
            IniAutoCad();

            Solid3d solid3d = new Solid3d();
            solid3d.CreateBox(10, 20, 30);

            solid3d.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 5) - Point3d.Origin));

            geoCreator.AddGeo(solid3d);

            ViewportTableRecord viewportTableRecord =
                transaction.GetObject(doc.Editor.ActiveViewportId, OpenMode.ForWrite) as ViewportTableRecord;

            viewportTableRecord.ViewDirection = new Vector3d(-1, 1, 1);
            doc.Editor.UpdateTiledViewportsFromDatabase();

            transaction.Commit();
        }

        [CommandMethod("ResizeWindow")]
        public void ResizeWin()
        {
            Application.MainWindow.SetLocation(new Point(200, 200));

            Size size = new Size(1000, 1000);
            Application.MainWindow.SetSize(size);
        }

        [CommandMethod("hideWindowState")]
        public void WindowState()
        {
            Application.MainWindow.Visible = false;

            System.Windows.Forms.MessageBox.Show("invisible", "show/hide");

            Application.MainWindow.Visible = true;
            System.Windows.Forms.MessageBox.Show("visible", "show/hide");
        }

        [CommandMethod("OpenFile")]
        public void OpenFile()
        {
            DocumentCollection docMgr = Application.DocumentManager;
            string filename = "C:\\test.dwg";

            if (File.Exists(filename))
            {
                docMgr.Open(filename);
            }
            else
            {
                docMgr.MdiActiveDocument.Editor.WriteMessage("File" + filename + "does not exist");
            }
        }

        [CommandMethod("GetStringFromUser")]
        public void GetStringFromUser()
        {
            IniAutoCad();

            PromptStringOptions pStrOpts = new PromptStringOptions("\n Enter your name: ");

            //允许输入空格
            pStrOpts.AllowSpaces = true;
            PromptResult pStrRes = doc.Editor.GetString(pStrOpts);

            Application.ShowAlertDialog("Name :" + pStrRes.StringResult);

            transaction.Commit();
        }

        [CommandMethod("GetKeywordsWithOptions")]
        public void GetStrWithOpts()
        {
            IniAutoCad();

            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\n Select options");
            pKeyOpts.Keywords.Add("Line");
            pKeyOpts.Keywords.Add("Arc");
            pKeyOpts.Keywords.Add("PolyLine");
            pKeyOpts.Keywords.Default = "PolyLine";

            PromptResult pResult = doc.Editor.GetKeywords(pKeyOpts);

            Application.ShowAlertDialog("type" + pResult.StringResult);

            transaction.Commit();
        }

        [CommandMethod("SendCommand2CAD")]
        public void SendCom()
        {
            IniAutoCad();

            doc.SendStringToExecute("._circle 20,20,0 40 ", true, false, false);
            doc.SendStringToExecute("._zoom _all ", true, false, false);

            transaction.Commit();
        }

        [CommandMethod("GetPointFromUser")]
        public void GetPointFromUser()
        {
            IniAutoCad();

            PromptPointResult ptsResult;

            PromptPointOptions pPointOptions = new PromptPointOptions("请指定起点 : ");
            ptsResult = doc.Editor.GetPoint(pPointOptions);

            Point3d ptStart = ptsResult.Value;

            //用户按取消或者esc
            if (ptsResult.Status == PromptStatus.Cancel)
                return;

            pPointOptions.Message = "指定第二点";
            pPointOptions.UseBasePoint = true;
            pPointOptions.BasePoint = ptStart;
            ptsResult = doc.Editor.GetPoint(pPointOptions);
            Point3d ptEnd = ptsResult.Value;

            //用户按取消或者esc
            if (ptsResult.Status == PromptStatus.Cancel)
                return;

            pPointOptions.Message = "请指定终点";
            pPointOptions.UseBasePoint = true;
            pPointOptions.BasePoint = ptEnd;
            ptsResult = doc.Editor.GetPoint(pPointOptions);
            Point3d ptEnds = ptsResult.Value;


            Leader leader = new Leader();
            leader.AppendVertex(ptStart);
            leader.AppendVertex(ptEnd);
            leader.AppendVertex(ptEnds);

            //控制箭头的大小
            leader.Dimasz = 70;
            leader.HasArrowHead = true;

            Leader leader1 = new Leader();
            leader1.AppendVertex(ptStart);
            leader1.AppendVertex(ptEnd);
            leader1.AppendVertex(ptEnds);

            //控制箭头的大小
            leader1.Dimasz = 100;
            leader1.HasArrowHead = true;

            geoCreator.AddGeo(leader, leader1);

            doc.SendStringToExecute("._zoom _all ", true, false, false);

            transaction.Commit();
        }

        [CommandMethod("TestGetPointFromUser")]
        public void TestGetVertices()
        {
            IniAutoCad();


            PromptEntityOptions pEntityOptions = new PromptEntityOptions("");
            PromptEntityResult promptEntityResult = doc.Editor.GetEntity(pEntityOptions);

            Point2d pt = promptEntityResult.PickedPoint.Convert2d(new Plane(new Point3d(), new Vector3d(0, 0, 1)));
            Point3d pt3 = promptEntityResult.PickedPoint.DivideBy(1);
            Circle cir = AutoGeo.CreateCircle(pt3, 10);
            
            AutoTools.AddGeo(blockTableRecord, transaction, cir);

            transaction.Commit();
        }
    }
}