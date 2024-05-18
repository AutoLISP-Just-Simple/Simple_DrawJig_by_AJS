using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.Generic;
using System.Linq;

namespace QAR
{
    public class RectangArraySecond : DrawJig
    {
        private DBObjectCollection[,] ArrayedEntities;
        private double unitwidth;
        private double unitheight;
        private Point3d basePoint, dragPoint;
        private string arraytype;
        private Matrix3d xform;
        private JigPromptPointOptions moptions;
        private Entity[] entities;

        public Matrix3d Transform
        { get { return xform; } }

        public Matrix3d UCS = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;

        public RectangArraySecond(IEnumerable<Entity> in_ents, Point3d basePt, JigPromptPointOptions options, string arrayType)
        {
            this.entities = in_ents.ToArray();
            this.basePoint = basePt.TransformBy(Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
            this.moptions = options;
            this.arraytype = arrayType;
            this.ArrayedEntities = new DBObjectCollection[2, 2];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    DBObjectCollection ents = new DBObjectCollection();
                    if (!(i == 0 && j == 0)) foreach (Entity e in entities) ents.Add(e.Clone() as Entity);
                    ArrayedEntities[i, j] = ents;
                }
            }
        }

        public Matrix3d ArrayMatrix(int row, int col)
        {
            Vector3d vec = new Vector3d(unitwidth * col, unitheight * row, 0);
            Matrix3d mat = Matrix3d.Displacement(vec.TransformBy(UCS));
            return mat;
        }

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                if (arraytype == "Array")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            geo.PushModelTransform(ArrayMatrix(i, j));
                            for (int k = 0; k < ArrayedEntities[i, j].Count; ++k) geo.Draw(ArrayedEntities[i, j][k].Drawable);
                            geo.PopModelTransform();
                        }
                    }
                }
                else if (arraytype == "Rotation")
                {
                    geo.PushModelTransform(ArrayMatrix(1, 1));
                    for (int k = 0; k < ArrayedEntities[1, 1].Count; ++k) geo.Draw(ArrayedEntities[1, 1][k].Drawable);
                    geo.PopModelTransform();
                }
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            PromptPointResult prResult1 = prompts.AcquirePoint(this.moptions);
            if (prResult1.Status != PromptStatus.OK) return SamplerStatus.Cancel;

            dragPoint = prResult1.Value;
            Vector3d v3d = basePoint.TransformBy(UCS.Inverse()).GetVectorTo(dragPoint.TransformBy(UCS.Inverse()));
            if (unitwidth == v3d.X && unitheight == v3d.Y || v3d.Length == 0) return SamplerStatus.NoChange;
            unitwidth = v3d.X;
            unitheight = v3d.Y;
            xform = Matrix3d.Displacement(v3d);
            return SamplerStatus.OK;
        }
    }
}