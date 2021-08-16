using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADLearning.BasicTools {
    public class AutoTools {
        public LayerTable layerTable;
        public BlockTableRecord blockRecord;
        public Transaction transaction;

        /// <summary>
        /// 关于AutoCAD画布空间的系统整合类
        /// 部分方法需要初始化之后才可以使用
        /// </summary>
        /// <param name="blockRecord"></param>
        /// <param name="layerTable"></param>
        /// <param name="transaction"></param>
        public AutoTools(BlockTableRecord blockRecord, LayerTable layerTable, Transaction transaction)
        {
            this.blockRecord = blockRecord;
            this.layerTable = layerTable;
            this.transaction = transaction;
        }

        /// <summary>
        ///  将新建的图元添加进transaction
        /// </summary>
        /// <param name="entityList"></param>
        public static void AddGeo(BlockTableRecord blockRecord, Transaction transaction, params Entity[] entityList)
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
        public static void AddLayer(LayerTable layerTable, Transaction transaction,
            params LayerTableRecord[] layerRecords)
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
        public static void CreateLayer(LayerTable layerTable, Transaction transaction, string name)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;

                AddLayer(layerTable, transaction, layerTableRecord);
            }
        }

        /// <summary>
        /// 添加图层并设置颜色
        /// </summary>
        /// <param name="name"></param>
        public static void CreateLayer(LayerTable layerTable, Transaction transaction, string name, short colorIndex)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;
                layerTableRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                AddLayer(layerTable, transaction, layerTableRecord);
            }
        }

        /// <summary>
        /// 添加图层并设置颜色以及线型
        /// </summary>
        /// <param name="name"></param>
        public static void CreateLayer(LayerTable layerTable, Transaction transaction, string name, short colorIndex, string lineType)
        {
            layerTable.UpgradeOpen();

            if (!layerTable.Has(name))
            {
                LayerTableRecord layerTableRecord = new LayerTableRecord();
                layerTableRecord.Name = name;
                layerTableRecord.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                AddLayer(layerTable, transaction, layerTableRecord);
            }
        }

        /// <summary>
        /// 将图元添加进指定图层
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="entityList"></param>
        public static void AddGeo2Layer(LayerTable layerTable, string layerName, params Entity[] entityList)
        {
            if (layerTable.Has(layerName))
            {
                foreach (var VARIABLE in entityList)
                {
                    VARIABLE.Layer = layerName;
                }
            }
            else
            {
                AlertCreator("指定图层名称不匹配");
            }
        }

        /// <summary>
        /// 从用户获取一个点
        /// </summary>
        /// <param name="pPointOptions"></param> 新建一个空的即可
        /// <param name="doc"></param>          当前文档doc
        /// <param name="message"></param>
        /// <param name="ifUseBasePt"></param>
        /// <param name="basePoint"></param>
        /// <returns></returns>
        public static Point3d GetPointFromUser(PromptPointOptions pPointOptions, Document doc, string message,
            bool ifUseBasePt, Point3d basePoint)
        {
            pPointOptions.Message = message;
            pPointOptions.UseBasePoint = ifUseBasePt;
            pPointOptions.BasePoint = basePoint;
            PromptPointResult ptsResult = doc.Editor.GetPoint(pPointOptions);
            return ptsResult.Value;
        }

        /// <summary>
        /// 从用户获取两个点，[0]是起点，[1]是终点
        /// 默认打开baseLine
        /// </summary>
        /// <param name="pPointOptions"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Point3d[] Get2PtsFromUser(PromptPointOptions pPointOptions, Document doc)
        {
            Point3d[] pts = new Point3d[2];
            Point3d pStart = AutoGeo.GetPointFromUser(pPointOptions, doc, "请输入第一个点：", false, new Point3d(0, 0, 0));
            Point3d pEnd = AutoGeo.GetPointFromUser(pPointOptions, doc, "请输入第二个点：", true, pStart);

            pts[0] = pStart;
            pts[1] = pEnd;

            return pts;
        }

        /// <summary>
        /// 创建AutoCAD级别的Alert
        /// </summary>
        /// <param name="message"></param>
        public static void AlertCreator(string message)
        {
            Application.ShowAlertDialog(message);
        }

        /// <summary>
        /// 以图层名称设置过滤器
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static SelectionFilter SetFilter(string layerName)
        {
            //过滤器只能识别到物体的属性，父图层的属性识别不到
            TypedValue[] typleValues = new TypedValue[1];
            typleValues.SetValue(new TypedValue((int) DxfCode.LayerName, layerName), 0);

            SelectionFilter filter = new SelectionFilter(typleValues);
            return filter;
        }

        /// <summary>
        /// 以图层名称设置过滤器
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static SelectionFilter SetTypFilter(string type)
        {
            //过滤器只能识别到物体的属性，父图层的属性识别不到
            TypedValue[] typleValues = new TypedValue[1];
            typleValues.SetValue(new TypedValue((int) DxfCode.Start, type), 0);

            SelectionFilter filter = new SelectionFilter(typleValues);
            return filter;
        }

        /// <summary>
        /// 以图层名称和图元类型设置过滤器
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static SelectionFilter SetFilter(string layerName, string type)
        {
            //过滤器只能识别到物体的属性，父图层的属性识别不到
            TypedValue[] typleValues = new TypedValue[2];
            typleValues.SetValue(new TypedValue((int) DxfCode.LayerName, layerName), 0);
            typleValues.SetValue(new TypedValue((int) DxfCode.Start, type), 1);

            SelectionFilter filter = new SelectionFilter(typleValues);
            return filter;
        }

        /// <summary>
        /// 设定用户选项
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="defaultOpt"></param> 默认值
        /// <param name="opts"></param> 备选值
        /// <returns></returns>
        public static string GetStrWithOpts(Document doc, string message, string defaultOpt, params string[] opts)
        {
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\n" + message);
            foreach (var VARIABLE in opts)
            {
                pKeyOpts.Keywords.Add(VARIABLE);
            }

            pKeyOpts.Keywords.Default = defaultOpt;
            PromptResult pResult = doc.Editor.GetKeywords(pKeyOpts);

            return pResult.StringResult;
        }





    }
}