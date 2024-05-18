using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace QAR
{
    internal class QARJig
    {
        //Third Point
        public static PromptStatus RectangArrayGetThirdPoint(ObjectIdCollection source, ref Point3d bspt, Point3d sept, string kword, ref string retkword, ref int NumberRow, ref int NumberCol,
            ref double unitWidth, ref double unitHeight)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptStatus ps = PromptStatus.None;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity[] entities = new Entity[source.Count];
                for (int i = 0; i < source.Count; i++) entities[i] = (Entity)tr.GetObject(source[i], OpenMode.ForRead);
                JigPromptPointOptions jppo = new JigPromptPointOptions("\nSpecify third point");
                jppo.BasePoint = bspt.TransformBy(ed.CurrentUserCoordinateSystem);
                jppo.UseBasePoint = true;
                if (kword != "")
                {
                    jppo.Keywords.Add(kword);
                    jppo.AppendKeywordsToMessage = true;
                }
                jppo.Cursor = CursorType.RubberBand;
                jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
                jppo.UserInputControls = UserInputControls.NullResponseAccepted;

                RectangArrayThird jigger = new RectangArrayThird(entities, bspt, ref sept, jppo, "Array");

                PromptResult jigRes = Application.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                ps = jigRes.Status;
                if (jigRes.Status == PromptStatus.OK)
                {
                    NumberRow = jigger.NumberRow + 1;
                    NumberCol = jigger.NumberCol + 1;
                    unitWidth = jigger.unitwidth;
                    unitHeight = jigger.unitheight;
                    bspt = bspt.TransformBy(jigger.Transform);
                }
                else if (jigRes.Status == PromptStatus.Keyword)
                    retkword = jigRes.StringResult;
                tr.Commit();
            }
            return ps;
        }

        //Second Point
        public static PromptStatus RectangArrayGetSecondPoint(ObjectIdCollection source, ref Point3d bspt, string kword, ref string retkword)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptStatus ps = PromptStatus.None;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity[] entities = new Entity[source.Count];
                for (int i = 0; i < source.Count; i++) entities[i] = (Entity)tr.GetObject(source[i], OpenMode.ForRead);
                JigPromptPointOptions jppo = new JigPromptPointOptions("\nSpecify second point");
                jppo.BasePoint = bspt.TransformBy(ed.CurrentUserCoordinateSystem);
                jppo.UseBasePoint = true;
                if (kword != "")
                {
                    jppo.Keywords.Add(kword);
                    jppo.AppendKeywordsToMessage = true;
                }
                jppo.Cursor = CursorType.RubberBand;
                jppo.UserInputControls = UserInputControls.Accept3dCoordinates;
                jppo.UserInputControls = UserInputControls.NullResponseAccepted;

                RectangArraySecond jigger = new RectangArraySecond(entities, bspt, jppo, "Array");

                PromptResult jigRes = Application.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                ps = jigRes.Status;
                if (jigRes.Status == PromptStatus.OK)
                    bspt = bspt.TransformBy(jigger.Transform);
                else if (jigRes.Status == PromptStatus.Keyword)
                    retkword = jigRes.StringResult;
                tr.Commit();
            }
            return ps;
        }
    }
}