using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
// Adicionados
using AYPluginsRevit.Apps.TopoTools;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Level = Autodesk.Revit.DB.Level;

namespace AYPluginsRevit.Apps.TopoTools
{
    public class TopoToolsHandler : IExternalEventHandler
    {
        // Instâncias do Request e da Janela
        public TopoToolsRequest TopoToolsRequest { get; set; }
        public TopoToolsWindow TopoToolsWindow { get; set; }

        public void Execute(UIApplication app)
        {
            switch (TopoToolsRequest)
            {
                case TopoToolsRequest.CreateExcavation:
                    CreateExcavation(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.SelectLevel:
                    SelectLevel(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.SelectElement:
                    SelectElement(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.AddElements:
                    AddElements(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.AddGroup:
                    AddGroup(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.AddIntersection:
                    AddIntersection(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.AddSpace:
                    AddSpace(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.CreateDifference:
                    CreateDifference(app.ActiveUIDocument);
                    break;
                case TopoToolsRequest.AddProjection:
                    AddProjection(app.ActiveUIDocument);
                    break;
                default:
                    break;
            }
        }

        public string GetName()
        {
            return "Winderson";
        }

        #region COMANDOS

        private void NomeDoComando(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

        }
        private void CreateDifference(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção de elementos
            ISelectionFilter filter = new Utilities.TopoSelectionFilter();
            Reference selection = uidoc.Selection.PickObject(ObjectType.Element, filter, "Selecione o elemento de escavação");

            if (selection != null)
            {
                List<Solid> lstIntersectionSolids = new List<Solid>();

                List<ElementGroup> elementGroups = new List<ElementGroup>();

                Element escavationEle = uidoc.Document.GetElement(selection.ElementId);
                BoundingBoxXYZ escavationBB = escavationEle.get_BoundingBox(null);
                double height = escavationBB.Max.Z - escavationBB.Min.Z;

                // Clone da observableCollection para não alterar a original
                foreach (ElementGroup eleGroup in TopoToolsWindow.intersections)
                {
                    elementGroups.Add((ElementGroup)eleGroup.Clone());
                }

                // Divide a lista em geoemtria e espaços
                List<ElementGroup> lstGeometry = new List<ElementGroup>();
                List<ElementGroup> lstSpaces = new List<ElementGroup>();
                List<ElementGroup> lstProjections = new List<ElementGroup>();

                foreach (ElementGroup eleGroup in elementGroups)
                {

                    if (eleGroup.Tipo == "Espaço")
                    {
                        lstSpaces.Add(eleGroup);
                    }
                    else if (eleGroup.Tipo == "Geometria")
                    {
                        lstGeometry.Add(eleGroup);
                    }
                    else if (eleGroup.Tipo == "Projeção")
                    {
                        lstProjections.Add(eleGroup);
                    }
                }

                // Pega as geometria das geometrias
                foreach (ElementGroup eleGroup in lstGeometry)
                {
                    foreach (Element ele in eleGroup.Elements)
                    {
                        List<Solid> solids = Utilities.GeometryUtilities.GetElementSolids(ele);

                        lstIntersectionSolids.AddRange(solids);
                    }
                }

                // Cria a geometria para as projeções
                // Vai considerar cada face inferior até o topo da geometria
                foreach (ElementGroup eleGroup in lstProjections)
                {
                    foreach (Element ele in eleGroup.Elements)
                    {
                        BoundingBoxXYZ bb = ele.get_BoundingBox(null);

                        Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, bb.Min);

                        List<Solid> lstEleSol = Utilities.GeometryUtilities.GetElementSolids(ele);

                        foreach (Solid sol in lstEleSol)
                        {
                            FaceArray solFaces = sol.Faces;

                            List<Face> faces = new List<Face>();

                            foreach (Face f in solFaces)
                            {
                                faces.Add(f);
                            }

                            List<Face> bottomFaces = GetBottomFaces(faces);

                            foreach (Face f in bottomFaces)
                            {
                                IList<CurveLoop> curveloops = f.GetEdgesAsCurveLoops();

                                IList<CurveLoop> ncurveLoops = new List<CurveLoop>();

                                foreach (CurveLoop curveloop in curveloops)
                                {
                                    ncurveLoops.Add(Utilities.GeometryUtilities.PlanifyCurveLoop(curveloop, plane));
                                }

                                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveloops, XYZ.BasisZ, height + 20);

                                if (solid != null)
                                {
                                    lstIntersectionSolids.Add(solid);
                                }
                            }
                        }
                    }
                }

                // Cria uma geometria para os espaços
                // Para cada grupo une os sólidos e cria um extrusion analyzer do mesmo
                List<CurveLoop> lstCurveLoops = new List<CurveLoop>();

                foreach (ElementGroup eleGroup in lstSpaces)
                {
                    List<Face> lstFaces = new List<Face>();

                    Outline outline = Utilities.GeometryUtilities.OutLineFromElementList(doc, eleGroup.Elements);

                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, outline.MinimumPoint);

                    foreach (Element ele in eleGroup.Elements)
                    {
                        List<Solid> lstEleSol = Utilities.GeometryUtilities.GetElementSolids(ele);

                        foreach (Solid sol in lstEleSol)
                        {
                            ExtrusionAnalyzer extruAnalyzer = ExtrusionAnalyzer.Create(sol, plane, XYZ.BasisZ);

                            Face face = extruAnalyzer.GetExtrusionBase();

                            lstFaces.Add(face);
                        }
                    }


                    // Adiciona a CurveLoop
                    List<XYZ> points = new List<XYZ>();

                    foreach (Face face in lstFaces)
                    {
                        foreach (CurveLoop cl in face.GetEdgesAsCurveLoops())
                        {
                            foreach (Curve c in cl)
                            {
                                points.Add(c.GetEndPoint(0));
                            }
                        }
                    }

                    CurveLoop convexHullLoop = CurveLoopConvexHull(points);

                    lstCurveLoops.Add(convexHullLoop);
                }

                // Cria os sólidos a partir das CurveLoops
                foreach (CurveLoop curveLoop in lstCurveLoops)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var c in curveLoop)
                    {
                        sb.AppendLine(c.GetEndPoint(0).ToString());
                        sb.AppendLine(c.GetEndPoint(1).ToString());
                    }

                    IList<CurveLoop> curves = new List<CurveLoop>();
                    Transform translation = Transform.CreateTranslation(new XYZ(0, 0, -10));
                    curves.Add(CurveLoop.CreateViaTransform(curveLoop, translation));

                    Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(curves, XYZ.BasisZ, height + 20);

                    if (solid != null)
                    {
                        lstIntersectionSolids.Add(solid);
                    }
                }

                // Cria um elemento a partir da intersecção entre dois sólidos
                Solid excavationSolid = Utilities.GeometryUtilities.GetElementSolids(escavationEle)[0];

                CreateSolidDifference(doc, excavationSolid, lstIntersectionSolids);
            }

        }
        private void AddSpace(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção dos elementos do grupo
            IList<Reference> selection = uidoc.Selection.PickObjects(ObjectType.Element, "Selecione os elementos para adicionar a intersecção");

            List<Element> lstElements = new List<Element>();

            foreach (Reference reference in selection)
            {
                Element ele = doc.GetElement(reference);

                lstElements.Add(ele);
            }

            ElementGroup eleGroup = new ElementGroup("Espaço", lstElements, lstElements.Count);

            TopoToolsWindow.intersections.Add(eleGroup);
        }
        private void AddProjection(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção dos elementos do grupo
            IList<Reference> selection = uidoc.Selection.PickObjects(ObjectType.Element, "Selecione os elementos para adicionar a intersecção");

            List<Element> lstElements = new List<Element>();

            foreach (Reference reference in selection)
            {
                Element ele = doc.GetElement(reference);

                lstElements.Add(ele);
            }

            ElementGroup eleGroup = new ElementGroup("Projeção", lstElements, lstElements.Count);

            TopoToolsWindow.intersections.Add(eleGroup);
        }
        private void AddIntersection(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção dos elementos do grupo
            IList<Reference> selection = uidoc.Selection.PickObjects(ObjectType.Element, "Selecione os elementos para adicionar a intersecção");

            List<Element> lstElements = new List<Element>();

            foreach (Reference reference in selection)
            {
                Element ele = doc.GetElement(reference);

                lstElements.Add(ele);
            }

            ElementGroup eleGroup = new ElementGroup("Geometria", lstElements, lstElements.Count);

            TopoToolsWindow.intersections.Add(eleGroup);
        }
        private void AddGroup(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção dos elementos do grupo
            IList<Reference> selection = uidoc.Selection.PickObjects(ObjectType.Element, "Selecione os elementos para adicionar a um grupo");

            List<Element> lstElements = new List<Element>();

            foreach (Reference reference in selection)
            {
                Element ele = doc.GetElement(reference);

                lstElements.Add(ele);
            }

            ElementGroup eleGroup = new ElementGroup("Grupo", lstElements, lstElements.Count);

            TopoToolsWindow.elementGroups.Add(eleGroup);
        }
        private void AddElements(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleção dos elementos do grupo
            List<Element> selection = uidoc.Selection.PickElementsByRectangle("Selecione os elementos a adicionar").ToList();

            ElementGroup eleGroup = new ElementGroup("Diversos", selection, selection.Count);

            TopoToolsWindow.elementGroups.Add(eleGroup);
        }
        private void CreateExcavation(UIDocument uidoc)
        {
            try
            {
                // CÓDIGO 
                Document doc = uidoc.Document;

                #region PEGA OS DADOS DA JANELA

                // Valores das janelas
                double factor = double.Parse(TopoToolsWindow.strFactor.Replace(',', '.'), CultureInfo.InvariantCulture);
                double levelElevation = double.Parse(TopoToolsWindow.strElevation.Replace(',', '.'), CultureInfo.InvariantCulture);
                double extraHeight = double.Parse(TopoToolsWindow.strExtraHeight.Replace(',', '.'), CultureInfo.InvariantCulture);
                double extralength = double.Parse(TopoToolsWindow.strExtraLenght.Replace(',', '.'), CultureInfo.InvariantCulture);
                int divisions = int.Parse(TopoToolsWindow.strDivisions);
                //bool considerOpenings = (bool)TopoToolsWindow.cb_openings.IsChecked;

                // Converte para as unidades internas do Revit
                levelElevation = UnitUtils.ConvertToInternalUnits(levelElevation, UnitTypeId.Meters);
                extraHeight = UnitUtils.ConvertToInternalUnits(extraHeight, UnitTypeId.Centimeters);
                extralength = UnitUtils.ConvertToInternalUnits(extralength, UnitTypeId.Centimeters);

                #endregion

                #region ORGANIZA OS ELEMENTOS EM ELEMENTOS ÚNICOS E DE GRUPO

                // Lista com todos os elementos criada a partir da ObservableCollection
                // Precisa ser dessa forma se não qualquer alteração na lista fica mantida na ObservableCollection
                List<ElementGroup> elementGroups = new List<ElementGroup>();

                foreach (ElementGroup eleGroup in TopoToolsWindow.elementGroups)
                {
                    elementGroups.Add((ElementGroup)eleGroup.Clone());
                }

                List<ElementGroup> diverseGroups = new List<ElementGroup>();
                List<ElementGroup> groupGroups = new List<ElementGroup>();
                List<ElementId> groupIds = new List<ElementId>();

                List<CurveLoop> lstCurveLoops = new List<CurveLoop>();

                foreach (ElementGroup eleGroup in elementGroups)
                {
                    if (eleGroup.Tipo == "Grupo")
                    {
                        groupGroups.Add(eleGroup);

                        foreach (var item in eleGroup.Elements)
                        {
                            groupIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        diverseGroups.Add(eleGroup);
                    }
                }

                // Remove da lista de elementos diversos os elementos que fazem parte dos grupos iterando ao contrário
                // para evitar erros

                foreach (ElementGroup eleGroup in diverseGroups)
                {
                    for (int i = eleGroup.Elements.Count - 1; i >= 0; i--)
                    {
                        Element ele = eleGroup.Elements[i];

                        if (groupIds.Contains(ele.Id))
                        {
                            eleGroup.Elements.Remove(ele);
                        }
                    }
                }

                // Adiciona os elementos a uma lista de elementos únicos
                List<Element> uniqueElements = new List<Element>();

                foreach (ElementGroup eleGroup in diverseGroups)
                {
                    uniqueElements.AddRange(eleGroup.Elements);
                }

                #endregion

                #region PEGA A GEOMETRIA DOS SÓLIDOS DOS ELEMENTOS E A PARTIR DELA AS FACES INFERIORES

                // Pega as faces dos elementos únicos
                foreach (ElementGroup eleGroup in diverseGroups)
                {
                    List<Face> lstFaces = new List<Face>();

                    foreach (Element ele in eleGroup.Elements)
                    {
                        List<Solid> solids = Utilities.GeometryUtilities.GetElementSolids(ele);

                        foreach (Solid solid in solids)
                        {
                            List<Face> eleFaces = new List<Face>();

                            FaceArray faceArr = solid.Faces;

                            foreach (Face face in faceArr)
                            {
                                eleFaces.Add(face);
                            }

                            List<Face> eleBottomFaces = GetBottomFaces(eleFaces);

                            lstFaces.AddRange(eleBottomFaces);

                            // Para cada face pegar as CurveLoops
                            foreach (Face face in eleBottomFaces)
                            {
                                IList<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops();
                                lstCurveLoops.Add(curveLoops.OrderBy(x => GetCurveLoopLength(x)).Last());
                            }
                        }
                    }
                }

                // Para cada grupo une os sólidos e cria um extrusion analyzer do mesmo
                foreach (ElementGroup eleGroup in groupGroups)
                {
                    List<Face> lstFaces = new List<Face>();

                    Outline outline = Utilities.GeometryUtilities.OutLineFromElementList(doc, eleGroup.Elements);

                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, outline.MinimumPoint);

                    foreach (Element ele in eleGroup.Elements)
                    {
                        List<Solid> lstEleSol = Utilities.GeometryUtilities.GetElementSolids(ele);

                        foreach (Solid sol in lstEleSol)
                        {
                            ExtrusionAnalyzer extruAnalyzer = ExtrusionAnalyzer.Create(sol, plane, XYZ.BasisZ);

                            Face face = extruAnalyzer.GetExtrusionBase();

                            lstFaces.Add(face);
                        }
                    }


                    // Adiciona a CurveLoop
                    List<XYZ> points = new List<XYZ>();

                    foreach (Face face in lstFaces)
                    {
                        foreach (CurveLoop cl in face.GetEdgesAsCurveLoops())
                        {
                            foreach (Curve c in cl)
                            {
                                points.Add(c.GetEndPoint(0));
                            }
                        }
                    }

                    CurveLoop convexHullLoop = CurveLoopConvexHull(points); // Para grupos cria um ConvexHull

                    // Cria linhas de modelo para conferência
                    List<Line> convexHullLines = new List<Line>();

                    foreach (var item in convexHullLoop)
                    {
                        convexHullLines.Add(item as Line);
                    }

                    //CreateModelLines(doc, convexHullLines);

                    lstCurveLoops.Add(convexHullLoop);
                }

                #endregion

                #region CRIA OS SÓLIDOS A PARTIR DAS GEOMETRIAS

                // Cria os sólidos a partir das CurveLoops
                List<Solid> blendSolids = new List<Solid>();

                foreach (CurveLoop curveLoop in lstCurveLoops)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var c in curveLoop)
                    {
                        sb.AppendLine(c.GetEndPoint(0).ToString());
                        sb.AppendLine(c.GetEndPoint(1).ToString());
                    }

                    //TaskDialog.Show("Teste", sb.ToString());

                    // Simplifica a curva removendo pontos excessivos
                    List<XYZ> loopPoints = new List<XYZ>();

                    foreach (Curve item in curveLoop)
                    {
                        loopPoints.Add(item.GetEndPoint(0));
                    }

                    List<XYZ> simplified = Utilities.GeometryUtilities.CurveSimplify(loopPoints, UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters));

                    List<Curve> simplifiedLines = Utilities.GeometryUtilities.CurveListFromPoints(simplified);

                    CurveLoop simplifiedCurveLoop = new CurveLoop();

                    foreach (var item in simplifiedLines)
                    {
                        simplifiedCurveLoop.Append(item);
                    }

                    Solid solid = CreateLoftSolidFromCurveLoop(doc, simplifiedCurveLoop, levelElevation, extraHeight, extralength, factor, divisions);

                    if (solid != null)
                    {
                        blendSolids.Add(solid);
                    }
                }

                #endregion

                #region UNE OS SÓLIDOS E CRIA UMA DIRECTSHAPE NO REVIT

                if ((bool)TopoToolsWindow.cb_separate.IsChecked)
                {
                    foreach (Solid solid in blendSolids)
                    {
                        CreateDirectShapeVolume(doc, solid);
                    }
                }
                else
                {
                    Solid finalSolid = Utilities.GeometryUtilities.JoinSolids(blendSolids);

                    CreateDirectShapeVolume(doc, finalSolid);
                }

                #endregion
            }
            catch (Exception e)
            {

                TaskDialog.Show("Erro", e.Message);
            }


        }
        private void CreateExcavationOLD(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Valores das janelas
            double factor = double.Parse(TopoToolsWindow.strFactor.Replace(',', '.'), CultureInfo.InvariantCulture);
            double levelElevation = double.Parse(TopoToolsWindow.strElevation.Replace(',', '.'), CultureInfo.InvariantCulture);
            double extraHeight = double.Parse(TopoToolsWindow.strExtraHeight.Replace(',', '.'), CultureInfo.InvariantCulture);
            double extralength = double.Parse(TopoToolsWindow.strExtraLenght.Replace(',', '.'), CultureInfo.InvariantCulture);

            // Converte para as unidades internas do Revit
            levelElevation = UnitUtils.ConvertToInternalUnits(levelElevation, UnitTypeId.Meters);
            extraHeight = UnitUtils.ConvertToInternalUnits(extraHeight, UnitTypeId.Centimeters);
            extralength = UnitUtils.ConvertToInternalUnits(extralength, UnitTypeId.Centimeters);

            // Seleciona os elementos de referência para criar os elementos de escavação
            IList<Element> listElements = uidoc.Selection.PickElementsByRectangle("Selecione os elementos para criar os volumes de escavação");
            List<Solid> createdSolids = new List<Solid>();

            // Itera sobre cada elemento pegando sua geometria e a partir dela as faces inferiores
            List<Face> lstBottomFaces = new List<Face>();
            try
            {
                if (listElements.Count > 0)
                {
                    // Une todos os sólidos dos elementos em um
                    List<Solid> lstSolids = new List<Solid>();

                    // Filtra as faces inferiores
                    foreach (Element ele in listElements)
                    {
                        lstSolids.AddRange(Utilities.GeometryUtilities.GetElementSolids(ele));
                    }

                    bool joinSolids = false;

                    if (joinSolids)
                    {
                        Solid joinedSolid = Utilities.GeometryUtilities.JoinSolids(lstSolids);

                        List<Face> lstJoinedFaces = Utilities.GeometryUtilities.GetSolidFace(joinedSolid);

                        lstBottomFaces.AddRange(GetBottomFaces(lstJoinedFaces));
                    }
                    else
                    {
                        foreach (Solid solid in lstSolids)
                        {
                            List<Face> lstJoinedFaces = Utilities.GeometryUtilities.GetSolidFace(solid);

                            lstBottomFaces.AddRange(GetBottomFaces(lstJoinedFaces));
                        }
                    }

                    // Para cada face inferior criar as curvas
                    foreach (Face face in lstBottomFaces)
                    {
                        Solid solid = CreateBlendSolidFromFace(doc, face, levelElevation, extraHeight, extralength, factor);

                        if (solid != null)
                        {
                            createdSolids.Add(solid);
                        }
                    }

                    // Une os sólidos em um único sólido
                    Solid joinedBlendSolid = Utilities.GeometryUtilities.JoinSolids(createdSolids);

                    CreateDirectShapeVolume(doc, joinedBlendSolid);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Erro", e.Message);
            }
        }
        private void SelectLevel(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleciona o level
            ISelectionFilter filter = new Utilities.LevelSelectionFilter();
            Reference selectedRef = uidoc.Selection.PickObject(ObjectType.Element, filter, "Selecione um nível");

            // Pega o Level e a partir dele sua elevação
            if (selectedRef != null)
            {
                Level level = doc.GetElement(selectedRef) as Level;

                double elevation = level.Elevation;

                // Preenche o valor da elevação na janela
                TopoToolsWindow.tb_terrainElevation.Text = Math.Round(UnitUtils.ConvertFromInternalUnits(elevation, UnitTypeId.Meters), 4).ToString("0.0000");
            }
        }
        private void SelectElement(UIDocument uidoc)
        {
            // CÓDIGO 
            Document doc = uidoc.Document;

            // Seleciona um elemento
            Reference selectedRef = uidoc.Selection.PickObject(ObjectType.Element, "Selecione um elemento");

            // Pega o Level e a partir dele sua elevação
            if (selectedRef != null)
            {
                Element ele = doc.GetElement(selectedRef);

                BoundingBoxXYZ bb = ele.get_BoundingBox(null);

                if (bb != null)
                {
                    // Posição do projeto
                    ProjectLocation projectLocation = doc.ActiveProjectLocation;
                    Transform transform = projectLocation.GetTotalTransform().Inverse;

                    XYZ transformedPoint = transform.OfPoint(bb.Max);

                    double elevation = transformedPoint.Z;

                    // Preenche o valor da elevação na janela
                    TopoToolsWindow.tb_terrainElevation.Text = Math.Round(UnitUtils.ConvertFromInternalUnits(elevation, UnitTypeId.Meters), 3).ToString();
                }

            }
        }
        private void CreateDirectShapeVolume(Document doc, Solid solid)
        {
            try
            {
                using (Transaction trans = new Transaction(doc, "Criar DirectShape"))
                {
                    trans.Start();

                    // Cria uma DirectShape a partir da lista de sólidos
                    IList<GeometryObject> lstGeoObj = new List<GeometryObject>();

                    lstGeoObj.Add(solid);

                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Topography));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(lstGeoObj);

                    Parameter dsparamForma = ds.LookupParameter("AY_CorteVolume");

                    if (dsparamForma != null)
                    {
                        dsparamForma.Set(solid.Volume);
                    }

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Erro", e.Message);
            }
        }
        private Solid CreateLoftSolidFromCurveLoop(Document doc, CurveLoop cl, double levelElevation, double extraHeight, double extralength, double factor, int divisions)
        {
            // Se a normal da face for para baixo inverter o sentido das curvas
            // Pega todas as curvas da CurveLoop e adiciona a uma lista de linhas

            IList<CurveLoop> curveLoops = new List<CurveLoop>();
            curveLoops.Add(cl);
            IList<CurveLoop> validCurveLoops = ExporterIFCUtils.ValidateCurveLoops(curveLoops, cl.GetPlane().Normal);

            CurveLoop curveloop = validCurveLoops.First();

            List<Line> lines = new List<Line>();

            //if (curveloop.GetPlane().Normal.Z < 0)
            //{
            //    foreach (Curve c in curveloop)
            //    {
            //        Curve nc = c.CreateReversed();

            //        lines.Add(Line.CreateBound(nc.GetEndPoint(0), nc.GetEndPoint(1)));
            //    }
            //}
            //else
            //{
            //    foreach (Curve c in curveloop)
            //    {
            //        lines.Add(Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1)));
            //    }
            //}

            if (extralength > 0)
            {
                CurveLoop offsetCurveLoop = CurveLoop.CreateViaOffset(curveloop, extralength, curveloop.GetPlane().Normal);

                curveloop = offsetCurveLoop;
            }

            if (curveloop.GetPlane().Normal.Z < 0)
            {
                curveloop.Flip();
            }

            foreach (Curve c in curveloop)
            {
                Line line = Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1));

                // Move a Curveloop para baixo de acordo com a altura extra
                if (extraHeight > 0)
                {
                    Transform translateExtraHeight = Transform.CreateTranslation(XYZ.BasisZ.Negate().Multiply(extraHeight));

                    lines.Add(line.CreateTransformed(translateExtraHeight) as Line);
                }
                else
                {
                    lines.Add(line);
                }
            }

            CurveLoop curveLoopB = new CurveLoop();

            foreach (var item in lines)
            {
                curveLoopB.Append(item);
            }

            curveloop = curveLoopB;

            // Itera sobre a lista de linhas
            List<XYZ> points = new List<XYZ>();

            // Nova lista de linhas
            List<Line> offsetLines = new List<Line>();
            List<List<XYZ>> divPoints = new List<List<XYZ>>();

            for (int i = 0; i < lines.Count(); i++)
            {
                Line line = lines[i];
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);
                XYZ lineDirection = line.Direction;
                XYZ basisZ = XYZ.BasisZ;
                XYZ lineSideVector = lineDirection.CrossProduct(basisZ).Normalize();

                double startHeight = levelElevation - GetPointLocation(doc, startPoint).Z;
                double endHeight = levelElevation - GetPointLocation(doc, endPoint).Z;

                if (startHeight + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && startHeight + extraHeight < 164 &&
                    endHeight + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && endHeight + extraHeight < 164)
                {
                    Transform translateZStartPoint = Transform.CreateTranslation(XYZ.BasisZ.Multiply(startHeight));
                    Transform translateZEndPoint = Transform.CreateTranslation(XYZ.BasisZ.Multiply(endHeight));

                    Transform translateSStartPoint = Transform.CreateTranslation(lineSideVector.Multiply(startHeight * factor));
                    Transform translateSEndPoint = Transform.CreateTranslation(lineSideVector.Multiply(endHeight * factor));

                    startPoint = translateZStartPoint.OfPoint(startPoint);
                    endPoint = translateZEndPoint.OfPoint(endPoint);

                    XYZ endpointline = endPoint; // Ponto no local superior mas sem o deslocamento horizontal, para ser usado posteriormente

                    startPoint = translateSStartPoint.OfPoint(startPoint);
                    endPoint = translateSEndPoint.OfPoint(endPoint);

                    // Dividindo
                    points.Add(startPoint);
                    points.Add(endPoint);

                    // Cria uma nova linha e armazena para ser verificado se alguma linha tem alguma intersecção
                    Line offLine = Line.CreateBound(startPoint, endPoint);
                    offsetLines.Add(offLine);

                    // Criação do ponto entre os pontos originais
                    XYZ firstDirection;
                    XYZ secondDirection;
                    XYZ resultVector;

                    if (i < lines.Count - 1)
                    {
                        firstDirection = line.Direction.Normalize().CrossProduct(XYZ.BasisZ);
                        secondDirection = lines[i + 1].Direction.Normalize().CrossProduct(XYZ.BasisZ);

                        resultVector = firstDirection.Add(secondDirection).Normalize();
                    }
                    else
                    {
                        firstDirection = line.Direction.Normalize().CrossProduct(XYZ.BasisZ);
                        secondDirection = lines[0].Direction.Normalize().CrossProduct(XYZ.BasisZ);

                        resultVector = firstDirection.Add(secondDirection).Normalize();
                    }


                    if (divisions > 0)
                    {
                        double angle = firstDirection.AngleOnPlaneTo(secondDirection, firstDirection.CrossProduct(secondDirection).Normalize());
                        double splitAngle = angle / (divisions + 1);

                        List<XYZ> dPoints = new List<XYZ>();

                        for (int j = 0; j < divisions; j++)
                        {
                            double currentAngle = splitAngle * (j + 1);
                            Transform rotateTransform = Transform.CreateRotation(firstDirection.CrossProduct(secondDirection), currentAngle);
                            XYZ rotatedVector = rotateTransform.OfVector(firstDirection);
                            Transform trans = Transform.CreateTranslation(rotatedVector.Multiply(endHeight * factor));
                            XYZ rotatedPoint = trans.OfPoint(endpointline);
                            points.Add(rotatedPoint);
                            dPoints.Add(rotatedPoint);
                        }

                        divPoints.Add(dPoints);
                    }

                }
            }



            // Loop para verificar se as linhas possuem intersecção
            // Lista final de pontos, agora sim deve ser a final
            List<XYZ> finalfinalPoints = new List<XYZ>();

            for (int i = 0; i < offsetLines.Count; i++)
            {
                bool intersect;
                bool segmentIntersect;
                XYZ intersection;
                XYZ closestPA;
                XYZ closestPB;

                if (i < offsetLines.Count - 1)
                {
                    Utilities.GeometryUtilities.Find2DLineIntersection(offsetLines[i], offsetLines[i + 1], out intersect, out segmentIntersect, out intersection, out closestPA, out closestPB);
                }
                else
                {
                    Utilities.GeometryUtilities.Find2DLineIntersection(offsetLines[i], offsetLines[0], out intersect, out segmentIntersect, out intersection, out closestPA, out closestPB);
                }


                if (i < offsetLines.Count - 1)
                {
                    if (segmentIntersect)
                    {
                        finalfinalPoints.Add(intersection);
                    }
                    else
                    {
                        finalfinalPoints.Add(offsetLines[i].GetEndPoint(1));

                        foreach (XYZ point in divPoints[i])
                        {
                            finalfinalPoints.Add(point);
                        }

                        finalfinalPoints.Add(offsetLines[i + 1].GetEndPoint(0));
                    }
                }
                else
                {
                    if (segmentIntersect)
                    {
                        finalfinalPoints.Add(intersection);
                    }
                    else
                    {
                        finalfinalPoints.Add(offsetLines[i].GetEndPoint(1));

                        foreach (XYZ point in divPoints[i])
                        {
                            finalfinalPoints.Add(point);
                        }

                        finalfinalPoints.Add(offsetLines[0].GetEndPoint(0));
                    }
                }
            }

            // Cria um novo CurveLoop a partir dos novos pontos
            List<Curve> lstLinesFinal = Utilities.GeometryUtilities.CurveListFromPoints(finalfinalPoints);


            // Cria linhas de modelo caso esteja a opção ativa
            if ((bool)TopoToolsWindow.cb_createLines.IsChecked)
            {
                List<Line> mlines = new List<Line>();

                foreach (var item in lstLinesFinal)
                {
                    mlines.Add(item as Line);
                }
                foreach (var item in curveloop)
                {
                    mlines.Add(item as Line);
                }

                CreateModelLines(doc, mlines);
            }


            using (Transaction trans = new Transaction(doc, "Criar linhas"))
            {
                trans.Start();


                //Cria linhas de modelo no local para conferência
                List<Line> modelLines = new List<Line>();
                modelLines.AddRange(lines);
                foreach (var item in lstLinesFinal)
                {
                    modelLines.Add(item as Line);
                }

                //foreach (var item in modelLines)
                //{
                //    Line lLine = Line.CreateBound(item.GetEndPoint(0), item.GetEndPoint(1));

                //    XYZ origin = lLine.Evaluate(0.5, true);

                //    XYZ lineX = lLine.Direction.Normalize();

                //    XYZ globalZ = XYZ.BasisZ;

                //    XYZ lineNormal = lineX.CrossProduct(XYZ.BasisZ);

                //    Plane plane = Plane.CreateByNormalAndOrigin(lineNormal, origin);

                //    SketchPlane sketch = SketchPlane.Create(doc, plane);

                //    ModelLine modelLine = doc.Create.NewModelCurve(lLine, sketch) as ModelLine;
                //}

                CurveLoop topcurves = CurveLoop.Create(lstLinesFinal);

                Solid blendSolid = GeometryCreationUtilities.CreateBlendGeometry(curveloop, topcurves, null);

                return blendSolid;
            }
        }
        private Solid CreateLoftSolidFromCurveLoopB(Document doc, CurveLoop cl, double levelElevation, double extraHeight, double extralength, double factor, int divisions)
        {
            // Se a normal da face for para baixo inverter o sentido das curvas
            // Pega todas as curvas da CurveLoop e adiciona a uma lista de linhas

            IList<CurveLoop> curveLoops = new List<CurveLoop>();
            curveLoops.Add(cl);
            IList<CurveLoop> validCurveLoops = ExporterIFCUtils.ValidateCurveLoops(curveLoops, cl.GetPlane().Normal);

            CurveLoop curveloop = validCurveLoops.First();

            List<Line> lines = new List<Line>();

            //if (curveloop.GetPlane().Normal.Z < 0)
            //{
            //    foreach (Curve c in curveloop)
            //    {
            //        Curve nc = c.CreateReversed();

            //        lines.Add(Line.CreateBound(nc.GetEndPoint(0), nc.GetEndPoint(1)));
            //    }
            //}
            //else
            //{
            //    foreach (Curve c in curveloop)
            //    {
            //        lines.Add(Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1)));
            //    }
            //}

            if (extralength > 0)
            {
                CurveLoop offsetCurveLoop = CurveLoop.CreateViaOffset(curveloop, extralength, curveloop.GetPlane().Normal);

                curveloop = offsetCurveLoop;
            }

            if (curveloop.GetPlane().Normal.Z < 0)
            {
                curveloop.Flip();
            }

            foreach (Curve c in curveloop)
            {
                Line line = Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1));

                // Move a Curveloop para baixo de acordo com a altura extra
                if (extraHeight > 0)
                {
                    Transform translateExtraHeight = Transform.CreateTranslation(XYZ.BasisZ.Negate().Multiply(extraHeight));

                    lines.Add(line.CreateTransformed(translateExtraHeight) as Line);
                }
                else
                {
                    lines.Add(line);
                }
            }

            CurveLoop curveLoopB = new CurveLoop();
            foreach (var item in lines)
            {
                curveLoopB.Append(item);
            }

            curveloop = curveLoopB;

            // Itera sobre a lista de linhas
            List<XYZ> points = new List<XYZ>();

            for (int i = 0; i < lines.Count(); i++)
            {
                Line line = lines[i];
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);
                XYZ lineDirection = line.Direction;
                XYZ basisZ = XYZ.BasisZ;
                XYZ lineSideVector = lineDirection.CrossProduct(basisZ).Normalize();

                double startHeight = levelElevation - GetPointLocation(doc, startPoint).Z;
                double endHeight = levelElevation - GetPointLocation(doc, endPoint).Z;

                if (startHeight + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && startHeight + extraHeight < 164 &&
                    endHeight + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && endHeight + extraHeight < 164)
                {
                    Transform translateZStartPoint = Transform.CreateTranslation(XYZ.BasisZ.Multiply(startHeight));
                    Transform translateZEndPoint = Transform.CreateTranslation(XYZ.BasisZ.Multiply(endHeight));

                    Transform translateSStartPoint = Transform.CreateTranslation(lineSideVector.Multiply(startHeight * factor));
                    Transform translateSEndPoint = Transform.CreateTranslation(lineSideVector.Multiply(endHeight * factor));

                    startPoint = translateZStartPoint.OfPoint(startPoint);
                    endPoint = translateZEndPoint.OfPoint(endPoint);

                    XYZ endpointline = endPoint; // Ponto no local superior mas sem o deslocamento horizontal, para ser usado posteriormente

                    startPoint = translateSStartPoint.OfPoint(startPoint);
                    endPoint = translateSEndPoint.OfPoint(endPoint);

                    // Criação do ponto entre os pontos originais
                    XYZ firstDirection;
                    XYZ secondDirection;
                    XYZ resultVector;

                    if (i < lines.Count - 1)
                    {
                        firstDirection = line.Direction.Normalize().CrossProduct(XYZ.BasisZ);
                        secondDirection = lines[i + 1].Direction.Normalize().CrossProduct(XYZ.BasisZ);

                        resultVector = firstDirection.Add(secondDirection).Normalize();
                    }
                    else
                    {
                        firstDirection = line.Direction.Normalize().CrossProduct(XYZ.BasisZ);
                        secondDirection = lines[0].Direction.Normalize().CrossProduct(XYZ.BasisZ);

                        resultVector = firstDirection.Add(secondDirection).Normalize();
                    }

                    // Dividindo
                    points.Add(startPoint);
                    points.Add(endPoint);

                    if (divisions > 0)
                    {
                        double angle = firstDirection.AngleOnPlaneTo(secondDirection, firstDirection.CrossProduct(secondDirection).Normalize());
                        double splitAngle = angle / (divisions + 1);

                        for (int j = 0; j < divisions; j++)
                        {
                            double currentAngle = splitAngle * (j + 1);
                            Transform rotateTransform = Transform.CreateRotation(firstDirection.CrossProduct(secondDirection), currentAngle);
                            XYZ rotatedVector = rotateTransform.OfVector(firstDirection);
                            Transform trans = Transform.CreateTranslation(rotatedVector.Multiply(endHeight * factor));
                            XYZ rotatedPoint = trans.OfPoint(endpointline);
                            points.Add(rotatedPoint);
                        }
                    }

                }
            }

            // Cria um novo CurveLoop a partir dos novos pontos
            List<Curve> lstLinesFinal = Utilities.GeometryUtilities.CurveListFromPoints(points);

            using (Transaction trans = new Transaction(doc, "Criar linhas"))
            {
                trans.Start();

                //Cria linhas de modelo no local para conferência
                List<Line> modelLines = new List<Line>();
                modelLines.AddRange(lines);
                foreach (var item in lstLinesFinal)
                {
                    modelLines.Add(item as Line);
                }

                //foreach (var item in modelLines)
                //{
                //    Line lLine = Line.CreateBound(item.GetEndPoint(0), item.GetEndPoint(1));

                //    XYZ origin = lLine.Evaluate(0.5, true);

                //    XYZ lineX = lLine.Direction.Normalize();

                //    XYZ globalZ = XYZ.BasisZ;

                //    XYZ lineNormal = lineX.CrossProduct(XYZ.BasisZ);

                //    Plane plane = Plane.CreateByNormalAndOrigin(lineNormal, origin);

                //    SketchPlane sketch = SketchPlane.Create(doc, plane);

                //    ModelLine modelLine = doc.Create.NewModelCurve(lLine, sketch) as ModelLine;
                //}

                CurveLoop topcurves = CurveLoop.Create(lstLinesFinal);

                Solid blendSolid = GeometryCreationUtilities.CreateBlendGeometry(curveloop, topcurves, null);

                return blendSolid;
            }
        }
        private Solid CreateBlendSolidFromFace(Document doc, Face face, double levelElevation, double extraHeight, double extralength, double factor)
        {
            XYZ point = GetPointLocation(doc, face.Evaluate(new UV(0.5, 0.5)));

            double faceElevation = point.Z;

            // A Partir da face inferior criar um BLEND com base na elevação do nível em comparação com a elevação da face
            double height = levelElevation - faceElevation;

            if (height + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && height + extraHeight < 164)
            {
                // Cria um novo CurveLoop a partir de um ConvexHull da face
                CurveLoop curveLoopA = face.GetEdgesAsCurveLoops().First();
                //CurveLoop curveLoopA = CurveLoopConvexHull(face);

                // Verifica se existe uma altura comprimento extra para ser adicionado a CurveloopA
                if (extralength > 0)
                {
                    curveLoopA = CurveLoop.CreateViaOffset(curveLoopA, extralength, face.ComputeNormal(new UV(0.5, 0.5)));
                }

                if (extraHeight > 0)
                {
                    height += extraHeight;

                    CurveLoop nCurveLoop = new CurveLoop();

                    foreach (Curve c in curveLoopA)
                    {
                        Transform translateA = Transform.CreateTranslation(XYZ.BasisZ.Negate().Multiply(extraHeight));
                        Curve newCurve = c.CreateTransformed(translateA);
                        nCurveLoop.Append(newCurve);
                    }

                    curveLoopA = nCurveLoop;
                }

                CurveLoop curveLoopB = new CurveLoop();

                foreach (Curve c in curveLoopA)
                {
                    Transform translate = Transform.CreateTranslation(XYZ.BasisZ.Multiply(height));
                    Curve newCurve = c.CreateTransformed(translate);
                    curveLoopB.Append(newCurve);
                }

                try
                {
                    CurveLoop curveLoopC = CurveLoop.CreateViaOffset(curveLoopB, -height * factor, XYZ.BasisZ);

                    Solid blendSolid = GeometryCreationUtilities.CreateBlendGeometry(curveLoopA, curveLoopC, null);

                    return blendSolid;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Erro", e.Message);

                    return null;
                }



            }
            else
            {
                return null;
            }
        }
        private Solid CreateBlendSolidFromCurveLoop(Document doc, CurveLoop curveLoop, double levelElevation, double extraHeight, double extralength, double factor)
        {
            XYZ point = GetPointLocation(doc, curveLoop.GetPlane().Origin);
            XYZ normal = curveLoop.GetPlane().Normal;
            double faceElevation = point.Z;

            // A Partir da face inferior criar um BLEND com base na elevação do nível em comparação com a elevação da face
            double height = levelElevation - faceElevation;

            if (height + extraHeight > UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters) && height + extraHeight < 164)
            {
                // Cria um novo CurveLoop a partir de um ConvexHull da face
                CurveLoop curveLoopA = curveLoop;
                //CurveLoop curveLoopA = CurveLoopConvexHull(face);

                // Verifica se existe uma altura comprimento extra para ser adicionado a CurveloopA
                if (extralength > 0)
                {
                    curveLoopA = CurveLoop.CreateViaOffset(curveLoopA, extralength, normal);
                }

                if (extraHeight > 0)
                {
                    height += extraHeight;

                    CurveLoop nCurveLoop = new CurveLoop();

                    foreach (Curve c in curveLoopA)
                    {
                        Transform translateA = Transform.CreateTranslation(XYZ.BasisZ.Negate().Multiply(extraHeight));
                        Curve newCurve = c.CreateTransformed(translateA);
                        nCurveLoop.Append(newCurve);
                    }

                    curveLoopA = nCurveLoop;
                }

                CurveLoop curveLoopB = new CurveLoop();

                foreach (Curve c in curveLoopA)
                {
                    Transform translate = Transform.CreateTranslation(XYZ.BasisZ.Multiply(height));
                    Curve newCurve = c.CreateTransformed(translate);
                    curveLoopB.Append(newCurve);
                }

                try
                {
                    CurveLoop curveLoopC = CurveLoop.CreateViaOffset(curveLoopB, height * factor, normal);

                    Solid blendSolid = GeometryCreationUtilities.CreateBlendGeometry(curveLoopA, curveLoopC, null);

                    return blendSolid;
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Erro", e.Message);

                    return null;
                }



            }
            else
            {
                return null;
            }
        }
        private void CreateSolidDifference(Document doc, Solid solid, List<Solid> lstSolids)
        {
            Solid joinedSolid = Utilities.GeometryUtilities.JoinSolids(lstSolids);

            Solid differenceSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, joinedSolid, BooleanOperationsType.Difference);

            CreateDirectShapeVolume(doc, differenceSolid);
        }
        public void SetFailureDialogSuppressor(Transaction tranasaction)
        {
            FailureHandlingOptions failOpt = tranasaction.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            tranasaction.SetFailureHandlingOptions(failOpt);
        }

        private void CreateModelLines(Document doc, List<Line> lines)
        {

            using (Transaction trans = new Transaction(doc, "Criar linhas de modelo"))
            {
                trans.Start();

                // Desativa as janelas de aviso
                SetFailureDialogSuppressor(trans);

                foreach (var item in lines)
                {
                    Line lLine = Line.CreateBound(item.GetEndPoint(0), item.GetEndPoint(1));

                    XYZ origin = lLine.Evaluate(0.5, true);

                    XYZ lineX = lLine.Direction.Normalize();

                    XYZ globalZ = XYZ.BasisZ;

                    XYZ lineNormal = lineX.CrossProduct(XYZ.BasisZ);

                    Plane plane = Plane.CreateByNormalAndOrigin(lineNormal, origin);

                    SketchPlane sketch = SketchPlane.Create(doc, plane);

                    ModelLine modelLine = doc.Create.NewModelCurve(lLine, sketch) as ModelLine;
                }

                trans.Commit();
            }
        }

        #endregion

        #region MÉTODOS AUXILIARES

        private XYZ GetPointLocation(Document doc, XYZ point)
        {
            // Posição do projeto
            ProjectLocation projectLocation = doc.ActiveProjectLocation;

            Transform transform = projectLocation.GetTotalTransform().Inverse;

            XYZ transformedPoint = transform.OfPoint(point);

            return transformedPoint;
        }
        private List<Face> GetBottomFaces(List<Face> lstFaces)
        {
            List<Face> lstBottomFaces = new List<Face>();

            foreach (Face face in lstFaces)
            {
                XYZ normal = face.ComputeNormal(new UV(0.5, 0.5));

                if (normal.IsAlmostEqualTo(XYZ.BasisZ.Negate()) || normal.Z < -0.5)
                {
                    lstBottomFaces.Add(face);
                }
            }

            return lstBottomFaces;
        }
        private CurveLoop CurveLoopConvexHull(List<XYZ> points)
        {
            List<XYZ> convexHullPoints = Utilities.ConvexHull.GetConvexHull(points);

            List<Curve> curves = Utilities.GeometryUtilities.CurveListFromPoints(convexHullPoints);

            CurveLoop nCurveLoop = new CurveLoop();

            foreach (Curve curve in curves)
            {
                nCurveLoop.Append(curve);
            }

            return nCurveLoop;
        }
        private double GetCurveLoopLength(CurveLoop curveLoop)
        {
            double length = 0;

            foreach (Curve c in curveLoop)
            {
                length += c.Length;
            }

            return length;
        }


        #endregion
    }

    public class WarningSwallower : IFailuresPreprocessor
    {
        /// <summary>
        /// Suprime as janelas de aviso
        /// https://forums.autodesk.com/t5/revit-api-forum/supressing-warning-pop-ups/td-p/4764741
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
        {
            IList<FailureMessageAccessor> failures = a.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                FailureDefinitionId id = f.GetFailureDefinitionId();

                FailureSeverity failureSeverity = a.GetSeverity();

                if (failureSeverity == FailureSeverity.Warning)   //simply catch all warnings, so you don't have to find out what warning is causing the message to pop up
                {
                    a.DeleteWarning(f);
                }
                else
                {
                    return FailureProcessingResult.ProceedWithRollBack;
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
