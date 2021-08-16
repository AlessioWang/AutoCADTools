using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADLearning {
    public class AutoGeo {
        public LayerTable layerTable;
        public BlockTableRecord blockRecord;
        public Transaction transaction;

        public AutoGeo(BlockTableRecord blockRecord, LayerTable layerTable, Transaction transaction)
        {
            this.blockRecord = blockRecord;
            this.layerTable = layerTable;
            this.transaction = transaction;
        }

        public static Line CreateLine(Point3d start, Point3d end)
        {
            return new Line(start, end);
        }

        public static Circle CreateCircle(Point3d center, double rad)
        {
            return new Circle(center, new Vector3d(0, 0, 1), rad);
        }

        public static Polyline CreatePolyline(List<Point2d> pts)
        {
            Polyline polyline = new Polyline();
            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, pts[i], 0, 0, 0);
            }

            return polyline;
        }

        public static DBPoint CreatePoint(Point3d pt)
        {
            return new DBPoint(pt);
        }

        /// <summary>
        ///  将新建的图元添加进transaction
        /// </summary>
        /// <param name="entityList"></param>
        public void AddGeo(params Entity[] entityList)
        {
            foreach (Entity entity in entityList)
            {
                //添加到块表记录
                blockRecord.AppendEntity(entity);

                // 添加到事务
                // 不加这一行图元不能被选中和编辑
                transaction.AddNewlyCreatedDBObject(entity, true);
            }
        }

        /// <summary>
        /// 将新建的图层layerRecord添加进transaction
        /// </summary>
        /// <param name="layerRecords"></param>
        public void AddLayer(params LayerTableRecord[] layerRecords)
        {
            foreach (LayerTableRecord layer in layerRecords)
            {
                layerTable.Add(layer);

                transaction.AddNewlyCreatedDBObject(layer, true);
            }
        }

        /// <summary>
        /// 添加简单图层
        /// </summary>
        /// <param name="name"></param>
        public void CreateLayer(string name)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;

                AddLayer(layerTableRecord);
            }
        }

        /// <summary>
        /// 添加图层并设置颜色
        /// </summary>
        /// <param name="name"></param>
        public void CreateLayer(string name, short colorIndex)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;
                layerTableRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                AddLayer(layerTableRecord);
            }
        }

        /// <summary>
        /// 添加图层并设置颜色以及线型
        /// </summary>
        /// <param name="name"></param>
        public void CreateLayer(string name, short colorIndex, string lineType)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;
                layerTableRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                AddLayer(layerTableRecord);
            }
        }

        /// <summary>
        /// 获取用户界面输入的点
        /// </summary>
        /// <param name="pPointOptions"></param>
        /// <param name="doc"></param>         当前文档的Document
        /// <param name="message"></param>     用户提示文本
        /// <param name="ifUseBasePt"></param> 是否显示基点
        /// <param name="basePoint"></param>   基点
        /// <returns></returns>
        public static Point3d GetPointFromUser(PromptPointOptions pPointOptions, Document doc,string message,
            bool ifUseBasePt, Point3d basePoint)
        {
            pPointOptions.Message = message;
            pPointOptions.UseBasePoint = ifUseBasePt;
            pPointOptions.BasePoint = basePoint;
            PromptPointResult ptsResult = doc.Editor.GetPoint(pPointOptions);
            return ptsResult.Value;
        }

        /// <summary>
        /// 获取三维多段线各定点坐标
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public static List<Point3d> GetVertices(Polyline3d pl)
        {
            List<Point3d> pts = new List<Point3d>();
            using (Transaction tran = pl.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in pl)
                {
                    //不可直接用point3d类
                    PolylineVertex3d pt = (PolylineVertex3d)tran.GetObject(
                        id, OpenMode.ForRead);
                    pts.Add(pt.Position);
                }
                tran.Commit();
            }
            return pts;
        }

        /// <summary>
        /// 镜像图元
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="baseLine"></param>
        /// <returns></returns>
        public static Entity MirrowGeo(Entity entity, Line3d baseLine)
        {
            return entity.GetTransformedCopy(Matrix3d.Mirroring(baseLine));
        }

    }

}