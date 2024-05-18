// (C) Copyright 2024 by
//
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using QAR;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(Simple_DrawJig_by_AJS.MyCommands))]

namespace Simple_DrawJig_by_AJS
{
    public partial class MyCommands
    {
        [CommandMethod("Mario", CommandFlags.Modal)]
        public void MyCommand()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.RejectObjectsOnLockedLayers = true;
            PromptSelectionResult result = ed.GetSelection(pso);
            if (result.Status != PromptStatus.OK) return;

            ObjectIdCollection ids = new ObjectIdCollection(result.Value.GetObjectIds());
            PromptPointOptions ppo = new PromptPointOptions("\nSpecify base point");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            Point3d bspt = ppr.Value;

            //Second point
            Point3d input_bspt = bspt;

            string retkword = "";
            PromptStatus ps = QARJig.RectangArrayGetSecondPoint(ids, ref input_bspt, "", ref retkword);
            if (ps != PromptStatus.OK || bspt.DistanceTo(input_bspt) < 0.0000001) return;

            Point3d pt = input_bspt;

            //Third point
            Point3d sept = input_bspt;
            input_bspt = bspt;

            int NumberRow = 1;
            int NumberCol = 1;
            double unitWidth = pt.X - bspt.X;
            double unitHeight = pt.Y - bspt.Y;
            ps = QARJig.RectangArrayGetThirdPoint(ids, ref input_bspt, sept, "", ref retkword, ref NumberRow, ref NumberCol, ref unitWidth, ref unitHeight);

            ed.WriteMessage("\nwww.lisp.vn");
        }
    }
}