using System;

public class PDMSOBject
{
    public Class1()
    {
    }
}

#region Models

public class PDMSStructure
{
    public string Name { get; set; }
    public string Purpose { get; set; }
    public double PositionE { get; set; }
    public double PositionN { get; set; }
    public double PositionU { get; set; }
    public double Angle { get; set; }
    public char OrientationY { get; set; }
    public char OrientationZ { get; set; }

    public PDMSStructure()
    {
    }
    public PDMSStructure(string name)
    {
        Name = name;
    }

    public PDMSStructure(string name, string purpose, double positionE, double positionN, double positionU)
    {
        Name = name;
        Purpose = purpose;
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
    }
    public PDMSStructure(string name, string purpose, double positionE, double positionN, double positionU, double angle, char orientationY, char orientationZ)
    {
        Name = name;
        Purpose = purpose;
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        Angle = angle;
        OrientationY = orientationY;
        OrientationZ = orientationZ;
    }
}

public interface IPDMSGeometry
{
    public double PositionE { get; set; }
    public double PositionN { get; set; }
    public double PositionU { get; set; }
}

public class PDMSBox : IPDMSGeometry
{
    public double PositionE { get; set; }
    public double PositionN { get; set; }
    public double PositionU { get; set; }
    public double LengthX { get; set; }
    public double LengthY { get; set; }
    public double LengthZ { get; set; }
    public double Angle { get; set; }
    public char OrientationY { get; set; }
    public char OrientationZ { get; set; }
    public bool Cut { get; set; }
    public PDMSBox()
    {
    }
    public PDMSBox(double positionE, double positionN, double positionU, double lengthX, double lengthY, double lengthZ, bool cut)
    {
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
        Cut = cut;
    }
    public PDMSBox(double positionE, double positionN, double positionU, double lengthX, double lengthY, double lengthZ, double angle, char orientationY, char orientationZ, bool cut)
    {
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
        Angle = angle;
        OrientationY = orientationY;
        OrientationZ = orientationZ;
        Cut = cut;
    }
}

public class PDMSPyramid : IPDMSGeometry
{
    public double PositionE { get; set; }
    public double PositionN { get; set; }
    public double PositionU { get; set; }
    public double LengthBottomX { get; set; }
    public double LengthBottomY { get; set; }
    public double LengthTopX { get; set; }
    public double LengthTopY { get; set; }
    public double Height { get; set; }
    public double Angle { get; set; }
    public char OrientationY { get; set; }
    public char OrientationZ { get; set; }
    public bool Cut { get; set; }
    public PDMSPyramid()
    {
    }
    public PDMSPyramid(double positionE, double positionN, double positionU, double lengthBottomX, double lengthBottomY, double lengthTopX, double lengthTopY, double height, bool cut)
    {
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        LengthBottomX = lengthBottomX;
        LengthBottomY = lengthBottomY;
        LengthTopX = lengthTopX;
        LengthTopY = lengthTopY;
        Height = height;
        Cut = cut;
    }
    public PDMSPyramid(double positionE, double positionN, double positionU, double lengthBottomX, double lengthBottomY, double lengthTopX, double lengthTopY, double height, double angle, char orientationY, char orientationZ, bool cut)
    {
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        LengthBottomX = lengthBottomX;
        LengthBottomY = lengthBottomY;
        LengthTopX = lengthTopX;
        LengthTopY = lengthTopY;
        Height = height;
        Angle = angle;
        OrientationY = orientationY;
        OrientationZ = orientationZ;
        Cut = cut;
    }
}

public class PDMSCylinder : IPDMSGeometry
{
    public double PositionE { get; set; }
    public double PositionN { get; set; }
    public double PositionU { get; set; }
    public double Diameter { get; set; }
    public double Height { get; set; }
    public bool Cut { get; set; }
    public PDMSCylinder()
    {
    }
    public PDMSCylinder(double positionE, double positionN, double positionU, double diameter, double height, bool cut)
    {
        PositionE = positionE;
        PositionN = positionN;
        PositionU = positionU;
        Diameter = diameter;
        Height = height;
        Cut = cut;
    }
}

#endregion

#endregion Methods

public class PDMSUtilities
{
    public static void CreatePDMSCone(double altura, double botDiameter, double topDiameter, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        botDiameter = Math.Round(botDiameter, 3);
        topDiameter = Math.Round(topDiameter, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        // Caso seja uma forma de corte então é NBOX
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NCONE");
        }
        else
        {
            stringBuilder.AppendLine("NEW CONE");
        }

        // Adiciona a posição do elemento
        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));

        // Adiciona as dimensões do elemento
        stringBuilder.AppendLine(String.Format("DTOP {0}mm", topDiameter));
        stringBuilder.AppendLine(String.Format("DBOT {0}mm", botDiameter));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));
        stringBuilder.AppendLine();

    }

    public static void CreatePDMSCylinder(double altura, double diameter, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        diameter = Math.Round(diameter, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        // Caso seja uma forma de corte então é NBOX
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NCYLINDER");
        }
        else
        {
            stringBuilder.AppendLine("NEW CYLINDER");
        }

        // Adiciona a posição do elemento
        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));

        // Adiciona as dimensões do elemento
        stringBuilder.AppendLine(String.Format("DIAM {0}mm", diameter));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));
        stringBuilder.AppendLine();

    }

    public static void CreatePDMSBox(double altura, double comprimentoX, double comprimentoY, double angle, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        comprimentoX = Math.Round(comprimentoX, 1);
        comprimentoY = Math.Round(comprimentoY, 1);
        //double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 1);
        //double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 1);
        //double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 1);

        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters) * 2) / 2;
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters) * 2) / 2;
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters) * 2) / 2;

        // Caso seja uma forma de corte então é NBOX
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NBOX");
        }
        else
        {
            stringBuilder.AppendLine("NEW BOX");
        }

        // Adiciona a posição do elemento
        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));

        // Adiciona a orientação do elemento, evitar pois depende do ângulo, melhor apenas rotacionar posteriormente
        /*
        if (angle != 0 && angle != 180)
        {
            stringBuilder.AppendLine(String.Format("ORI Y is N {0} E and Z is U", angle));
        }
        */

        // Adiciona as dimensões do elemento
        stringBuilder.AppendLine(String.Format("XLEN {0}mm", comprimentoX));
        stringBuilder.AppendLine(String.Format("YLEN {0}mm", comprimentoY));
        stringBuilder.AppendLine(String.Format("ZLEN {0}mm", altura));
        stringBuilder.AppendLine();

        // Rotaciona o elemento
        if (angle != 0)
        {
            stringBuilder.AppendLine(String.Format("Rotate by {0}", angle));
            stringBuilder.AppendLine();
        }



    }

    public static void CreatePDMSPyramid(double altura, double botComprimentoX, double botComprimentoY, double topComprimentoX, double topComprimentoY,
        double xOffset, double yOffset, double angle, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        botComprimentoX = Math.Round(botComprimentoX, 1);
        botComprimentoY = Math.Round(botComprimentoY, 1);
        topComprimentoX = Math.Round(topComprimentoX, 1);
        topComprimentoY = Math.Round(topComprimentoY, 1);
        xOffset = Math.Round(xOffset, 1);
        yOffset = Math.Round(yOffset, 1);
        //double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 1);
        //double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 1);
        //double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 1);

        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters) * 2) / 2;
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters) * 2) / 2;
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters) * 2) / 2;

        // Caso seja uma forma de corte então é NPYRAMID
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NPYRAMID");
        }
        else
        {
            stringBuilder.AppendLine("NEW PYRAMID");
        }

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));

        if (angle != 0 && angle != 180)
        {
            stringBuilder.AppendLine(String.Format("ORI Y is N {0} E and Z is U", angle));
        }


        stringBuilder.AppendLine(String.Format("XBOT {0}mm", botComprimentoX));
        stringBuilder.AppendLine(String.Format("YBOT {0}mm", botComprimentoY));
        stringBuilder.AppendLine(String.Format("XTOP {0}mm", topComprimentoX));
        stringBuilder.AppendLine(String.Format("YTOP {0}mm", topComprimentoY));
        stringBuilder.AppendLine(String.Format("XOFF {0}mm", xOffset));
        stringBuilder.AppendLine(String.Format("YOFF {0}mm", yOffset));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));
        stringBuilder.AppendLine();
    }

    public static void CreatePDMSPyramidHorizontal(double altura, double botComprimentoX, double botComprimentoY, double topComprimentoX, double topComprimentoY,
double xOffset, double yOffset, double angle, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        botComprimentoX = Math.Round(botComprimentoX, 1);
        botComprimentoY = Math.Round(botComprimentoY, 1);
        topComprimentoX = Math.Round(topComprimentoX, 1);
        topComprimentoY = Math.Round(topComprimentoY, 1);
        xOffset = Math.Round(xOffset, 1);
        yOffset = Math.Round(yOffset, 1);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 1);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 1);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 1);

        // Caso seja uma forma de corte então é NPYRAMID
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NPYRAMID");
        }
        else
        {
            stringBuilder.AppendLine("NEW PYRAMID");
        }

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));
        stringBuilder.AppendLine("ORI Y is N and Z is E");

        stringBuilder.AppendLine(String.Format("XBOT {0}mm", botComprimentoX));
        stringBuilder.AppendLine(String.Format("YBOT {0}mm", botComprimentoY));
        stringBuilder.AppendLine(String.Format("XTOP {0}mm", topComprimentoX));
        stringBuilder.AppendLine(String.Format("YTOP {0}mm", topComprimentoY));
        stringBuilder.AppendLine(String.Format("XOFF {0}mm", xOffset));
        stringBuilder.AppendLine(String.Format("YOFF {0}mm", yOffset));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));

        if (angle != 0)
        {
            stringBuilder.AppendLine(String.Format("ROTATE BY {0} ABOUT U", angle));
        }


        stringBuilder.AppendLine();
    }

    public static void CreatePDMSExtrusion(double altura, List<XYZ> lstPoints, XYZ center, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 1);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 1);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 1);

        // Caso seja uma forma de corte então é NPYRAMID
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NXTRUSION");
        }
        else
        {
            stringBuilder.AppendLine("NEW EXTRUSION");
        }

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", 0, 0, centerZ));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW LOOP");

        foreach (XYZ point in lstPoints)
        {
            double px = Math.Round(UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Millimeters), 1);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Millimeters), 1);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Millimeters), 1);

            stringBuilder.AppendLine("NEW VERTEX");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", px, pY, pZ));
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();

    }

    public static void CreatePDMSExtrusionRef(double altura, List<XYZ> lstPoints, XYZ center, XYZ refPoint, StringBuilder stringBuilder, bool cortar)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        center = center - refPoint;
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 1);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 1);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 1);

        // Caso seja uma forma de corte então é NPYRAMID
        if (cortar)
        {
            stringBuilder.AppendLine("NEW NXTRUSION");
        }
        else
        {
            stringBuilder.AppendLine("NEW EXTRUSION");
        }

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", 0, 0, centerZ));
        stringBuilder.AppendLine(String.Format("HEIG {0}mm", altura));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW LOOP");

        foreach (XYZ point in lstPoints)
        {
            XYZ newPoint = point - refPoint;

            double px = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.X, UnitTypeId.Millimeters), 1);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Y, UnitTypeId.Millimeters), 1);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Z, UnitTypeId.Millimeters), 1);

            stringBuilder.AppendLine("NEW VERTEX");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", px, pY, pZ));
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();

    }

    public static void CreatePDMSFloor(double altura, List<XYZ> lstPoints, XYZ center, StringBuilder stringBuilder)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        stringBuilder.AppendLine("NEW FLOOR");

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", 0, 0, centerZ));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW PLOOP");
        stringBuilder.AppendLine(String.Format("HEIG {0}", altura));
        stringBuilder.AppendLine("SJUS UTOP");

        foreach (XYZ point in lstPoints)
        {
            double px = Math.Round(UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Millimeters), 3);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Millimeters), 3);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Millimeters), 3);

            stringBuilder.AppendLine("NEW PAVERT");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", px, pY, pZ));
            stringBuilder.AppendLine("END");
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();

    }

    public static void CreatePDMSGWall(double altura, List<XYZ> lstPoints, XYZ center, StringBuilder stringBuilder)
    {
        // Arredonda os valores
        altura = Math.Round(UnitUtils.ConvertFromInternalUnits(altura, UnitTypeId.Millimeters), 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        stringBuilder.AppendLine("NEW GWALL");
        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", 0, 0, centerZ));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW PLOOP");
        stringBuilder.AppendLine(String.Format("HEIG {0}", altura));
        stringBuilder.AppendLine("SJUS DBOT");

        foreach (XYZ point in lstPoints)
        {
            double px = Math.Round(UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Millimeters), 3);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Millimeters), 3);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Millimeters), 3);

            stringBuilder.AppendLine("NEW PAVERT");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", px, pY, pZ));
            stringBuilder.AppendLine("END");
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();
    }

    public static void CriarNichos(Element ele, Document doc, XYZ referencePoint, StringBuilder stringBuilder)
    {
        ElementId typeVId = ele.GetTypeId();

        ElementType typeV = doc.GetElement(typeVId) as ElementType;

        string famNameV = typeV.FamilyName;

        BoundingBoxXYZ bbv = ele.get_BoundingBox(doc.ActiveView);

        XYZ bbvMax = bbv.Max;
        XYZ bbvMin = bbv.Min;

        XYZ bbvCenter = (bbvMax + bbvMin) / 2;

        bbvCenter = bbvCenter - referencePoint;

        //Caso seja uma FamilyInstance então tem rotação
        FamilyInstance vInstance = ele as FamilyInstance;

        XYZ rotation = new XYZ();
        double angleRadians = 0;
        double angleDegrees = 0;

        if (vInstance != null)
        {
            rotation = vInstance.FacingOrientation;
            angleRadians = rotation.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);
            angleDegrees = (180 / Math.PI) * angleRadians;
            angleDegrees = Math.Round(angleDegrees, 2);
        }


        if (famNameV == "Nicho caixa")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter NichoComprimentoParam = ele.LookupParameter("Comprimento");
            Parameter nichoLarguraParam = ele.LookupParameter("Largura");

            double xlen = UnitUtils.ConvertFromInternalUnits(nichoLarguraParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);

            xlen = Math.Round(xlen, 3);
            ylen = Math.Round(ylen, 3);
            zlen = Math.Round(zlen, 3);

            CreatePDMSBox(zlen, xlen, ylen, angleDegrees, bbvCenter, stringBuilder, true);
        }
        else if (famNameV == "Nicho pirâmide")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter NichoComprimentoParam = ele.LookupParameter("Comprimento");
            Parameter NichoComprimentoTParam = ele.LookupParameter("ComprimentoT");
            Parameter nichoLarguraParam = ele.LookupParameter("Largura");
            Parameter nichoLarguraTParam = ele.LookupParameter("LarguraT");

            double xlen = UnitUtils.ConvertFromInternalUnits(nichoLarguraParam.AsDouble(), UnitTypeId.Millimeters);
            double xlenT = UnitUtils.ConvertFromInternalUnits(nichoLarguraTParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double ylenT = UnitUtils.ConvertFromInternalUnits(NichoComprimentoTParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);

            xlen = Math.Round(xlen, 3);
            xlenT = Math.Round(xlenT, 3);
            ylen = Math.Round(ylen, 3);
            ylenT = Math.Round(ylenT, 3);
            zlen = Math.Round(zlen, 3);

            CreatePDMSPyramid(zlen, xlen, ylen, xlenT, ylenT, 0, 0, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "Cilindro genérico")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter nichoDiâmetroParam = ele.LookupParameter("Diâmetro");

            double nichoAltura = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);
            double nichoDiametro = UnitUtils.ConvertFromInternalUnits(nichoDiâmetroParam.AsDouble(), UnitTypeId.Millimeters);

            nichoAltura = Math.Round(nichoAltura, 3);
            nichoDiametro = Math.Round(nichoDiametro, 3);

            CreatePDMSCylinder(nichoAltura, nichoDiametro, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "Corte de canto triangular")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("HEIG");
            Parameter NichoComprimentoParam = ele.LookupParameter("YTOP");
            Parameter NichoComprimentoTParam = ele.LookupParameter("YBOT");
            Parameter nichoLarguraTParam = ele.LookupParameter("XTOP");
            Parameter xoffParam = ele.LookupParameter("XOFF");

            double xlen = 0;
            double xlenT = UnitUtils.ConvertFromInternalUnits(nichoLarguraTParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double ylenT = UnitUtils.ConvertFromInternalUnits(NichoComprimentoTParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);
            double xoff = UnitUtils.ConvertFromInternalUnits(xoffParam.AsDouble(), UnitTypeId.Millimeters);

            xlenT = Math.Round(xlenT, 3);
            ylen = Math.Round(ylen, 3);
            ylenT = Math.Round(ylenT, 3);
            zlen = Math.Round(zlen, 3);
            xoff = Math.Round(xoff, 3);

            angleDegrees = angleDegrees + 180;

            CreatePDMSPyramid(zlen, xlen, ylen, xlenT, ylenT, xoff, 0, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "PDMSBox")
        {
            Parameter alturaParam = ele.LookupParameter("ZLEN");
            Parameter larguraParam = ele.LookupParameter("XLEN");
            Parameter comprimentoParam = ele.LookupParameter("YLEN");


            double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
            double largura = UnitUtils.ConvertFromInternalUnits(larguraParam.AsDouble(), UnitTypeId.Millimeters);
            double comprimento = UnitUtils.ConvertFromInternalUnits(comprimentoParam.AsDouble(), UnitTypeId.Millimeters);

            // Arredonda os valores
            altura = Math.Round(altura, 2);
            largura = Math.Round(largura, 2);
            comprimento = Math.Round(comprimento, 2);

            CreatePDMSBox(altura, largura, comprimento, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "PDMSCylinder")
        {
            Parameter alturaParam = ele.LookupParameter("HEIG");
            Parameter diametroParam = ele.LookupParameter("DIAM");


            double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
            double largura = UnitUtils.ConvertFromInternalUnits(diametroParam.AsDouble(), UnitTypeId.Millimeters);

            // Arredonda os valores
            altura = Math.Round(altura, 2);
            largura = Math.Round(largura, 2);

            CreatePDMSCylinder(altura, largura, bbvCenter, stringBuilder, true);
        }
        else if (ele is Floor)
        {
            Floor floor = ele as Floor;

            Parameter vparamThickness = ele.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);

            double vpisothickness = vparamThickness.AsDouble();

            double vpisoaltura = Math.Round(UnitUtils.ConvertFromInternalUnits(vpisothickness, UnitTypeId.Millimeters), 3);

            List<XYZ> vfloorPoints = new List<XYZ>();

            BoundingBoxXYZ vpisoBb = ele.get_BoundingBox(null);

            XYZ vbbcenter = (vpisoBb.Max + vpisoBb.Min) / 2;

            vbbcenter = new XYZ(vbbcenter.X, vbbcenter.Y, vbbcenter.Z - referencePoint.Z - vpisothickness / 2);

            CurveArrArray vcurveArrArray = (doc.GetElement(floor.SketchId) as Sketch).Profile;

            foreach (CurveArray caa in vcurveArrArray)
            {
                foreach (Curve c in caa)
                {
                    vfloorPoints.Add(c.GetEndPoint(0));
                }
            }

            // Cria uma extrusão do PDMS
            Utilities.PDMSUtilities.CreatePDMSExtrusion(vpisoaltura, vfloorPoints, vbbcenter, stringBuilder, true);
        }
    }

    public static void CriarNichosRef(Element ele, Document doc, XYZ centerPoint, XYZ refPoint, StringBuilder stringBuilder)
    {
        ElementId typeVId = ele.GetTypeId();

        ElementType typeV = doc.GetElement(typeVId) as ElementType;

        string famNameV = typeV.FamilyName;

        BoundingBoxXYZ bbv = ele.get_BoundingBox(doc.ActiveView);

        XYZ bbvMax = bbv.Max;
        XYZ bbvMin = bbv.Min;

        XYZ bbvCenter = (bbvMax + bbvMin) / 2;

        bbvCenter = bbvCenter - centerPoint;

        //Caso seja uma FamilyInstance então tem rotação
        FamilyInstance vInstance = ele as FamilyInstance;

        XYZ rotation = new XYZ();
        double angleRadians = 0;
        double angleDegrees = 0;

        if (vInstance != null)
        {
            rotation = vInstance.FacingOrientation;
            angleRadians = rotation.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);
            angleDegrees = (180 / Math.PI) * angleRadians;
            angleDegrees = Math.Round(angleDegrees, 2);
        }


        if (famNameV == "Nicho caixa")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter NichoComprimentoParam = ele.LookupParameter("Comprimento");
            Parameter nichoLarguraParam = ele.LookupParameter("Largura");

            double xlen = UnitUtils.ConvertFromInternalUnits(nichoLarguraParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);

            xlen = Math.Round(xlen, 3);
            ylen = Math.Round(ylen, 3);
            zlen = Math.Round(zlen, 3);

            CreatePDMSBox(zlen, xlen, ylen, angleDegrees, bbvCenter, stringBuilder, true);
        }
        else if (famNameV == "Nicho pirâmide")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter NichoComprimentoParam = ele.LookupParameter("Comprimento");
            Parameter NichoComprimentoTParam = ele.LookupParameter("ComprimentoT");
            Parameter nichoLarguraParam = ele.LookupParameter("Largura");
            Parameter nichoLarguraTParam = ele.LookupParameter("LarguraT");

            double xlen = UnitUtils.ConvertFromInternalUnits(nichoLarguraParam.AsDouble(), UnitTypeId.Millimeters);
            double xlenT = UnitUtils.ConvertFromInternalUnits(nichoLarguraTParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double ylenT = UnitUtils.ConvertFromInternalUnits(NichoComprimentoTParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);

            xlen = Math.Round(xlen, 3);
            xlenT = Math.Round(xlenT, 3);
            ylen = Math.Round(ylen, 3);
            ylenT = Math.Round(ylenT, 3);
            zlen = Math.Round(zlen, 3);

            CreatePDMSPyramid(zlen, xlen, ylen, xlenT, ylenT, 0, 0, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "Cilindro genérico")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("Altura");
            Parameter nichoDiâmetroParam = ele.LookupParameter("Diâmetro");

            double nichoAltura = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);
            double nichoDiametro = UnitUtils.ConvertFromInternalUnits(nichoDiâmetroParam.AsDouble(), UnitTypeId.Millimeters);

            nichoAltura = Math.Round(nichoAltura, 3);
            nichoDiametro = Math.Round(nichoDiametro, 3);

            CreatePDMSCylinder(nichoAltura, nichoDiametro, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "Corte de canto triangular")
        {
            Parameter nichoAlturaParam = ele.LookupParameter("HEIG");
            Parameter NichoComprimentoParam = ele.LookupParameter("YTOP");
            Parameter NichoComprimentoTParam = ele.LookupParameter("YBOT");
            Parameter nichoLarguraTParam = ele.LookupParameter("XTOP");
            Parameter xoffParam = ele.LookupParameter("XOFF");

            double xlen = 0;
            double xlenT = UnitUtils.ConvertFromInternalUnits(nichoLarguraTParam.AsDouble(), UnitTypeId.Millimeters);
            double ylen = UnitUtils.ConvertFromInternalUnits(NichoComprimentoParam.AsDouble(), UnitTypeId.Millimeters);
            double ylenT = UnitUtils.ConvertFromInternalUnits(NichoComprimentoTParam.AsDouble(), UnitTypeId.Millimeters);
            double zlen = UnitUtils.ConvertFromInternalUnits(nichoAlturaParam.AsDouble(), UnitTypeId.Millimeters);
            double xoff = UnitUtils.ConvertFromInternalUnits(xoffParam.AsDouble(), UnitTypeId.Millimeters);

            xlenT = Math.Round(xlenT, 3);
            ylen = Math.Round(ylen, 3);
            ylenT = Math.Round(ylenT, 3);
            zlen = Math.Round(zlen, 3);
            xoff = Math.Round(xoff, 3);

            angleDegrees = angleDegrees + 180;

            CreatePDMSPyramid(zlen, xlen, ylen, xlenT, ylenT, xoff, 0, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "PDMSBox")
        {
            Parameter alturaParam = ele.LookupParameter("ZLEN");
            Parameter larguraParam = ele.LookupParameter("XLEN");
            Parameter comprimentoParam = ele.LookupParameter("YLEN");


            double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
            double largura = UnitUtils.ConvertFromInternalUnits(larguraParam.AsDouble(), UnitTypeId.Millimeters);
            double comprimento = UnitUtils.ConvertFromInternalUnits(comprimentoParam.AsDouble(), UnitTypeId.Millimeters);

            // Arredonda os valores
            altura = Math.Round(altura, 2);
            largura = Math.Round(largura, 2);
            comprimento = Math.Round(comprimento, 2);

            CreatePDMSBox(altura, largura, comprimento, angleDegrees, bbvCenter, stringBuilder, true);

        }
        else if (famNameV == "PDMSCylinder")
        {
            Parameter alturaParam = ele.LookupParameter("HEIG");
            Parameter diametroParam = ele.LookupParameter("DIAM");


            double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
            double largura = UnitUtils.ConvertFromInternalUnits(diametroParam.AsDouble(), UnitTypeId.Millimeters);

            // Arredonda os valores
            altura = Math.Round(altura, 2);
            largura = Math.Round(largura, 2);

            CreatePDMSCylinder(altura, largura, bbvCenter, stringBuilder, true);
        }
        else if (ele is Floor)
        {
            Floor floor = ele as Floor;

            Parameter vparamThickness = ele.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);

            double vpisothickness = vparamThickness.AsDouble();

            double vpisoaltura = Math.Round(UnitUtils.ConvertFromInternalUnits(vpisothickness, UnitTypeId.Millimeters), 3);

            List<XYZ> vfloorPoints = new List<XYZ>();

            BoundingBoxXYZ vpisoBb = ele.get_BoundingBox(null);

            XYZ vbbcenter = (vpisoBb.Max + vpisoBb.Min) / 2;

            vbbcenter = new XYZ(vbbcenter.X, vbbcenter.Y, vbbcenter.Z - centerPoint.Z - vpisothickness / 2);

            CurveArrArray vcurveArrArray = (doc.GetElement(floor.SketchId) as Sketch).Profile;

            foreach (CurveArray caa in vcurveArrArray)
            {
                foreach (Curve c in caa)
                {
                    vfloorPoints.Add(c.GetEndPoint(0));
                }
            }

            // Cria uma extrusão do PDMS
            Utilities.PDMSUtilities.CreatePDMSExtrusionRef(vpisoaltura, vfloorPoints, vbbcenter, refPoint, stringBuilder, true);
        }
    }

    public static void CreatePDMSCommands(Document doc, List<Element> lstElements, StringBuilder sb, XYZ refPoint)
    {
        foreach (Element ele in lstElements)
        {
            #region VERIFICA OS ELEMENTOS VAZIOS QUE INTERSECCIONAM

            BoundingBoxXYZ pisobb = ele.get_BoundingBox(doc.ActiveView);

            XYZ pisobbMax = pisobb.Max;
            XYZ pisobbMin = pisobb.Min;

            XYZ pisobbCenter = (pisobbMax + pisobbMin) / 2;

            #endregion

            #region VERIFICA SE OS ELEMENTOS QUE ESTÃO UNIDOS AO ELEMENTO

            List<Element> intersectelements = new List<Element>();

            ICollection<ElementId> joinedElements = JoinGeometryUtils.GetJoinedElements(doc, ele);

            foreach (var item in joinedElements)
            {
                intersectelements.Add(doc.GetElement(item));
            }

            BoundingBoxXYZ bb = ele.get_BoundingBox(null);

            XYZ bbMax = bb.Max;
            XYZ bbMin = bb.Min;

            XYZ bbCenter = (bbMax + bbMin) / 2;

            bbCenter = bbCenter - refPoint;

            #endregion

            #region CASO O ELEMENTO SEJA UMA FAMILY INSTANCE

            // Caso seja uma family Instance pega os ângulos

            double bangleDegrees = 0;

            if (ele is FamilyInstance)
            {
                FamilyInstance bInstance = ele as FamilyInstance;

                XYZ brotation = bInstance.FacingOrientation;
                XYZ brotatioB = bInstance.HandOrientation;

                double bangleRadians = brotation.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ.Negate());

                bangleDegrees = (180 / Math.PI) * bangleRadians;

                bangleDegrees = Math.Round(bangleDegrees, 2);
            }

            // Pega o nome da família

            ElementId typeId = ele.GetTypeId();

            ElementType type = doc.GetElement(typeId) as ElementType;

            FamilySymbol symbol = type as FamilySymbol;

            string famName = "";

            if (symbol != null)
            {
                famName = type.FamilyName;
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSBox

            if (famName == "PDMSBox")
            {
                Parameter alturaParam = ele.LookupParameter("ZLEN");
                Parameter larguraParam = ele.LookupParameter("XLEN");
                Parameter comprimentoParam = ele.LookupParameter("YLEN");


                double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
                double largura = UnitUtils.ConvertFromInternalUnits(larguraParam.AsDouble(), UnitTypeId.Millimeters);
                double comprimento = UnitUtils.ConvertFromInternalUnits(comprimentoParam.AsDouble(), UnitTypeId.Millimeters);

                // Arredonda os valores
                altura = Math.Round(altura, 2);
                largura = Math.Round(largura, 2);
                comprimento = Math.Round(comprimento, 2);

                Utilities.PDMSUtilities.CreatePDMSBox(altura, largura, comprimento, bangleDegrees, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSCylinder

            if (famName == "PDMSCylinder")
            {
                Parameter alturaParam = ele.LookupParameter("HEIG");
                Parameter diametroParam = ele.LookupParameter("DIAM");


                double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
                double largura = UnitUtils.ConvertFromInternalUnits(diametroParam.AsDouble(), UnitTypeId.Millimeters);

                // Arredonda os valores
                altura = Math.Round(altura, 2);
                largura = Math.Round(largura, 2);

                Utilities.PDMSUtilities.CreatePDMSCylinder(altura, largura, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSPyramid

            if (famName == "PDMSPyramid")
            {
                Parameter heigParam = ele.LookupParameter("HEIG");
                Parameter xbotParam = ele.LookupParameter("XBOT");
                Parameter ybotParam = ele.LookupParameter("YBOT");
                Parameter xtopParam = ele.LookupParameter("XTOP");
                Parameter ytopParam = ele.LookupParameter("YTOP");
                Parameter xoffParam = ele.LookupParameter("XOFF");
                Parameter yoffParam = ele.LookupParameter("YOFF");

                // Corrige a posição de acordo com o offset
                double xoff = xoffParam.AsDouble();
                double yoff = yoffParam.AsDouble();
                bbCenter = new XYZ(bbCenter.X + xoff / 2, bbCenter.Y + yoff / 2, bbCenter.Z);

                double heig = UnitUtils.ConvertFromInternalUnits(heigParam.AsDouble(), UnitTypeId.Millimeters);
                double xbot = UnitUtils.ConvertFromInternalUnits(xbotParam.AsDouble(), UnitTypeId.Millimeters);
                double ybot = UnitUtils.ConvertFromInternalUnits(ybotParam.AsDouble(), UnitTypeId.Millimeters);
                double xtop = UnitUtils.ConvertFromInternalUnits(xtopParam.AsDouble(), UnitTypeId.Millimeters);
                double ytop = UnitUtils.ConvertFromInternalUnits(ytopParam.AsDouble(), UnitTypeId.Millimeters);
                xoff = UnitUtils.ConvertFromInternalUnits(xoff, UnitTypeId.Millimeters);
                yoff = UnitUtils.ConvertFromInternalUnits(yoff, UnitTypeId.Millimeters);

                // Transforma os offsets


                // Arredonda os valores
                heig = Math.Round(heig, 2);
                xbot = Math.Round(xbot, 2);
                ybot = Math.Round(ybot, 2);
                xtop = Math.Round(xtop, 2);
                ytop = Math.Round(ytop, 2);
                xoff = Math.Round(xoff, 2);
                yoff = Math.Round(yoff, 2);

                Utilities.PDMSUtilities.CreatePDMSPyramid(heig, xbot, ybot, xtop, ytop, xoff, yoff, bangleDegrees, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSPyramidB

            if (famName == "PDMSPyramidB")
            {
                Parameter heigParam = ele.LookupParameter("HEIG");
                Parameter xbotParam = ele.LookupParameter("XBOT");
                Parameter ybotParam = ele.LookupParameter("YBOT");
                Parameter xtopParam = ele.LookupParameter("XTOP");
                Parameter ytopParam = ele.LookupParameter("YTOP");
                Parameter xoffParam = ele.LookupParameter("XOFF");
                Parameter yoffParam = ele.LookupParameter("YOFF");

                // Corrige a posição de acordo com o offset
                double xoff = xoffParam.AsDouble();
                double yoff = yoffParam.AsDouble();
                bbCenter = new XYZ(bbCenter.X + xoff / 2, bbCenter.Y + yoff / 2, bbCenter.Z);

                double heig = UnitUtils.ConvertFromInternalUnits(heigParam.AsDouble(), UnitTypeId.Millimeters);
                double xbot = UnitUtils.ConvertFromInternalUnits(xbotParam.AsDouble(), UnitTypeId.Millimeters);
                double ybot = UnitUtils.ConvertFromInternalUnits(ybotParam.AsDouble(), UnitTypeId.Millimeters);
                double xtop = UnitUtils.ConvertFromInternalUnits(xtopParam.AsDouble(), UnitTypeId.Millimeters);
                double ytop = UnitUtils.ConvertFromInternalUnits(ytopParam.AsDouble(), UnitTypeId.Millimeters);
                xoff = UnitUtils.ConvertFromInternalUnits(xoff, UnitTypeId.Millimeters);
                yoff = UnitUtils.ConvertFromInternalUnits(yoff, UnitTypeId.Millimeters);

                // Transforma os offsets


                // Arredonda os valores
                heig = Math.Round(heig, 2);
                xbot = Math.Round(xbot, 2);
                ybot = Math.Round(ybot, 2);
                xtop = Math.Round(xtop, 2);
                ytop = Math.Round(ytop, 2);
                xoff = Math.Round(xoff, 2);
                yoff = Math.Round(yoff, 2);

                Utilities.PDMSUtilities.CreatePDMSPyramid(heig, xbot, ybot, xtop, ytop, xoff, yoff, bangleDegrees, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSPyramidHor

            if (famName == "PDMSPyramidHor")
            {
                Parameter heigParam = ele.LookupParameter("HEIG");
                Parameter xbotParam = ele.LookupParameter("XBOT");
                Parameter ybotParam = ele.LookupParameter("YBOT");
                Parameter xtopParam = ele.LookupParameter("XTOP");
                Parameter ytopParam = ele.LookupParameter("YTOP");
                Parameter xoffParam = ele.LookupParameter("XOFF");
                Parameter yoffParam = ele.LookupParameter("YOFF");

                // Corrige a posição de acordo com o offset
                double xoff = xoffParam.AsDouble();
                double yoff = yoffParam.AsDouble();
                bbCenter = new XYZ(bbCenter.X, bbCenter.Y + yoff / 2, bbCenter.Z - xoff / 2);

                double heig = UnitUtils.ConvertFromInternalUnits(heigParam.AsDouble(), UnitTypeId.Millimeters);
                double xbot = UnitUtils.ConvertFromInternalUnits(xbotParam.AsDouble(), UnitTypeId.Millimeters);
                double ybot = UnitUtils.ConvertFromInternalUnits(ybotParam.AsDouble(), UnitTypeId.Millimeters);
                double xtop = UnitUtils.ConvertFromInternalUnits(xtopParam.AsDouble(), UnitTypeId.Millimeters);
                double ytop = UnitUtils.ConvertFromInternalUnits(ytopParam.AsDouble(), UnitTypeId.Millimeters);
                xoff = UnitUtils.ConvertFromInternalUnits(xoff, UnitTypeId.Millimeters);
                yoff = UnitUtils.ConvertFromInternalUnits(yoff, UnitTypeId.Millimeters);

                // Transforma os offsets

                // Arredonda os valores
                heig = Math.Round(heig, 2);
                xbot = Math.Round(xbot, 2);
                ybot = Math.Round(ybot, 2);
                xtop = Math.Round(xtop, 2);
                ytop = Math.Round(ytop, 2);
                xoff = Math.Round(xoff, 2);
                yoff = Math.Round(yoff, 2);

                Utilities.PDMSUtilities.CreatePDMSPyramidHor(heig, xbot, ybot, xtop, ytop, xoff, yoff, bangleDegrees, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UMA PDMSCone

            if (famName == "PDMSCone")
            {
                Parameter alturaParam = ele.LookupParameter("HEIG");
                Parameter diametroBaseParam = ele.LookupParameter("DBOT");
                Parameter diametroTopoParam = ele.LookupParameter("DTOP");

                double altura = UnitUtils.ConvertFromInternalUnits(alturaParam.AsDouble(), UnitTypeId.Millimeters);
                double diametroBase = UnitUtils.ConvertFromInternalUnits(diametroBaseParam.AsDouble(), UnitTypeId.Millimeters);
                double diametroTopo = UnitUtils.ConvertFromInternalUnits(diametroTopoParam.AsDouble(), UnitTypeId.Millimeters);

                // Arredonda os valores
                altura = Math.Round(altura, 2);
                diametroBase = Math.Round(diametroBase, 2);
                diametroTopo = Math.Round(diametroTopo, 2);

                Utilities.PDMSUtilities.CreatePDMSCone(altura, diametroBase, diametroTopo, bbCenter, sb, false);

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, bbCenter, sb);
                    }
                }
            }

            #endregion

            #region CASO O ELEMENTO SEJA UM PISO

            if (ele is Floor)
            {
                #region CRIA O CÓDIGO DO PISO A PARTIR DOS SEUS PONTOS E SUA ESPESSURA

                Floor floor = ele as Floor;

                Parameter paramThickness = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);

                double pisothickness = paramThickness.AsDouble();

                double pisoaltura = Math.Round(UnitUtils.ConvertFromInternalUnits(pisothickness, UnitTypeId.Millimeters), 3);

                List<XYZ> floorPoints = new List<XYZ>();

                pisobbCenter = new XYZ(pisobbCenter.X, pisobbCenter.Y, pisobbCenter.Z - pisothickness / 2);

                CurveArrArray curveArrArray = (doc.GetElement(floor.SketchId) as Sketch).Profile;

                foreach (CurveArray caa in curveArrArray)
                {
                    foreach (Curve c in caa)
                    {
                        floorPoints.Add(c.GetEndPoint(0));
                    }
                }

                // Cria uma extrusão do PDMS

                Utilities.PDMSUtilities.CreatePDMSExtrusionRef(pisoaltura, floorPoints, pisobbCenter, refPoint, sb, false);

                #endregion

                #region CRIA OS ELEMENTOS VAZIOS

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichosRef(intersect, doc, new XYZ(0, 0, pisobbCenter.Z), refPoint, sb);
                    }
                }

                #endregion
            }

            #endregion

            #region CASO O ELEMENTOS SEJA UMA PAREDE
            /*
            if (ele is Wall)
            {
                #region CRIA O CÓDIGO DA PAREDE A PARTIR DA SUA LINHA CENTRAL

                Wall wall = ele as Wall;
                ElementId wallTypeId = wall.GetTypeId();
                ElementType wallType = doc.GetElement(wallTypeId) as ElementType;

                // Pega os parâmetros da parede
                Parameter paramBaseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                double wallBaseOffset = paramBaseOffset.AsDouble();
                Parameter paramWallWidth = wallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);
                double wallWidth = paramWallWidth.AsDouble();
                Parameter paramWallHeigth = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                double wallHeigth = paramWallHeigth.AsDouble();

                // Pega a curva de localização da parede e transforma a mesma para ficar abaixo da boundingbox
                LocationCurve wallLocation = wall.Location as LocationCurve;
                Curve wallCurve = wallLocation.Curve;
                BoundingBoxXYZ bb = wall.get_BoundingBox(null);
                double distance = Math.Abs(bb.Max.Z) - Math.Abs(bb.Min.Z);
                Transform translate = Transform.CreateTranslation(XYZ.BasisZ.Multiply(-distance));
                wallCurve.CreateTransformed(translate);

                // Lista de pontos da parede
                List<XYZ> wallPoints = new List<XYZ>();

                // Cria novos pontos de acordo com a espessura da parede
                Line line = Line.CreateBound(wallCurve.GetEndPoint(0), wallCurve.GetEndPoint(1));
                Transform rotationA = Transform.CreateRotationAtPoint(XYZ.BasisZ, 0.5 * Math.PI, line.Origin);
                XYZ lineNormalA = rotationA.OfVector(line.Direction);
                Transform translate3 = Transform.CreateTranslation(lineNormalA.Multiply(wallWidth / 2));
                Transform translate4 = Transform.CreateTranslation(lineNormalA.Negate().Multiply(wallWidth / 2));

                Line lateralLineA1 = line.CreateTransformed(translate3) as Line;
                Line lateralLineA2 = line.CreateTransformed(translate4) as Line;

                wallPoints.Add(lateralLineA1.GetEndPoint(0));
                wallPoints.Add(lateralLineA1.GetEndPoint(1));
                wallPoints.Add(lateralLineA2.GetEndPoint(1));
                wallPoints.Add(lateralLineA2.GetEndPoint(0));


                // Cria uma extrusão do PDMS

                Utilities.PDMSUtilities.CreatePDMSGWall(wallHeigth, wallPoints, line.Evaluate(0.5, true), sb);

                #endregion

                #region VERIFICA SE OS ELEMENTOS INTERSECCIONAM A PAREDE

                List<Element> intersectelements = new List<Element>();

                XYZ bbMax = bb.Max;
                XYZ bbMin = bb.Min;

                XYZ bbCenter = (bbMax + bbMin) / 2;

                Outline outline = new Outline(bb.Min, bb.Max);

                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline, -UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters));

                foreach (var vazios in lstVazioElements)
                {
                    if (filter.PassesFilter(doc, vazios.Id))
                    {
                        intersectelements.Add(vazios);
                    }
                }

                #endregion

                #region CRIA OS ELEMENTOS VAZIOS

                if (intersectelements.Count() > 0)
                {
                    foreach (var intersect in intersectelements)
                    {
                        Utilities.PDMSUtilities.CriarNichos(intersect, doc, new XYZ(), sb);
                    }
                }

                #endregion
            }
            */
            #endregion

        }
    }

    public static void CreatePDMSPanel(double altura, List<XYZ> lstPoints, XYZ center, StringBuilder stringBuilder)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        stringBuilder.AppendLine("NEW PANEL");

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW PLOOP");
        stringBuilder.AppendLine(String.Format("HEIG {0}", altura));
        stringBuilder.AppendLine("SJUS CENTRE");

        foreach (XYZ point in lstPoints)
        {
            XYZ newPoint = point - center;

            double px = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.X, UnitTypeId.Millimeters), 3);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Y, UnitTypeId.Millimeters), 3);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Z, UnitTypeId.Millimeters), 3);

            stringBuilder.AppendLine("NEW PAVERT");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", px, pY, pZ));
            stringBuilder.AppendLine("END");
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();
    }

    public static void CreatePDMSVerticalPanel(double altura, List<XYZ> lstPoints, XYZ center, XYZ orientation, StringBuilder stringBuilder)
    {
        // Arredonda os valores
        altura = Math.Round(altura, 3);
        double centerX = Math.Round(UnitUtils.ConvertFromInternalUnits(center.X, UnitTypeId.Millimeters), 3);
        double centerY = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Y, UnitTypeId.Millimeters), 3);
        double centerZ = Math.Round(UnitUtils.ConvertFromInternalUnits(center.Z, UnitTypeId.Millimeters), 3);

        // Calcula o ângulo entre a normal da parede e o eixo X (E)
        double angle = orientation.AngleTo(XYZ.BasisX);
        double angleDegrees = (180 / Math.PI) * angle;
        angleDegrees = Math.Round(angleDegrees, 3);


        stringBuilder.AppendLine("NEW PANEL");

        stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", centerX, centerY, centerZ));
        stringBuilder.AppendLine(String.Format("ORI Y is U and Z is N", angleDegrees));
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("NEW PLOOP");
        stringBuilder.AppendLine(String.Format("HEIG {0}", altura));
        stringBuilder.AppendLine("SJUS CENTRE");

        foreach (XYZ point in lstPoints)
        {
            XYZ newPoint = point - center;

            double pX = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.X, UnitTypeId.Millimeters), 3);
            double pY = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Y, UnitTypeId.Millimeters), 3);
            double pZ = Math.Round(UnitUtils.ConvertFromInternalUnits(newPoint.Z, UnitTypeId.Millimeters), 3);

            stringBuilder.AppendLine("NEW PAVERT");
            stringBuilder.AppendLine(String.Format("POS E {0}mm N {1}mm U {2}mm", pX, pZ, 0
                ));
            stringBuilder.AppendLine("END");
        }

        stringBuilder.AppendLine("END");
        stringBuilder.AppendLine();
    }

}

#endregion

