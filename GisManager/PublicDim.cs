using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using Microsoft.Win32;

namespace GisManager
{
    [Guid("875ec57f-39b1-49fb-a600-7b95d8317545")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GisManager.PublicDim")]
    public class PublicDim
    {
        public static void ReverseMouseWheel()
        {
            try
            {
                RegistryKey setKey = Registry.CurrentUser.OpenSubKey(@"Software\ESRI\ArcMap\Settings", true);
                if (setKey != null)
                {
                    if (setKey.GetValue("ReverseMouseWheel") == null)
                    {
                        setKey.SetValue("ReverseMouseWheel", 0, RegistryValueKind.DWord);
                    }
                    else if (setKey.GetValue("ReverseMouseWheel").ToString() != "0")
                    {
                        setKey.SetValue("ReverseMouseWheel", 0);
                    }
                }
            }
            catch { }
        }

        public static List<TuLine> DrawDog(List<IPoint> pointLoc, double yuzhi)
        {
            int number = pointLoc.Count;
            ////ԭʼ���ݻ�ͼ
            //Graphics myGraphics = this.CreateGraphics();
            //myGraphics.DrawLines(new Pen(Color.Red), pointLoc);
            //myGraphics.Dispose();

            // this.label1.Text = Convert.ToString(yuzhi);

            //��������
            //******************˼·*********************
            //���ȶ���һ�����ڴ洢�м������Զ���������ֵ�ĵ�ĵ������飬unpoint_index�����ݸ�����Ϊ������С
            //�ڶ���һ�����ڴ洢�м��Ѿ���������Զ���������ֵ�ĵ�ĵ������飬alpoint_index�����ݸ�����Ϊ������С
            //unfirst��unlast�ֱ������ڴ����еĵ���Ǳ�
            //����Զ��
            //Ȼ������Ǻ��鱾�������б�ʾ������һ����

            int un = 0;
            int al = -1;
            int unfirst = 0;
            int unlast = number - 1;

            int[,] unpoint_index = new int[number * (number - 1) / 2, 2];
            unpoint_index[0, 0] = 0;
            unpoint_index[0, 1] = number - 1;

            int[,] alpoint_index = new int[number * (number - 1) / 2, 2];
            int now = 0;

            double l_every = 0;//���߾���
            double l_most = 0;//������Զ���룬��ÿ��ѭ���и���ֵ0
            int index_most = 0;//��Զ��ĽǱ�

            pel:
            //����Զ��
            double[] ABC = lineFunction(pointLoc[unfirst].X, pointLoc[unfirst].Y, pointLoc[unlast].X, pointLoc[unlast].Y);
            for (int a = unfirst; a <= unlast; a++)
            {
                l_every = pointToLine(ABC[0], ABC[1], ABC[2], pointLoc[a].X, pointLoc[a].Y);
                if (l_every > l_most)
                {
                    l_most = l_every;
                    index_most = a;
                }
            }
            //����ֵ�ж�
            if (l_most >= yuzhi)
            {
                un++;
                unpoint_index[un, 0] = unfirst;
                unpoint_index[un, 1] = index_most;
                un++;
                unpoint_index[un, 0] = index_most;
                unpoint_index[un, 1] = unlast;
                l_most = 0;
                now++;
                unfirst = unpoint_index[now, 0];
                unlast = unpoint_index[now, 1];
                goto
                    pel;
            }
            else
            {
                al++;
                alpoint_index[al, 0] = unfirst;
                alpoint_index[al, 1] = unlast;

                if (now != un)
                {
                    l_most = 0;
                    now++;
                    unfirst = unpoint_index[now, 0];
                    unlast = unpoint_index[now, 1];
                    goto
                    pel;
                }
            }


            List<TuLine> listTuLine = new List<TuLine>();
            for (int a = 0; a <= al; a++)
            {
                TuLine tuLine = new TuLine();
                tuLine.Begin = pointLoc[alpoint_index[a, 0]];
                tuLine.End = pointLoc[alpoint_index[a, 1]];
                listTuLine.Add(tuLine);
            }
            return listTuLine;
            ////////Graphics myGraphics1 = this.CreateGraphics();
            ////////for (int a = 0; a <= al; a++)
            ////////{
            ////////    myGraphics1.DrawLine(new Pen(Color.Blue), pointLoc[alpoint_index[a, 0]], pointLoc[alpoint_index[a, 1]]);
            ////////}
            ////////myGraphics1.Dispose();
        }

        //��ֱ��
        static double[] lineFunction(double x1, double y1, double x2, double y2)
        {
            double[] abc = new double[3];
            if (x1 == x2)
            {
                abc[0] = 1;
                abc[1] = 0;
                abc[2] = -Convert.ToDouble(x1);
            }
            else
            {
                abc[0] = (Convert.ToDouble(y1) - Convert.ToDouble(y2)) / (Convert.ToDouble(x1) - Convert.ToDouble(x2));
                abc[1] = -1;
                abc[2] = Convert.ToDouble(y1) - abc[0] * Convert.ToDouble(x1);
            }
            return abc;
        }

        //��㵽�ߵľ���
        static double pointToLine(double A, double B, double C, double x, double y)
        {
            double L = 0;
            L = A * Convert.ToDouble(x) + B * Convert.ToDouble(y) + C;
            L = Math.Abs(L);
            L = L / Math.Pow((A * A + B * B), 0.5);
            return L;
        }
    }
}
