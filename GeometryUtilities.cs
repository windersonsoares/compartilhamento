using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//Revit
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace AYPluginsRevit.Utilities
{
    public static class GeometryUtilities
    {
        public static void MaxAndMinPointFromList(List<XYZ> points, out XYZ minPoint, out XYZ maxPoint)
        {
            List<double> coordsX = new List<double>();
            List<double> coordsY = new List<double>();
            List<double> coordsZ = new List<double>();

            foreach (XYZ point in points)
            {
                coordsX.Add(point.X);
                coordsY.Add(point.Y);
                coordsZ.Add(point.Z);
            }

            minPoint = new XYZ(coordsX.Min(), coordsY.Min(), coordsZ.Min());
            maxPoint = new XYZ(coordsX.Max(), coordsY.Max(), coordsZ.Max());
        }
        public static Outline OutLineFromElementList(Document doc, List<Element> lstElements)
        {
            List<XYZ> lstXYZ = new List<XYZ>();

            foreach (Element ele in lstElements)
            {
                BoundingBoxXYZ bb = ele.get_BoundingBox(null);

                lstXYZ.Add(bb.Min);
                lstXYZ.Add(bb.Max);
            }

            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            List<double> zs = new List<double>();

            foreach (XYZ p in lstXYZ)
            {
                xs.Add(p.X);
                ys.Add(p.Y);
                zs.Add(p.Z);
            }

            XYZ minP = new XYZ(xs.Min(), ys.Min(), zs.Min());
            XYZ maxP = new XYZ(xs.Max(), ys.Max(), zs.Max());

            Outline outline = new Outline(minP, maxP);

            return outline;
        }
        public static XYZ GetLineNormal(Line line)
        {
            XYZ direction = line.Direction;
            XYZ xvector = direction.CrossProduct(XYZ.BasisZ);
            XYZ normal = direction.CrossProduct(xvector);

            return normal;
        }
        /// <summary>
        /// Filtra as faces de acordo com sua orientação
        /// </summary>
        /// <param name="listFaces"></param>
        /// <param name="topFaces"></param>
        /// <param name="bottomFaces"></param>
        /// <param name="verticalFaces"></param>
        /// <param name="positiveFaces"></param>
        /// <param name="negativeFaces"></param>
        public static void FilterFaceByOrientation(List<Face> listFaces,
            out List<Face> topFaces,
            out List<Face> bottomFaces,
            out List<Face> verticalFaces,
            out List<Face> positiveFaces,
            out List<Face> negativeFaces)
        {
            topFaces = new List<Face>();
            bottomFaces = new List<Face>();
            verticalFaces = new List<Face>();
            positiveFaces = new List<Face>();
            negativeFaces = new List<Face>();

            foreach (Face f in listFaces)
            {
                if (f.ComputeNormal(new UV(0.5, 0.5)).IsAlmostEqualTo(new XYZ(0, 0, 1)))
                {
                    topFaces.Add(f);
                }
                else if (f.ComputeNormal(new UV(0.5, 0.5)).IsAlmostEqualTo(new XYZ(0, 0, -1)))
                {
                    bottomFaces.Add(f);
                }
                else if (f.ComputeNormal(new UV(0.5, 0.5)).IsAlmostEqualTo(new XYZ(0, 0, -1)))
                {
                    verticalFaces.Add(f);
                }
                else if ((f.ComputeNormal(new UV(0.5, 0.5)).Z > 0))
                {
                    positiveFaces.Add(f);
                }
                else if ((f.ComputeNormal(new UV(0.5, 0.5)).Z < 0))
                {
                    negativeFaces.Add(f);
                }
            }
        }
        // Encontra o elemento que gerou as faces da intersecção
        // Quando unidos os elementos é possível identificar nas faces de encontro qual elemento gerou aquela face
        public static void FindElementFromIntersection(UIDocument uidoc, Element eleA, Element eleB)
        {
            // Coleção que vai receber os elementos
            List<ElementId> joinedElements = new List<ElementId>();

            List<Solid> solids = Utilities.GeometryUtilities.GetElementSolids(eleA);

            // Para cada face do sólido procurar qual elemento gerou a mesma
            foreach (Solid s in solids)
            {
                foreach (Face f in s.Faces)
                {
                    // Coleção dos elementos a partir das faces
                    ICollection<ElementId> generatingElementIds = eleA.GetGeneratingElementIds(f);

                    // Remove o elemento original deixando apenas os elementos que se unem a ele
                    generatingElementIds.Remove(eleA.Id);

                    foreach (ElementId eleId in generatingElementIds)
                    {
                        // Caso não exista o Id na lista adicionar o mesmo
                        if (!joinedElements.Contains(eleId))
                        {
                            joinedElements.Add(eleId);
                        }
                    }
                }
            }

            // Seleciona os elementos
            uidoc.Selection.SetElementIds(joinedElements);
        }
        public static void CreateTestElement(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FamilySymbol symbol;
            try
            {
                symbol = collector.OfClass(typeof(FamilySymbol)).WhereElementIsElementType().Cast<FamilySymbol>().First(x => x.FamilyName == "Elemento teste");

                if (symbol != null)
                {

                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                    }

                    doc.Create.NewFamilyInstance(point, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }
            }
            catch (Exception)
            {
                TaskDialog.Show("Erro", "A família Elemento teste não esta no projeto, carregue a mesma antes de utilizar o comando");
            }



        }
        /// <summary>
        /// Pega o centro do triângulo
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static XYZ GetTriangleCenter(MeshTriangle triangle)
        {
            XYZ vertex1 = triangle.get_Vertex(0);
            XYZ vertex2 = triangle.get_Vertex(1);
            XYZ vertex3 = triangle.get_Vertex(2);

            XYZ center = new XYZ((vertex1.X + vertex2.X + vertex3.X) / 3,
                (vertex1.Y + vertex2.Y + vertex3.Y) / 3,
                (vertex1.Z + vertex2.Z + vertex3.Z) / 3);

            return center;
        }
        /// <summary>
        /// Pega a normal de cada face do sólido
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static IDictionary<Face, XYZ> FaceGetNormal(Solid solid)
        {
            //Dicionário para receber as faces e normais
            IDictionary<Face, XYZ> dictFaceNormal = new Dictionary<Face, XYZ>();

            //Para cada face do sólido
            foreach (Face gFace in solid.Faces)
            {
                XYZ normal = gFace.ComputeNormal(new UV(0.5, 0.5));
            }

            return dictFaceNormal;
        }

        /// <summary>
        ///  Une os sólidos da lista em um único sólido
        /// </summary>
        /// <param name=""></param>
        public static Solid JoinSolids(List<Solid> lsolids)
        {
            // Une os sólidos em um único sólido
            Solid joinedSolid;

            if (lsolids.Count > 1)
            {
                joinedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(lsolids[0], lsolids[1], BooleanOperationsType.Union);

                lsolids.RemoveRange(0, 2);

                if (lsolids.Count > 0)
                {
                    for (int i = 0; i < lsolids.Count; i++)
                    {
                        Solid joinedSolid2 = BooleanOperationsUtils.ExecuteBooleanOperation(joinedSolid, lsolids[i], BooleanOperationsType.Union);
                        joinedSolid = joinedSolid2;
                    }
                }
            }
            else
            {
                joinedSolid = lsolids[0];
            }

            return joinedSolid;
        }
        /// <summary>
        /// Pega todas as faces de uma lista de sólidos
        /// </summary>
        /// <param name="lstSolids"></param>
        /// <returns></returns>
        public static List<Face> GetSolidsFaces(List<Solid> lstSolids)
        {
            List<Face> lstFaces = new List<Face>();

            foreach (Solid sol in lstSolids)
            {
                FaceArray faceArr = sol.Faces;

                foreach (Face face in faceArr)
                {
                    lstFaces.Add(face);
                }
                
            }

            return lstFaces;
        }

        public static List<Face> GetSolidFace(Solid solid)
        {
            List<Face> lstFaces = new List<Face>();

            FaceArray faceArr = solid.Faces;

            foreach (Face face in faceArr)
            {
                lstFaces.Add(face);
            }

            return lstFaces;
        }

        /// <summary>
        /// Retorna uma lista contendo os sólidos do elemento
        /// </summary>
        /// <param name="element"></param>
        public static List<Solid> GetElementSolids(Element element)
        {
            // Opções da geometria
            Options gOptions = new Options();
            gOptions.DetailLevel = ViewDetailLevel.Fine;

            // Lista que receberá os sólidos
            List<Solid> lsolids = new List<Solid>();

            // Verifica se não é um ambiente
            Room room = element as Room;

            if (room != null)
            {
                lsolids.Add(Utilities.RoomUtilities.GetRoomGeometry(room, room.Document));
            }
            else
            {
                // Pega a geometria do elemento e a partir dela as faces, e então a face inferior
                GeometryElement geom = element.get_Geometry(gOptions);

                Solid gSolid = null;

                // Verifica se a geometria é uma instância no caso de famílias, caso contrário é uma família de sistema
                foreach (GeometryObject gObj in geom)
                {
                    GeometryInstance instance = gObj as GeometryInstance;

                    if (instance != null)
                    {
                        GeometryElement gEle = instance.GetInstanceGeometry();

                        foreach (GeometryObject gObj2 in gEle)
                        {
                            gSolid = gObj2 as Solid;

                            if (gSolid.Volume != 0)
                            {
                                lsolids.Add(gSolid);
                            }
                        }
                    }
                    else
                    {
                        gSolid = gObj as Solid;

                        if (gSolid.Volume != 0)
                        {
                            lsolids.Add(gSolid);
                        }
                    }
                }
            }



            return lsolids;
        }

        /// <summary>
        /// Verifica se duas linhas se interseccionam
        /// http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
        /// If 0 <= t1 <= 1, then the point lies on the first segment.
        /// If 0 <= t2 <= 1, then the point lies on the second segment.
        /// If dy1 * dx2 - dx1 * dy2 = 0, then you can’t calculate t1 or t2 (it would require dividing by 0), and the lines are parallel.
        /// If the point of intersection is not on both segments, then this is almost certainly not the point where the two segments are closest.
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        public static void Find2DLineIntersection(Line lineA, Line lineB, out bool linesIntersect, out bool segmentsIntersect, out XYZ intersection, out XYZ closestPA, out XYZ closestPB)
        {
            // Pontos das curvas
            XYZ p1 = lineA.GetEndPoint(0);
            XYZ p2 = lineA.GetEndPoint(1);
            XYZ p3 = lineB.GetEndPoint(0);
            XYZ p4 = lineB.GetEndPoint(1);

            // Pega os segmentos
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Soliciona t1 e t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = (((float)p1.X - (float)p3.X) * (float)dy34 + ((float)p3.Y - (float)p1.Y) * (float)dx34) / (float)Math.Round(denominator, 4);

            // Caso t1 seja infinito ou próximo as linhas são paralelas
            if (float.IsInfinity(t1))
            {
                TaskDialog.Show("Teste", "Infinity");
                linesIntersect = false;
                segmentsIntersect = false;
                intersection = new XYZ(0, 0, 0);
                closestPA = new XYZ(0, 0, 0);
                closestPB = new XYZ(0, 0, 0);
                return;
            }
            linesIntersect = true;

            double t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Acha o ponto de intersecção
            intersection = new XYZ(p1.X + dx12 * t1, p1.Y + dy12 * t1, 0);

            // Os segmentos interseccionam se t1 e t2 estiverem entre 0 e 1
            segmentsIntersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            // Encontra os pontos mais próximos dos segmentos
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            closestPA = new XYZ(p1.X + dx12 * t1, p1.Y + dy12 * t1, 0);
            closestPB = new XYZ(p3.X + dx34 * t2, p3.Y + dy34 * t2, 0);
        }

        /// <summary>
        /// Verifica se um ponto está do lado direito, esquerdo, ou na linham
        /// </summary>
        public static void PointDirectionByLine()
        {

        }

        /// <summary>
        /// Cria uma BoundingBox alinhada com o objeto de forma a manter o menor tamanho necessário
        /// </summary>
        public static void AlignedBoundingBox()
        {

        }

        /// <summary>
        /// Compute the distance between a point and a line
        /// </summary>
        /// <param name="a">The start point of the line</param>
        /// <param name="b">The end point of the line</param>
        /// <param name="p">The desired point to measure the distance</param>
        /// <returns></returns>
        public static double ComputeDistance(XYZ a, XYZ b, XYZ p)
        {
            XYZ d = (b - a) / b.DistanceTo(a);
            XYZ v = p - a;
            double t = v.DotProduct(d);
            XYZ P = a + t * d;
            return P.DistanceTo(p);
        }

        /// <summary>
        /// Calcula a distância entre um ponto e uma linha como se fossem 2D
        /// https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double ComputeDistance2DPointToBoundedLine(XYZ a, XYZ b, XYZ p)
        {
            double x = p.X;
            double y = p.Y;
            double x1 = a.X;
            double y1 = a.Y;
            double x2 = b.X;
            double y2 = b.Y;

            double A = x - x1;
            double B = y - y1;
            double C = x2 - x1;
            double D = y2 - y1;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;

            double param = -1;
            if (len_sq != 0)
            {
                //in case of 0 length line
                param = dot / len_sq;
            }

            double xx, yy;

            bool calcular;

            if (param < 0) //O ponto se encontra fora da linha e mais próximo do ponto inicial dela
            {
                xx = x1;
                yy = y1;

                calcular = false;
            }
            else if (param > 1) //O ponto se encontra fora da linha e mais próximo do ponto final dela
            {
                xx = x2;
                yy = y2;

                calcular = false;
            }
            else //O ponto se encontra próximo otogonal a linha e pode ser calculado
            {
                xx = x1 + param * C;
                yy = y1 + param * D;

                calcular = true;
            }

            var dx = x - xx;
            var dy = y - yy;


            if (calcular = true)
            {
                return Math.Sqrt(dx * dx + dy * dy);
            }
            else
            {
                return 1000;
            }

        }

        /// <summary>
        /// Projeta um ponto em uma linha
        /// </summary>
        /// <param name="line"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static XYZ ProjectPointIntoLine(Line line, XYZ p)
        {
            XYZ a = line.GetEndPoint(0);
            XYZ b = line.GetEndPoint(1);

            XYZ ap = p.Subtract(a);
            XYZ ab = b.Subtract(a);

            XYZ projectPoint = a + ap.DotProduct(ab) / ab.DotProduct(ab) * ab;

            return projectPoint;
        }

        /// <summary>
        /// Remove pontos duplicados de acordo com sua distância, melhor do que por coordenadas
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<XYZ> RemoveDuplicatedPointsByDistance(List<XYZ> points)
        {
            double tolerance = 0.00001;

            List<XYZ> result = points.Skip(1).Aggregate(points.Take(1).ToList(), (xys, xy) =>
            {
                if (xys.All(xy2 => xy.DistanceTo(xy2) >= tolerance))
                {
                    xys.Add(xy);
                }
                return xys;
            });

            return points;
        }

        #region SIMPLIFY LINES
        /// <summary>
        /// Apply methods to simplify lines, using Douglas-Peucker reduction and a combination of shift arrays to manipulate the points and allow to remove all the unecessary ones
        /// </summary>
        /// <param name="points">List of points to simplify</param>
        /// <param name="tolerance">Distance of tolerance</param>
        /// <returns></returns>
        public static List<XYZ> CurveSimplify(List<XYZ> points, double tolerance)
        {
            XYZ[] shiftedArray = points.ToArray();

            double toler = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);

            for (int i = 0; i < points.Count(); i++)
            {
                List<XYZ> pointss = shiftedArray.ToList();

                List<XYZ> newPoints = DouglasPeuckerReduction(pointss, toler);

                shiftedArray = newPoints.ToArray();

                shiftedArray = ListUtilities.ArrayShift(shiftedArray);
            }

            shiftedArray.Reverse();

            for (int i = 0; i < points.Count(); i++)
            {
                List<XYZ> pointss = shiftedArray.ToList();

                List<XYZ> newPoints = DouglasPeuckerReduction(pointss, toler);

                shiftedArray = newPoints.ToArray();

                shiftedArray = ListUtilities.ArrayShift(shiftedArray);
            }

            shiftedArray.Reverse();

            List<XYZ> newnewPoints = shiftedArray.ToList();

            return newnewPoints;
        }
        public static List<XYZ> DouglasPeuckerReduction(List<XYZ> Points, Double Tolerance)
        {
            if (Points == null || Points.Count < 3)
                return Points;

            Int32 firstPoint = 0;
            Int32 lastPoint = Points.Count - 1;
            List<Int32> pointIndexsToKeep = new List<Int32>();

            //Add the first and last index to the keepers
            pointIndexsToKeep.Add(firstPoint);
            pointIndexsToKeep.Add(lastPoint);

            //The first and the last point cannot be the same
            while (Points[firstPoint].Equals(Points[lastPoint]))
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(Points, firstPoint, lastPoint,
            Tolerance, ref pointIndexsToKeep);

            List<XYZ> returnPoints = new List<XYZ>();
            pointIndexsToKeep.Sort();
            foreach (Int32 index in pointIndexsToKeep)
            {
                returnPoints.Add(Points[index]);
            }

            return returnPoints;
        }
        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstPoint">The first point.</param>
        /// <param name="lastPoint">The last point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexsToKeep">The point index to keep.</param>
        private static void DouglasPeuckerReduction(List<XYZ> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep)
        {
            Double maxDistance = 0;
            Int32 indexFarthest = 0;

            for (Int32 index = firstPoint; index < lastPoint; index++)
            {
                Double distance = ComputeDistance(points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexsToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint,
                indexFarthest, tolerance, ref pointIndexsToKeep);
                DouglasPeuckerReduction(points, indexFarthest,
                lastPoint, tolerance, ref pointIndexsToKeep);
            }
        }
        #endregion
        /// <summary>
        /// Cria uma lista de curvas a partir de uma lista de pontos
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<Curve> CurveListFromPoints(List<XYZ> points)
        {
            List<Curve> listCurves = new List<Curve>();

            for (int i = 0; i < points.Count() - 1; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                listCurves.Add(line);
            }

            listCurves.Add(Line.CreateBound(points.Last(), points.First()));

            return listCurves;
        }
        /// <summary>
        /// Cria um CurveArray a partir de uma lista de curvas
        /// </summary>
        /// <param name="listCurve"></param>
        /// <returns></returns>
        public static CurveArray CurveArrayFromCurveList(List<Curve> listCurve)
        {
            CurveArray curveArray = new CurveArray();

            foreach (var item in listCurve)
            {
                curveArray.Append(item);
            }

            return curveArray;
        }
        /// <summary>
        /// Pega a geometria do elemento
        /// </summary>
        /// <param name="element"></param>
        /// <param name="geoOptions"></param>
        /// <returns></returns>
        public static GeometryElement GeometryFromElement(Element element, Options geoOptions)
        {
            GeometryElement geoElement = element.get_Geometry(geoOptions);

            return geoElement;
        }
        /// <summary>
        /// Opções de geometria padrão
        /// </summary>
        /// <returns></returns>
        public static Options GeometryOptionsFine()
        {
            //Options from the geometry
            Options geoOptions = new Options();
            geoOptions.DetailLevel = ViewDetailLevel.Fine;
            geoOptions.ComputeReferences = false;
            geoOptions.IncludeNonVisibleObjects = false;

            return geoOptions;
        }

        /// <summary>
        /// Get the intersection point between two lines
        /// the lines p1 --> p2 and p3 --> p4
        /// http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public static List<XYZ> FindIntersectionBetween2DLines(XYZ p1, XYZ p2, XYZ p3, XYZ p4)
        {
            //List to receive points
            List<XYZ> listPoints = new List<XYZ>(0);

            //Get the segment parameters
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p3.X - p4.X;
            double dy34 = p3.Y - p4.Y;

            //Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;

            if (double.IsInfinity(t1))
            {
                //The lines are parallel or very close to it
            }

            //lines_intersect = true;

            double t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / denominator;

            //Find the point of intersection
            var intersection = new XYZ(p1.X + dx12 * t1, p1.Y + dy12 * t1, p1.Z);

            //The segments intersect if t1 and t2 are between 0 and 1
            var segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            //Find the closest points on the segments
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            var close_p1 = new XYZ(p1.X + dx12 * t1, p1.Y + dy12 * t1, p1.Z);
            var close_p2 = new XYZ(p3.X + dx34 * t2, p3.Y + dy34 * t2, p1.Z);

            listPoints.Add(intersection);
            listPoints.Add(close_p1);
            listPoints.Add(close_p2);

            return listPoints;
        }

        /// <summary>
        /// Transformar pontos
        /// </summary>
        /// <param name="point"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }

        /// <summary>
        /// Projetar um ponto em um plano
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ ProjectPointOnPlane(Plane plane, XYZ point)
        {
            UV uv;
            double distance;

            plane.Project(point, out uv, out distance);

            XYZ projectPoint = plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;

            return projectPoint;
        }

        /// <summary>
        /// Projetar um ponto em um plano no eixo
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ ProjectPointOnPlaneInAxis(Plane plane, XYZ point, XYZ axis)
        {
            double distance = (point - plane.Origin).DotProduct(plane.Normal);

            double angle = plane.Normal.AngleTo(axis.Negate());

            double distanceAxis = distance / Math.Sin(Math.PI / 2 - angle);

            XYZ projectPoint = new XYZ(point.X, point.Y, point.Z + distanceAxis);

            return projectPoint;
        }

        public static CurveLoop PlanifyCurveLoop(CurveLoop curveLoop, Plane plane)
        {
            CurveLoop loop = new CurveLoop();

            foreach (Curve curve in curveLoop)
            {
                XYZ startPoint = ProjectPointOnPlaneInAxis(plane, curve.GetEndPoint(0), plane.Normal);
                XYZ endPoint = ProjectPointOnPlaneInAxis(plane, curve.GetEndPoint(1), plane.Normal);

                if (curve is Line)
                {
                    Line line = Line.CreateBound(startPoint, endPoint);
                    loop.Append(line);
                }
            }

            return loop;
        }

        /// <summary>
        /// Verifica se o ponto esta dentro do triângulo
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        /// 

        public static double Sign(XYZ v1, XYZ v2, XYZ v3)
        {
            return (v1.X - v3.X) * (v2.Y - v3.Y) - (v2.X - v3.X) * (v1.Y - v3.Y);
        }

        public static bool PointInTriangle(XYZ point, XYZ v1, XYZ v2, XYZ v3)
        {
            double d1, d2, d3;
            bool neg, pos;

            d1 = Sign(point, v1, v2);
            d2 = Sign(point, v2, v3);
            d3 = Sign(point, v3, v1);

            neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(neg && pos);
        }

        #region CONVEX HULL ALGORITHM

        /// <summary>
        /// Return the convex hull of a list of points 
        /// using the Jarvis march or Gift wrapping:
        /// https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
        /// Written by Maxence.
        /// </summary>

        public static List<XYZ> ConvexHull(List<XYZ> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            XYZ startPoint = points.MinBy(p => p.X);
            var convexHullPoints = new List<XYZ>();
            XYZ walkingPoint = startPoint;
            XYZ refVector = XYZ.BasisY.Negate();
            do
            {
                convexHullPoints.Add(walkingPoint);
                XYZ wp = walkingPoint;
                XYZ rv = refVector;
                walkingPoint = points.MinBy(p =>
                {
                    double angle = (p - wp).AngleOnPlaneTo(rv, XYZ.BasisZ);
                    if (angle < 1e-10) angle = 2 * Math.PI;
                    return angle;
                });
                refVector = wp - walkingPoint;
            } while (walkingPoint != startPoint);
            convexHullPoints.Reverse();
            return convexHullPoints;
        }

        #endregion
    }

    /// <summary>
    /// Método auxiliar para o algorítmo de ConvexHull
    /// https://thebuildingcoder.typepad.com/blog/2016/08/online-revit-api-docs-and-convex-hull.html?utm_source=feedburner&utm_medium=feed&utm_campaign=Feed%3A+typepad%2Fthe-building-coder+%28The+Building+Coder%29
    /// </summary>
    public static class IEnumerableExtensions
    {
        public static tsource MinBy<tsource, tkey>(
            this IEnumerable<tsource> source,
            Func<tsource, tkey> selector)
        {
            return source.MinBy(selector, Comparer<tkey>.Default);
        }
        public static tsource MinBy<tsource, tkey>(
          this IEnumerable<tsource> source,
          Func<tsource, tkey> selector,
          IComparer<tkey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            using (IEnumerator<tsource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                    throw new InvalidOperationException("Sequence was empty");
                tsource min = sourceIterator.Current;
                tkey minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    tsource candidate = sourceIterator.Current;
                    tkey candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }
    }

    /// <summary>
    /// Algoritimo para gerar um ConvexHull 2D a partir de uma lista de pontos
    /// Sem a necessidade de bibliotecas
    /// https://stackoverflow.com/questions/14671206/how-to-compute-convex-hull-in-c-sharp
    /// https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain
    /// </summary>
    public class ConvexHull
    {
        public static double cross(XYZ O, XYZ A, XYZ B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }
        public static List<XYZ> GetConvexHull(List<XYZ> points)
        {
            // Caso não tenha pontos retornar NULL
            if (points == null)
            {
                return null;
            }

            // É preciso ter ao menos 3 pontos para rodar o algoritimo
            if (points.Count() <= 1)
            {
                return points;
            }

            // Organiza os pontos pela coordenada X, caso tenha mais de um então organiza pela coordenada Y
            points.Sort((a,b) =>a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Inicia as listas U e L como vazias
            // Enquanto L contém pelo menos 2 pontos e a sequyência dos últimos pontos de L e o ponto P[i] não faz uma volta anti-horária
            // Remover o último ponto de L
            // Acrescenta P[i] para L
            int n = points.Count();
            int k = 0;
            List<XYZ> H = new List<XYZ>(new XYZ[2 * n]);

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            return H.Take(k - 1).ToList();
        }
    }
}
