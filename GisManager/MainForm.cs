using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.Collections.Generic;
using stdole;
using ESRI.ArcGIS.DataSourcesFile;

namespace GisManager
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //加载地图
            //string sPath=System.IO.Path.Combine(Application.StartupPath,@"Map\Map.mxd");
            //axMapControl1.LoadMxFile(sPath);
            //axMapControl1.Refresh();

            //InitList();

            //PublicDim.ReverseMouseWheel();

            ////初始化图层
            //CreateDrawnLayer();
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced evant in order to update the statusbar and the Save menu
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            //get the current document name from the MapControl
            m_mapDocumentName = m_mapControl.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                statusBarXY.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                statusBarXY.Text = System.IO.Path.GetFileName(m_mapDocumentName);
            }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }

        #region 私有方法
        private IFeatureLayer DrawnLayer;
        /// <summary>
        /// 创建人员图层
        /// </summary>
        /// <param name="DataSetName"></param>
        /// <param name="AliaseName"></param>
        /// <param name="SpatialRef"></param>
        /// <param name="GeometryType"></param>
        /// <param name="PropertyFields"></param>
        /// <returns></returns>
        private void CreateDrawnLayer()
        {
            try
            {
                if (axMapControl1.LayerCount >= 2)
                {
                    axMapControl1.DeleteLayer(0);
                }
            }
            catch
            { }

            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", "MyWorkspace", null, 0);
            ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
            ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();
            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;
            IFeatureLayer oFeatureLayer = null;
            try
            {
                oFieldsEdit = oFields as IFieldsEdit;
                oFieldEdit = oField as IFieldEdit;

                //创建图元属性
                FieldClass feadName= new FieldClass();
                IFieldEdit edit = feadName;
                edit.Name_2 = "Name";
                edit.Type_2 = esriFieldType.esriFieldTypeString;
                oFieldsEdit.AddField(feadName);

                FieldClass feadID = new FieldClass();
                IFieldEdit editID = feadID;
                editID.Name_2 = "ID";
                editID.Type_2 = esriFieldType.esriFieldTypeString;
                oFieldsEdit.AddField(editID);


                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                //geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = false;
                geometryDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;
                oFieldEdit.Name_2 = "SHAPE";
                oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                oFieldEdit.GeometryDef_2 = geometryDef;
                oFieldEdit.IsNullable_2 = true;
                oFieldEdit.Required_2 = true;
                oFieldsEdit.AddField(oField);
                oFeatureClass = (inmemWor as IFeatureWorkspace).CreateFeatureClass("DrawnDS", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                (oFeatureClass as IDataset).BrowseName = "DrawnDS";
                oFeatureLayer = new FeatureLayerClass();
                oFeatureLayer.Name = "Drawn";
                oFeatureLayer.FeatureClass = oFeatureClass;

            }
            catch
            {
            }
            finally
            {
                try
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oField);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFields);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldsEdit);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldEdit);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(name);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workspaceFactory);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workspaceName);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inmemWor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFeatureClass);
                }
                catch { }

                GC.Collect();
            }

            DrawnLayer = oFeatureLayer;
            axMapControl1.Map.AddLayer(oFeatureLayer);
            UniqueValueRenderFlyr(oFeatureLayer);
            axMapControl1.Refresh();
            
        }

        ///<summary>
        ///设置要素图层唯一值符号化
        ///</summary>
        ///<param name="pFeatureLayer"></param>
        private void UniqueValueRenderFlyr(IFeatureLayer pFeatureLayer)
        {
            //创建SimpleMarkerSymbolClass对象

            SimpleLineSymbolClass pSimpleMarkerSymbol = new SimpleLineSymbolClass();
            //创建RgbColorClass对象为pSimpleMarkerSymbol设置颜色
            IRgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = 255;
            pSimpleMarkerSymbol.Color = pRgbColor as IColor;
            pSimpleMarkerSymbol.Style = esriSimpleLineStyle.esriSLSDash;
            //设置pSimpleMarkerSymbol对象大小，设置为５
            pSimpleMarkerSymbol.Width = 1;
            //显示外框线
            //pSimpleMarkerSymbol.Outline = true;
            //为外框线设置颜色
            IRgbColor pLineRgbColor = new RgbColorClass();
            pLineRgbColor.Green = 255;
            //pSimpleMarkerSymbol.OutlineColor = pLineRgbColor as IColor;
            //设置外框线的宽度
            //pSimpleMarkerSymbol.OutlineSize = 1;

            IUniqueValueRenderer pUVRender = new UniqueValueRendererClass();
            pUVRender.AddValue("", "", pSimpleMarkerSymbol as ISymbol);
            pUVRender.DefaultLabel = "Name";
            pUVRender.DefaultSymbol = pSimpleMarkerSymbol as ISymbol;

            //设置IGeofeatureLayer的Renderer属性
            (pFeatureLayer as IGeoFeatureLayer).Renderer= pUVRender as IFeatureRenderer;
        }

        #endregion

        private void cmiClear_Click(object sender, EventArgs e)
        {
            clear();
        }


        List<IFeature> lstFeature;
        private void InitList()
        {
            lstFeature = new List<IFeature>();
            ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
            queryFilter.WhereClause = "1=1";
            IFeatureLayer pFeatureLayer = axMapControl1.get_Layer(0) as IFeatureLayer;
            IFeatureCursor featureCursor = pFeatureLayer.Search(queryFilter, false);
            ESRI.ArcGIS.Geodatabase.IFeature pFeature;
            while ((pFeature = featureCursor.NextFeature()) != null)
            {
                lstFeature.Add(pFeature);
            }
        }

        private void cmiDraw_Click(object sender, EventArgs e)
        {
            SelectDistance frmSelectDistance = new SelectDistance();
            if (frmSelectDistance.ShowDialog() == DialogResult.OK)
            {
                this.Drawn(frmSelectDistance.Distance);
            }
        }

        private void Drawn(int Distance)
        {
            pGra = axMapControl1.Map as IGraphicsContainer;
            pGra.DeleteAllElements();
            pAcitveView = pGra as IActiveView;
            //清空绘图
            clear();

            //分析矩形关系
            //List<IPolyline> lstAllFeature = new List<IPolyline>();
            foreach (IFeature f in lstFeature)
            {
                if (f.Shape is IPointCollection)
                {
                    IPointCollection p=f.Shape as IPointCollection;
                    List<IPoint> listPPP=new List<IPoint>();
                    for(int i=0;i<p.PointCount;i++)
                    {
                        listPPP.Add(p.get_Point(i));
                    }

                    List<TuLine> listTuline=PublicDim.DrawDog(listPPP,Distance);
                    foreach(TuLine t in listTuline)
                    {
                        //g.DrawLine(pen, ((Line)linelist[i]).Begin, ((Line)linelist[i]).End);
                        ILineElement pLineEle = new LineElementClass();
                        pEle = pLineEle as IElement;
                        ILineSymbol pLineSym = new SimpleLineSymbolClass();
                        //pLineSym.Color = this.getRGBColor(0, 0, 255);
                        pLineSym.Width = 2;
                        pLineEle.Symbol = pLineSym;
                        //pFeat = new LineClass();
                        //pFeat.Shape
                        pEle.Geometry = CreatePolyline(t);
                        pGra.AddElement(pEle, 0);
                        pAcitveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    }
                }
            }
            //axMapControl1.Refresh();
        }

        private ArrayList list = new ArrayList();
        private ArrayList linelist = new ArrayList();
        private void TuBao(IPoint A, IPoint B, ArrayList C)//判断C中所有点是否在A，B连线的同一侧。。。。
        {
            int count;
            count = 0;

            foreach (IPoint D in C)
            {
                //int count;
                double z;
                //count = 0;
                z = (D.X - A.X) * (B.Y - A.Y) / (B.X - A.X) + A.Y;
                if (z > D.Y)
                {

                    count++;
                }
            }
            if (count == list.Count - 2||count == 0)
            {
                TuLine line = new TuLine();
                line.Begin = A;
                line.End = B;
                linelist.Add(line);

            }


        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (pFeatureLayer != null)
            {
                if (e.button == 2)
                {
                    contextMenuStrip1.Show(new System.Drawing.Point(e.x + 191, e.y + 52));
                }
            }
        }
        IElement pEle;
        //在绘制前，清除mainkMapControl中的任何图形元素
        IGraphicsContainer pGra;
        IActiveView pAcitveView;
        IFeatureClass pFeatCls;//好像不能放函数体里面，放里出现提示。
        IFeature pFeat;
        private void DrawLineInMap(List<TuLine> listDraw)
        {
            foreach (TuLine t in listDraw)
            {
                ILineElement pLineEle = new LineElementClass();
                pEle = pLineEle as IElement;
                ILineSymbol pLineSym = new SimpleLineSymbolClass();
                //pLineSym.Color = this.getRGBColor(0, 0, 255);
                pLineSym.Width = 2;
                pLineEle.Symbol = pLineSym;
                //pFeat = new LineClass();
                //pFeat.Shape
                pEle.Geometry = CreatePolyline(t);;
                pGra.AddElement(pEle, 0);
                pAcitveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                //string PersionID = Guid.NewGuid().ToString();
                //string PersionName = PersionID;

                //IFeatureLayer pFeatureLyr = DrawnLayer as IFeatureLayer;
                //IFeatureClass pFeatCls = pFeatureLyr.FeatureClass;
                //IDataset pDataset = pFeatCls as IDataset;
                //IWorkspace pWS = pDataset.Workspace;
                //IWorkspaceEdit pWorkspaceEdit = pWS as IWorkspaceEdit;
                //pWorkspaceEdit.StartEditing(false);
                //pWorkspaceEdit.StartEditOperation();
                //IFeatureBuffer pFeatureBuffer;
                //IFeatureCursor pFeatureCuror;
                //IFeature pFeature;
                //IPolyline pPoint = CreatePolyline(t);
                //pFeatureBuffer = pFeatCls.CreateFeatureBuffer();
                //pFeatureCuror = pFeatCls.Insert(true);
                //pFeature = pFeatureBuffer as IFeature;

                //int field1 = pFeature.Fields.FindField("Name");
                //pFeature.set_Value(field1, PersionName);
                //field1 = pFeature.Fields.FindField("ID");
                //pFeature.set_Value(field1, PersionID);

                //IGeometry pPointGeo = pPoint as IGeometry;
                //pFeature.Shape = pPointGeo;
                //pFeatureCuror.InsertFeature(pFeatureBuffer);
                //pWorkspaceEdit.StopEditOperation();
                //pWorkspaceEdit.StopEditing(true);
            }
        }

        public static IPolyline CreatePolyline(TuLine t)
        {
            ISegment pSegment;
            ILine pLine;
            object o = Type.Missing;
            ISegmentCollection pPath = new PathClass();
            pLine = new LineClass();
            pLine.PutCoords(t.Begin, t.End);
            pSegment = pLine as ISegment;
            pPath.AddSegment(pSegment, ref o, ref o);
            IGeometryCollection pPolyline = new PolylineClass();
            pPolyline.AddGeometry(pPath as IGeometry, ref o, ref o);
            return pPolyline as IPolyline;
        }

        //计算两点之间距离
        private double CalcDistance(IPoint from, IPoint to)
        {
            return Math.Sqrt(Math.Abs(from.X - to.X) * Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) * Math.Abs(from.Y - to.Y));
        }

        private void clear()
        {
            try
            {
                pGra.DeleteAllElements();
            }
            catch
            { }
            //CreateDrawnLayer();
            //try { 
            //    OleDbConnection oleconn = getConn(); 
            //    oleconn.Open(); string strSel = "Delete From [Myuser] where UserID=" + m_id.ToString(); OleDbCommand myCommand = new OleDbCommand(strSel, oleconn); myCommand.ExecuteNonQuery(); oleconn.Close(); }
            //catch (Exception e) { throw new Exception("删除记录出错" + e.Message); }
            ////try
            ////{
            ////    //DrawnLayer.FeatureClass.FeatureDataset.Delete();

                
            ////}
            ////catch
            ////{ }
        }

        string fileName;
        string filePath;
        string strFullPath;
        IFeatureLayer pFeatureLayer;//点图层
        private void menuOpenShp_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;

            //获取当前路径和文件名
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "shpfile|*.shp";
            String ThiessenPolygoPath = @"C:\MyVoronoi\ThiessenPolygo.Shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                strFullPath = dlg.FileName;
                if (strFullPath == "") return;
                int Index = strFullPath.LastIndexOf("\\");
                filePath = strFullPath.Substring(0, Index);
                fileName = strFullPath.Substring(Index + 1);
                //打开工作空间并添加shp文件
                pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
                //注意此处的路径是不能带文件名的
                pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filePath, 0);
                pFeatureLayer = new FeatureLayerClass();
                //注意这里的文件名是不能带路径的
                pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(fileName);
                if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show("shp文件不是线要素图层");
                    return;
                }
                pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                axMapControl1.Map.AddLayer(pFeatureLayer);
                axMapControl1.ActiveView.Refresh();
                menuOpenShp.Enabled = false;


                InitList();

                PublicDim.ReverseMouseWheel();

                //初始化图层
                CreateDrawnLayer();

            }
        }

        
    }
}