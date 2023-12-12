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
            int allChekCounter = 0;
            //int dnNotMatchPidCounter = 0;
            List<Network> networkList = DocumentData.CurrentCivilDocument.GetNetworks(); //все сети
            if (networkList.Count == 0)
                return;

            using (Transaction ts = DocumentData.CurrentDatabase.TransactionManager.StartTransaction())
            {

                List<Pipe> pipeList = new List<Pipe>();
                List<Pipe> chanelList = new List<Pipe>();


                foreach (Network network in networkList)
                {
                    //pipeList.AddRange(network.GetPipesOfNetwork());

                    foreach (ObjectId pipeId in network.GetPipeIds())
                    {
                        Pipe pipe = ts.GetObject(pipeId, OpenMode.ForRead, false, true) as Pipe;

                        string description = pipe.Description;
                        bool containsChanelType = PipeData.ChanelType.Contains(description);


                        if (containsChanelType == true)
                        {
                            chanelList.Add(pipe);
                        }

                        else
                        {
                            pipeList.Add(pipe);
                        }
                    }
                }

                foreach (Pipe chanel in chanelList)
                {
                    string description = chanel.Description;
                    PartDataRecord chanelPDR = chanel.PartData;
                    string chanelDN = chanelPDR.GetDataFieldBy("DN").Value.ToString();
                    string key = description + " " + chanelDN;
                    ObjectId chanelRefAlignmentId = chanel.RefAlignmentId;

                    //bool containsChanelType = PipeData.ChanelType.Contains(description);
                    ////bool containsKey = PipeData.ChanelType.ContainsKey(description);
                    ////if (pipe.Description == "В стальных футлярах и ж.б. обойме")
                    //if (containsChanelType == true)
                    //{
                    //foreach (Pipe otherPipe in pipeList) //Внутренний цикл
                    //{
                    foreach (Pipe pipe in pipeList) //Внутренний цикл
                    {
                        //if (chanel.ObjectId != otherPipe.ObjectId) // Исключаем текущую трубу из проверки
                        //    {
                        //PartDataRecord pipePDR = pipe.PartData;
                        //string pipePID = pipePDR.GetDataFieldBy("DN").Value.ToString();
                        ObjectId pipeRefAlignmentId = pipe.RefAlignmentId;
                        if (chanelRefAlignmentId == pipeRefAlignmentId)
                        {
                            //string pipePID = pipe.OuterDiameterOrWidth.ToString();

                            LineSegment3d lineSegment3D = new LineSegment3d(PointExts.ChangeH(pipe.StartPoint, 0),
                                                                        PointExts.ChangeH(pipe.EndPoint, 0));

                            Tolerance acrossTolerance = new Tolerance(1, 0.01);
                            Tolerance alongTolerance = new Tolerance(0, 0.10);

                            bool isOnlineStartPointAcrossTolerance = lineSegment3D.IsOn(PointExts.ChangeH(chanel.StartPoint, 0), acrossTolerance);
                            bool isOnlineEndPointAcrossTolerance = lineSegment3D.IsOn(PointExts.ChangeH(chanel.EndPoint, 0), acrossTolerance);

                            bool isOnlineStartPointAlongTolerance = lineSegment3D.IsOn(PointExts.ChangeH(chanel.StartPoint, 0), alongTolerance);
                            bool isOnlineEndPointAlongTolerance = lineSegment3D.IsOn(PointExts.ChangeH(chanel.EndPoint, 0), alongTolerance);


                            if ((isOnlineStartPointAcrossTolerance == true || isOnlineStartPointAlongTolerance == true) &&
                                (isOnlineEndPointAcrossTolerance == true || isOnlineEndPointAlongTolerance == true))
                            {
                                double pipeStartPointNewZ = PointExts.GetPointZValueOnPipe(pipe, chanel.StartPoint) + PipeData.distanceBetweenAxles[key];
                                double pipeEndPointNewZ = PointExts.GetPointZValueOnPipe(pipe, chanel.EndPoint) + PipeData.distanceBetweenAxles[key];

                                chanel.UpgradeOpen();
                                chanel.StartPoint = new Point3d(chanel.StartPoint.X, chanel.StartPoint.Y, pipeStartPointNewZ);
                                chanel.EndPoint = new Point3d(chanel.EndPoint.X, chanel.EndPoint.Y, pipeEndPointNewZ);
                                allChekCounter++;
                                chanel.DowngradeOpen();

                                break; //если есть мэтч
                            }

                        }
                        //else
                        //{
                        //    dnNotMatchPidCounter++;
                        //}
                        //}
                        //}
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
            MessageBox.Show($"Всего проверенно: {allChekCounter.ToString()}", "Выравнивание каналов");
        }
    }
}
