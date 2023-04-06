using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//Revit
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace AYPluginsRevit.Utilities
{
    public class FilterUtilities
    {
    }
    class StairSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Stairs) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class LevelSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Levels) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class LinkRVTSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_RvtLinks) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class TopoSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Topography) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WorkPlaneSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_CLines) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class LineSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

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
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class SketchSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors ||
                builtInCategory == BuiltInCategory.OST_Roofs ||
                builtInCategory == BuiltInCategory.OST_Ceilings) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class FloorSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class BuildingPadSelecionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_BuildingPad) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WallAllSelactionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Walls || builtInCategory == BuiltInCategory.OST_StackedWalls
                ) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WallSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Walls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WallSelectionFilterWithReferences : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Walls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class StackedWallSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_StackedWalls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class FloorRoofWallSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors || builtInCategory == BuiltInCategory.OST_Roofs || builtInCategory == BuiltInCategory.OST_Walls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class FloorWallSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors || builtInCategory == BuiltInCategory.OST_Walls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class BeamFloorSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors || builtInCategory == BuiltInCategory.OST_StructuralFraming) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WallBeamSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Walls || builtInCategory == BuiltInCategory.OST_StructuralFraming) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class BeamFloorConnectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Floors || builtInCategory == BuiltInCategory.OST_StructuralFraming || builtInCategory == BuiltInCategory.OST_StructConnections) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class BeamColumnSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_StructuralColumns || builtInCategory == BuiltInCategory.OST_StructuralFraming || builtInCategory == BuiltInCategory.OST_StructConnections) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class PartSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Parts) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class RoomSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Rooms) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class CeilingSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Ceilings) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class DoorsAndWindowsSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Doors || builtInCategory == BuiltInCategory.OST_Windows) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class CADSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            if (element is ImportInstance)
            {
                Element e = element.Document.GetElement(element.GetTypeId());
                if (e is CADLinkType)
                    return true;
            }
            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
    class DoorSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Doors) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class IFCSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_RvtLinks) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class WallGridSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            //Pega a BuiltInCategory do elemento
            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(element);

            //if statement for the wall object
            //if (builtInCategory == BuiltInCategory.OST_Rebar) return true;
            if (builtInCategory == BuiltInCategory.OST_Grids || builtInCategory == BuiltInCategory.OST_Walls) return true;

            return false;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
    class ElementSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            return true;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
    class SurfaceSelectionFilter : ISelectionFilter
    {
        //Sets up a boolean based on whether an object can be selection
        public bool AllowElement(Element element)
        {
            return true;
        }
        //sets up whether a reference can be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            if (refer.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE)
            {
                return true;
            }
            else
            {
                return false; 
            }
        }
    }
}
