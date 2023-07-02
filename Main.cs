using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using CVL3Dv23LibraryVAA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace PipeAlign
{
    public class Main
    {
        [CommandMethod("VAA", "PipeAlign", CommandFlags.Modal)]

        public static void CommandRun()
        {
            var networks = DocumentData.CurrentCivilDocument.GetNetworks();
            if (networks.Count == 0)
                return;

            Network firstNetwork = networks.FirstOrDefault();
            List<Pipe> pipes = firstNetwork.GetPipesOfNetwork();

            List<string> pipeData = new List<string>();
            foreach (Pipe oPipe in pipes)
            {
                pipeData.Add($"{oPipe.PartSizeName}");

            }

            string concatenatedItems = string.Join("\n", pipeData);

            //MessageBox.Show($"Количество трубопроводных сетейq: {pipeData}");
            MessageBox.Show(concatenatedItems);
        } 
    }
}
