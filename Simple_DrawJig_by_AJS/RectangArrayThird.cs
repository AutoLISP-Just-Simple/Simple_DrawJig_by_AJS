using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using lhBasic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QAR
{
    public class RectangArrayThird : DrawJig
    {
        public Matrix3d UCS = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;

        private static PICTURE Picture = PICTURE.LogoEN;
        public double unitwidth;
        public double unitheight;
        private Point3d basePoint, dragPoint;
        private Matrix3d xform;
        private string arraytype;
        private JigPromptPointOptions options;
        private List<Entity> entities = new List<Entity>();

        public Matrix3d Transform
        { get { return xform; } }

        public RectangArrayThird(IEnumerable<Entity> input_ents, Point3d basePt, ref Point3d secondPt, JigPromptPointOptions options, string arrayType)
        {
            this.entities = input_ents.Select(x => x.Clone() as Entity).ToList();
            this.basePoint = basePt.TransformBy(Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
            this.options = options;
            this.arraytype = arrayType;

            Vector3d v3d = basePt.GetVectorTo(secondPt);
            this.unitwidth = v3d.X;
            this.unitheight = v3d.Y;
            oldNumberRow = 0;
            oldNumberCol = 0;

            //ArrayedEntities = new List<DBObjectCollection>();
            //for (int i = 0; i < oldNumberRow * oldNumberCol; ++i)
            //{
            //    DBObjectCollection ents = new DBObjectCollection();
            //    foreach (Entity e in entities)
            //        ents.Add(e.Clone() as Entity);

            //    ArrayedEntities.Add(ents);
            //}
            if (Picture == PICTURE.LogoEN) Picture = PICTURE.Mario;
            else if (Picture == PICTURE.Mario) Picture = PICTURE.Coin;
            else Picture = PICTURE.LogoEN;
        }

        public int NumberRow, NumberCol, oldNumberRow, oldNumberCol;

        public Matrix3d ArrayMatrix(int row, int col)
        {
            Vector3d vec = new Vector3d(unitwidth * col, unitheight * row, 0);
            Matrix3d mat = Matrix3d.Displacement(vec.TransformBy(UCS));
            return mat;
        }

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo == null) return false;

            if (arraytype == "Array")
            {
                for (int i = 0; i <= NumberRow; i++)
                {
                    for (int j = 0; j <= NumberCol; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        geo.PushModelTransform(ArrayMatrix(i, j));

                        Autodesk.AutoCAD.Colors.Color col = LOGO_TK_EN.GetColor(Math.Abs(i), Math.Abs(j), Math.Abs(NumberRow), Math.Abs(NumberCol), Picture);
                        foreach (Entity e in entities)
                        {
                            e.Color = col;
                            geo.Draw(e.Drawable);
                        }
                        geo.PopModelTransform();
                    }
                }
            }
            else if (arraytype == "Rotation")
            {
                for (int j = Math.Min(NumberCol, 0); j <= Math.Max(NumberCol, 0); j++)
                {
                    Autodesk.AutoCAD.Colors.Color col = LOGO_TK_EN.GetColor(1, 1, 1, 1, Picture);
                    if (j == 0) continue;
                    geo.PushModelTransform(ArrayMatrix(j, j));
                    foreach (Entity e in entities)
                    {
                        e.Color = col;
                        geo.Draw(e);
                    }
                    geo.PopModelTransform();
                }
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            PromptPointResult prResult1 = prompts.AcquirePoint(this.options);
            if (prResult1.Status != PromptStatus.OK) return SamplerStatus.Cancel;

            dragPoint = prResult1.Value;
            Vector3d v3d = basePoint.TransformBy(UCS.Inverse()).GetVectorTo(dragPoint.TransformBy(UCS.Inverse()));
            double a = unitwidth == 0 ? 0 : (int)(v3d.X / unitwidth);
            double b = unitheight == 0 ? 0 : (int)(v3d.Y / unitheight);
            if (a > 1000) a = 1000;
            if (b > 1000) b = 1000;
            if (a < -1000) a = -1000;
            if (b < -1000) b = -1000;
            int newNumberCol = (int)a;
            int newNumberRow = 0;
            if (arraytype == "Array")
                newNumberRow = (int)b;
            else if (arraytype == "Rotation")
                newNumberRow = 0;
            xform = Matrix3d.Displacement(v3d);
            bool forceChange = false;
            if (newNumberCol < 0)
            {
                newNumberCol = Math.Abs(newNumberCol);
                unitwidth = -unitwidth;
                forceChange = true;
            }
            if (newNumberRow < 0)
            {
                newNumberRow = Math.Abs(newNumberRow);
                if (newNumberRow != 0) unitheight = -unitheight;
                forceChange = true;
            }
            if (!forceChange && NumberRow == newNumberRow && NumberCol == newNumberCol) return SamplerStatus.NoChange;

            NumberRow = newNumberRow;
            NumberCol = newNumberCol;
            return SamplerStatus.OK;
        }
    }
}