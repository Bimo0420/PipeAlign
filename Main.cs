using Autodesk.Aec.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using CVL3Dv23LibraryVAA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;

namespace PipeAlign
{
    public class Main
    {
        [CommandMethod("VAA", "PipeAlign", CommandFlags.Modal)]

        public static void CommandRun()
        {
            List <Network> networkList = DocumentData.CurrentCivilDocument.GetNetworks(); //все сети
            if (networkList.Count == 0)
                return;
                    
            using (Transaction ts = DocumentData.CurrentDatabase.TransactionManager.StartTransaction())
            {

                List<Pipe> pipeList = new List<Pipe>();

                foreach (Network oNetwork in networkList)
                {
                    pipeList.AddRange(oNetwork.GetPipesOfNetwork());

                    foreach (ObjectId oPipeId in oNetwork.GetPipeIds())
                    {
                        Pipe oPipe = ts.GetObject(oPipeId, OpenMode.ForWrite, false, true) as Pipe;
                        pipeList.Add(oPipe);
                    }
                }

                foreach (Pipe pipe in pipeList)
                {
                    string description = pipe.Description;
                    bool containsKey = PipeData.ChanelType.ContainsKey(description);
                    //if (pipe.Description == "В стальных футлярах и ж.б. обойме")
                    if (containsKey == true)
                    {
                        foreach (Pipe otherPipe in pipeList) //Внутренний цикл
                        {
                            if (pipe.ObjectId != otherPipe.ObjectId) // Исключаем текущую трубу из проверки
                            {
                                LineSegment3d lineSegment3D = new LineSegment3d(PointExts.ChangeH(otherPipe.StartPoint, 0),
                                                                                PointExts.ChangeH(otherPipe.EndPoint, 0));

                                Tolerance tolerance = new Tolerance(1, 0.01);

                                bool isOnlineStartPoint = lineSegment3D.IsOn(PointExts.ChangeH(pipe.StartPoint, 0), tolerance);
                                bool isOnlineEndPoint = lineSegment3D.IsOn(PointExts.ChangeH(pipe.EndPoint, 0), tolerance);

                                if (isOnlineStartPoint == true && isOnlineEndPoint == true) 
                                {
                                    double pipeStartPointNewZ = PointExts.SetPointZValueOnPipe(otherPipe, pipe.StartPoint) + PipeData.ChanelType[description];
                                    double pipeEndPointNewZ = PointExts.SetPointZValueOnPipe(otherPipe, pipe.StartPoint) + PipeData.ChanelType[description];

                                    pipe.StartPoint = new Point3d(pipe.StartPoint.X, pipe.StartPoint.Y, pipeStartPointNewZ);
                                    pipe.EndPoint = new Point3d(pipe.EndPoint.X, pipe.EndPoint.Y, pipeEndPointNewZ);
                                }
                                                               
                            }
                        }
                    }
                }

                ts.Commit();
            }


            //List<Point3d> pipePoint = new List<Point3d>();
            //foreach (Pipe oPipe in pipeList)
            //{
            //    pipePoint.Add(oPipe.StartPoint);

            //}

            //string concatenatedItems = string.Join("\n", pipePoint);

            //MessageBox.Show($"5: {concatenatedItems}");
            MessageBox.Show("DONE");
        } 
    }
}
