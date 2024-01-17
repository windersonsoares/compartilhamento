using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AYRevit.Commands.Modify
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class SplitFloorByLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region DADOS DO REVIT

            // Pega o UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Pega o Document
            Document doc = uidoc.Document;

            // Pega a UIApplication
            UIApplication uiapp = new UIApplication(doc.Application);

            // Pega a Application
            Application app = uiapp.Application;

            #endregion

            #region SELEÇÃO DOS ELEMENTOS

            ISelectionFilter elementSelectionFilter = new ElementSelectionFilter();
            ISelectionFilter lineSelectionFilter = new LineSelectionFilter();

            Reference elementToSplit = uidoc.Selection.PickObject(ObjectType.Element, elementSelectionFilter);
            IList<Reference> splitLines = uidoc.Selection.PickObjects(ObjectType.Element, lineSelectionFilter);

            #endregion

            try
            {
                if (elementToSplit != null && splitLines != null && splitLines.Count > 0)
                {
                    #region CURVAS DO ELEMENTO A SER DIVIDIDO

                    Element element = doc.GetElement(elementToSplit);

                    ElementId typeId = element.GetTypeId();

                    // Nível do elemento
                    ElementId levelId = element.LevelId;

                    if (levelId == null)
                    {
                        BoundingBoxXYZ bb = element.get_BoundingBox(null);
                        XYZ bbCenter = (bb.Max + bb.Min) / 2;
                        levelId = Level.GetNearestLevelId(doc, bbCenter.Z);
                    }

                    Level level = doc.GetElement(levelId) as Level;

                    // Pega o croqui dos elementos
                    CurveArrArray curveArrArray = new CurveArrArray();

                    if (element is Floor floor)
                    {
                        // Curvas do piso
                        curveArrArray = (doc.GetElement(floor.SketchId) as Sketch).Profile;
                    }
                    else if (element is Ceiling ceiling)
                    {
                        // Curvas do forro
                        curveArrArray = (doc.GetElement(ceiling.SketchId) as Sketch).Profile;
                    }

                    // Transforma os CurveArray em CurveLoop
                    IList<CurveLoop> curveLoops = new List<CurveLoop>();

                    foreach (CurveArray curveArray in curveArrArray)
                    {
                        CurveLoop curveLoop = new CurveLoop();

                        foreach (Curve curve in curveArray)
                        {
                            curveLoop.Append(curve);
                        }

                        curveLoops.Add(curveLoop);
                    }

                    #endregion

                    #region CURVAS DE DIVISÃO

                    List<Element> splitElements = splitLines.Select(x => doc.GetElement(x)).ToList();

                    List<Curve> curves = new List<Curve>();

                    foreach (var item in splitElements)
                    {
                        if (item is DetailCurve detailCurve)
                        {
                            if (detailCurve.GeometryCurve.IsClosed)
                            {
                                Arc arc = detailCurve.GeometryCurve as Arc;

                                XYZ center = arc.Center;

                                XYZ startPoint = center.Add(new XYZ(-arc.Radius, 0, 0));
                                XYZ endPoint = center.Add(new XYZ(arc.Radius, 0, 0));
                                XYZ middlePoint1 = center.Add(new XYZ(0, arc.Radius, 0));
                                XYZ middlePoint2 = center.Add(new XYZ(0, -arc.Radius, 0));

                                curves.Add(Arc.Create(startPoint, endPoint, middlePoint1));
                                curves.Add(Arc.Create(endPoint, startPoint, middlePoint2));
                            }
                            else
                            {
                                curves.Add(detailCurve.GeometryCurve);
                            }
                        }
                        else if (item is ModelCurve modelCurve)
                        {
                            curves.Add(modelCurve.GeometryCurve);
                        }
                    }

                    List<List<Curve>> orderedCurves = OrderCurvesFromCurves(curves);

                    IList<CurveLoop> divideCurveLoops = new List<CurveLoop>();

                    foreach (var item in orderedCurves)
                    {
                        divideCurveLoops.Add(CurveLoop.Create(item));
                    }

                    #endregion

                    #region OBJETOS E LISTAS QUE SERÃO USADAS DEPOIS PARA CRIAR E DELETAR OS ELEMENTOS

                    // Transforma os CurveArray em CurveLoop
                    List<IList<CurveLoop>> dividedFloorCurveLoops = new List<IList<CurveLoop>>();

                    ElementId topoType = new FilteredElementCollector(doc)
                        .OfClass(typeof(ToposolidType))
                        .First().Id;

                    Toposolid toposolid;
                    List<ElementId> dividedTopoSolids = new List<ElementId>();

                    #endregion

                    #region TRANSAÇÕES

                    #region CRIA UM TOPOSOLID TEMPORARIAMENTE

                    using (Transaction trans = new Transaction(doc, "Criar toposolids"))
                    {
                        // Desativa as janelas de aviso
                        Utilities.AppUtilites.SetFailureDialogSuppressor(trans);

                        trans.Start();

                        toposolid = Toposolid.Create(doc, curveLoops, topoType, element.LevelId);

                        trans.Commit();
                    }

                    #endregion

                    #region DIVIDE O TOPOSOLID CRIADO E ADICIONA SUAS CURVAS A UMA LISTA PARA CRIAR OS PISOS

                    using (Transaction trans = new Transaction(doc, "Dividir TopoSolid"))
                    {
                        // Desativa as janelas de aviso
                        Utilities.AppUtilites.SetFailureDialogSuppressor(trans);

                        trans.Start();

                        IList<ElementId> resultTopo = toposolid.Split(divideCurveLoops);

                        foreach (ElementId eleId in resultTopo)
                        {
                            Toposolid dividedTopoSolid = doc.GetElement(eleId) as Toposolid;
                            CurveArrArray dividedArray = (doc.GetElement(dividedTopoSolid.SketchId) as Sketch).Profile;
                            IList<CurveLoop> currentFloorCurveLoop = new List<CurveLoop>();

                            foreach (CurveArray curveArray in dividedArray)
                            {
                                CurveLoop curveLoop = new CurveLoop();

                                foreach (Curve curve in curveArray)
                                {
                                    curveLoop.Append(curve);
                                }

                                currentFloorCurveLoop.Add(curveLoop);
                            }

                            dividedFloorCurveLoops.Add(currentFloorCurveLoop);
                        }

                        trans.RollBack();
                    }

                    #endregion

                    #region CRIA OS NOVOS PISOS E APAGA OS ELEMENTOS ANTERIORES

                    using (Transaction trans = new Transaction(doc, "Criar pisos"))
                    {
                        // Desativa as janelas de aviso
                        Utilities.AppUtilites.SetFailureDialogSuppressor(trans);

                        trans.Start();

                        // Apaga os items criados anteriormente
                        doc.Delete(element.Id);
                        doc.Delete(toposolid.Id);
                        foreach (var item in dividedTopoSolids)
                        {
                            doc.Delete(item);
                        }

                        // Cria os pisos
                        foreach (IList<CurveLoop> loops in dividedFloorCurveLoops)
                        {
                            Floor dividedFloor = Floor.Create(doc, loops, typeId, level.Id);
                        }

                        trans.Commit();
                    }

                    #endregion

                    #endregion
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed; ;
            }
        }

        private List<List<Curve>> OrderCurvesFromCurves(List<Curve> curves)
        {
            List<List<Curve>> orderedLoops = new List<List<Curve>>();

            // Retorna caso não tenha curvas em sequência ou seja apenas uma
            if (curves == null || curves.Count <= 1)
            {
                orderedLoops.Add(curves);

                return orderedLoops;
            }

            // Itera sobre as curvas procurando as curvas sequenciais
            List<Curve> remainingCurves = new List<Curve>(curves);

            while (remainingCurves.Count > 0)
            {
                List<Curve> orderedCurves = new List<Curve>();
                Curve startCurve = remainingCurves[0];
                orderedCurves.Add(startCurve);
                remainingCurves.Remove(startCurve);


                // Verifica se existe pelo menos uma conectada a curva atual
                bool hasConnectedCurve = remainingCurves.Any(curve =>
                    curve.GetEndPoint(0).IsAlmostEqualTo(startCurve.GetEndPoint(1)) ||
                    curve.GetEndPoint(1).IsAlmostEqualTo(startCurve.GetEndPoint(1)));

                if (!hasConnectedCurve)
                {
                    // Move para a próxima curva caso nenhuma curva for encontrada
                    continue;
                }

                while (remainingCurves.Count > 0)
                {
                    XYZ endPoint = orderedCurves.Last().GetEndPoint(1);

                    Curve nextCurve = null;
                    foreach (Curve curve in remainingCurves)
                    {
                        if (curve.GetEndPoint(0).IsAlmostEqualTo(endPoint))
                        {
                            nextCurve = curve;
                            break;
                        }
                        else if (curve.GetEndPoint(1).IsAlmostEqualTo(endPoint))
                        {
                            nextCurve = curve.CreateReversed();
                            break;
                        }
                    }

                    if (nextCurve != null)
                    {
                        orderedCurves.Add(nextCurve);
                        remainingCurves.Remove(nextCurve);
                    }
                    else
                    {
                        break; // Não existem mais curvas na sequência
                    }
                }

                orderedLoops.Add(orderedCurves);
            }

            return orderedLoops;
        }
    }

    class ElementSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsLong(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors ||
                builtInCategory == BuiltInCategory.OST_Ceilings) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private long GetCategoryIdAsLong(Element element)
        {
            return element?.Category?.Id?.Value ?? -1;
        }
    }

    class LineSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsLong(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Lines) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private long GetCategoryIdAsLong(Element element)
        {
            return element?.Category?.Id?.Value ?? -1;
        }
    }
}