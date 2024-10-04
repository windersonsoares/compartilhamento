using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

// Se o Xbim estiver disponível, inclua este construtor e método relacionados ao XbimMatrix3D
#if XBIM_AVAILABLE

using Xbim.Common.Geometry;

#endif

namespace AYPluginsAutoCAD
{
    public enum CadType
    {
        Line,
        Arc,
        Circle,
        Polyline,
        Hatch,
        Point,
        Block,
        BoundingBox,
    }

    public class CadObject
    {
        public CadType CadType;
        public LineGeometry LineGeometry { get; set; }
        public ArcGeometry ArcGeometry { get; set; }
        public CircleGeometry CircleGeometry { get; set; }
        public PolylineGeometry PolylineGeometry { get; set; }
        public PointGeometry PointGeometry { get; set; }
        public LoopsGeometry LoopsGeometry { get; set; }
        public BlockGeometry BlockGeometry { get; set; }
        public BoundingBoxGeometry BoundingBoxGeometry { get; set; }
        public IDictionary<string, string> CadProperties { get; set; }
        public CadObject()
        {
        }
        public CadObject(LineGeometry lineGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Line;
            LineGeometry = lineGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(ArcGeometry arcGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Arc;
            ArcGeometry = arcGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(CircleGeometry circleGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Circle;
            CircleGeometry = circleGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(PolylineGeometry polylineGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Polyline;
            PolylineGeometry = polylineGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(PointGeometry pointGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Point;
            PointGeometry = pointGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(BlockGeometry blockGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Block;
            BlockGeometry = blockGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(LoopsGeometry loopsGeometry, IDictionary<string, string> cadProperties)
        {
            CadType = CadType.Hatch;
            LoopsGeometry = loopsGeometry;
            CadProperties = cadProperties;
        }
        public CadObject(BoundingBoxGeometry boundingBoxGeometry)
        {
            CadType = CadType.BoundingBox;
            BoundingBoxGeometry = boundingBoxGeometry;
        }
    }

    public static class CadObjectUtils
    {
        public static CadPoint2D? FindIntersection(CadPoint2D p1, CadPoint2D p2, CadPoint2D p3, CadPoint2D p4)
        {
            // Calcula os coeficientes A1 e B1 para a equação da primeira reta.
            // A equação da reta é dada por: A1 * x + B1 * y = C1
            double A1 = p2.Y - p1.Y; // Coeficiente A1 (diferença entre as coordenadas Y dos pontos p1 e p2)
            double B1 = p1.X - p2.X; // Coeficiente B1 (diferença entre as coordenadas X dos pontos p1 e p2)
            double C1 = A1 * p1.X + B1 * p1.Y; // Coeficiente C1 (produto de A1 e B1 com as coordenadas do ponto p1)

            // Calcula os coeficientes A2 e B2 para a equação da segunda reta.
            // A equação da reta é dada por: A2 * x + B2 * y = C2
            double A2 = p4.Y - p3.Y; // Coeficiente A2 (diferença entre as coordenadas Y dos pontos p3 e p4)
            double B2 = p3.X - p4.X; // Coeficiente B2 (diferença entre as coordenadas X dos pontos p3 e p4)
            double C2 = A2 * p3.X + B2 * p3.Y; // Coeficiente C2 (produto de A2 e B2 com as coordenadas do ponto p3)

            // Calcula o determinante (det) das equações lineares.
            // Se det for zero, significa que as retas são paralelas (não há interseção).
            double det = A1 * B2 - A2 * B1;

            // Verifica se o determinante é zero.
            // Se for zero, as retas são paralelas e não se intersectam.
            if (det == 0)
            {
                return null;
            }

            // Calcula as coordenadas x e y da interseção das duas retas usando a fórmula:
            // x = (B2 * C1 - B1 * C2) / det
            // y = (A1 * C2 - A2 * C1) / det
            double x = (B2 * C1 - B1 * C2) / det; // Coordenada X do ponto de interseção
            double y = (A1 * C2 - A2 * C1) / det; // Coordenada Y do ponto de interseção

            // Retorna o ponto de interseção (x, y) encapsulado em um objeto CadPoint2D.
            return new CadPoint2D(x, y);
        }

        public static CadPoint2D? FindIntersection(CadLine2D l1, CadLine2D l2)
        {
            return FindIntersection(l1.StartPoint, l1.EndPoint, l2.StartPoint, l2.EndPoint);
        }

        // https://computergraphics.stackexchange.com/questions/5910/rounding-corners-of-polygon-given-vertices-of-its-corners
        public static CadArc2D? GenerateFillet(CadPoint2D V1, CadPoint2D V2, CadPoint2D V3, double radius)
        {
            CadPoint2D a = (V2 - V1).Normalize();
            CadPoint2D b = (V2 - V3).Normalize();

            double halfang = Math.Acos(CadPoint2D.Dot(a, b)) / 2;

            // Calcular o ponto intermediário do arco usando a metade do ângulo calculado.
            // Ângulo médio entre os vetores a e b com relação ao ponto de interseção (pi).
            CadPoint2D midDir = (a + b).Normalize(); // Direção média entre os vetores.


            // Cálculo de C
            CadPoint2D centerPointC = new CadPoint2D(
                V2.X - (radius / Math.Sin(halfang)) * midDir.X,
                V2.Y - (radius / Math.Sin(halfang)) * midDir.Y
            );

            // Cálculo de A
            CadPoint2D startPointA = new CadPoint2D(
                V2.X - (radius / Math.Tan(halfang)) * a.X,
                V2.Y - (radius / Math.Tan(halfang)) * a.Y
            );

            // Cálculo de B
            CadPoint2D endPointB = new CadPoint2D(
                V2.X - (radius / Math.Tan(halfang)) * b.X,
                V2.Y - (radius / Math.Tan(halfang)) * b.Y
            );



            // Ponto intermediário usando o centro do arco e a direção média.
            CadPoint2D midPoint = new CadPoint2D(
                centerPointC.X + radius * midDir.X,
                centerPointC.Y + radius * midDir.Y
            );

            // Calcular ângulos inicial e final com base no centro do arco
            double startAngle = Math.Atan2(startPointA.Y - centerPointC.Y, startPointA.X - centerPointC.X);
            double endAngle = Math.Atan2(endPointB.Y - centerPointC.Y, endPointB.X - centerPointC.X);


            double startAngleDegrees = startAngle * 180 / Math.PI;
            double endAngleDegrees = endAngle * 180 / Math.PI;


            // Retornar o arco com os ângulos inicial e final
            return new CadArc2D(startPointA, endPointB, centerPointC, midPoint, startAngle, endAngle, radius);
        }

        public static CadArc2D? GenerateFillet(CadLine2D l1, CadLine2D l2, double radius)
        {
            CadPoint2D? intersection = FindIntersection(l1, l2);

            if (intersection == null) return null;

            return GenerateFillet(l1.StartPoint, intersection, l2.EndPoint, radius);
        }

        public static double CalculateAreaWithTolerance(List<ICadCurve2D> curves, double tolerance = 0.1)
        {
            List<CadPoint2D> vertices = new List<CadPoint2D>();

            foreach (var curve in curves)
            {
                if (curve is CadLine2D line)
                {
                    // Adiciona os pontos das linhas diretamente
                    vertices.Add(line.StartPoint);
                    vertices.Add(line.EndPoint);
                }
                else if (curve is CadArc2D arc)
                {
                    // Cálculo do comprimento do arco
                    double arcLength = arc.Radius * (arc.EndAngle - arc.StartAngle);

                    if (arcLength <= tolerance)
                    {
                        // Se o comprimento do arco for menor ou igual à tolerância, aproximar como uma linha
                        vertices.Add(arc.StartPoint);
                        vertices.Add(arc.EndPoint);
                    }
                    else
                    {
                        // Calcula o número de segmentos com base na tolerância
                        int numSegments = (int)Math.Ceiling(arcLength / tolerance);

                        // Divide o arco em segmentos com base no número de segmentos calculados
                        double angleIncrement = (arc.EndAngle - arc.StartAngle) / numSegments;

                        for (int i = 0; i <= numSegments; i++)
                        {
                            double angle = arc.StartAngle + i * angleIncrement;

                            // Calcula os pontos ao longo do arco
                            double x = arc.Center.X + arc.Radius * Math.Cos(angle);
                            double y = arc.Center.Y + arc.Radius * Math.Sin(angle);

                            // Adiciona o ponto calculado
                            vertices.Add(new CadPoint2D(x, y));
                        }
                    }
                }
            }

            // Aplica a fórmula de Shoelace nos vértices calculados
            return ShoelaceFormula(vertices);
        }

        public static double ShoelaceFormula(List<CadPoint2D> vertices)
        {
            int n = vertices.Count;
            double area = 0;

            for (int i = 0; i < n - 1; i++)
            {
                area += (vertices[i].X * vertices[i + 1].Y) - (vertices[i + 1].X * vertices[i].Y);
            }

            // Conectar o último ponto ao primeiro
            area += (vertices[n - 1].X * vertices[0].Y) - (vertices[0].X * vertices[n - 1].Y);

            // Retornar a área absoluta dividida por 2
            return Math.Abs(area / 2);
        }

        public static List<ICadCurve2D> ConvertCurvesToLinesWithTolerance(List<ICadCurve2D> curves, double tolerance = 0.1)
        {
            List<ICadCurve2D> resultCurves = new List<ICadCurve2D>();

            foreach (var curve in curves)
            {
                if (curve is CadLine2D line)
                {
                    // Adiciona as linhas diretamente
                    resultCurves.Add(line);
                }
                else if (curve is CadArc2D arc)
                {
                    // Cálculo do comprimento do arco
                    double arcLength = Math.Abs(arc.Radius * (arc.EndAngle - arc.StartAngle));



                    if (arcLength <= tolerance)
                    {
                        // Se o comprimento do arco for menor ou igual à tolerância, aproximar como uma linha
                        resultCurves.Add(new CadLine2D(arc.StartPoint, arc.EndPoint));
                    }
                    else
                    {
                        // Calcula o número de segmentos com base na tolerância
                        int numSegments = (int)Math.Ceiling(arcLength / tolerance);

                        // Divide o arco em segmentos de linha
                        double angleIncrement = (arc.EndAngle - arc.StartAngle) / numSegments;

                        CadPoint2D previousPoint = arc.StartPoint;

                        for (int i = 1; i <= numSegments; i++)
                        {
                            double angle = arc.StartAngle + i * angleIncrement;

                            // Calcula o próximo ponto ao longo do arco
                            double x = arc.Center.X + arc.Radius * Math.Cos(angle);
                            double y = arc.Center.Y + arc.Radius * Math.Sin(angle);
                            CadPoint2D nextPoint = new CadPoint2D(x, y);

                            // Adiciona a linha que conecta os pontos anteriores e próximos
                            resultCurves.Add(new CadLine2D(previousPoint, nextPoint));

                            // Atualiza o ponto anterior
                            previousPoint = nextPoint;
                        }
                    }
                }
            }

            return resultCurves;
        }

        public static void ScaleCurves(List<ICadCurve3D> cadCurves, double scaleFactor)
        {
            // Verifica se o fator de escala é válido
            if (scaleFactor <= 0)
            {
                throw new ArgumentException("O fator de escala deve ser um número positivo.", nameof(scaleFactor));
            }

            // Percorre cada curva na lista
            foreach (var curve in cadCurves)
            {
                // Verifica se a curva é uma linha e aplica o fator de escala
                if (curve is CadLine3D line)
                {
                    // Escala os pontos de início e fim
                    line.StartPoint = new CadPoint3D(line.StartPoint.X * scaleFactor,
                                                      line.StartPoint.Y * scaleFactor,
                                                      line.StartPoint.Z * scaleFactor);

                    line.EndPoint = new CadPoint3D(line.EndPoint.X * scaleFactor,
                                                    line.EndPoint.Y * scaleFactor,
                                                    line.EndPoint.Z * scaleFactor);
                }
                // Verifica se a curva é um arco e aplica o fator de escala
                else if (curve is CadArc3D arc)
                {
                    arc.StartPoint = new CadPoint3D(arc.StartPoint.X * scaleFactor,
                                                     arc.StartPoint.Y * scaleFactor,
                                                     arc.StartPoint.Z * scaleFactor);

                    arc.EndPoint = new CadPoint3D(arc.EndPoint.X * scaleFactor,
                                                   arc.EndPoint.Y * scaleFactor,
                                                   arc.EndPoint.Z * scaleFactor);

                    arc.Center = new CadPoint3D(arc.Center.X * scaleFactor,
                                                 arc.Center.Y * scaleFactor,
                                                 arc.Center.Z * scaleFactor);

                    arc.Middle = new CadPoint3D(arc.Middle.X * scaleFactor,
                                                 arc.Middle.Y * scaleFactor,
                                                 arc.Middle.Z * scaleFactor);

                    arc.Normal = new CadPoint3D(arc.Normal.X * scaleFactor,
                                                 arc.Normal.Y * scaleFactor,
                                                 arc.Normal.Z * scaleFactor);
                }
                // Adicione outras verificações de tipo de curva conforme necessário
            }
        }

        public static string IdentifyProfiles(List<List<ICadCurve3D>> cadCurves)
        {
            StringBuilder sb = new StringBuilder();

            foreach (List<ICadCurve3D> curves in cadCurves)
            {
                sb.Append(IdentifyProfile(curves));
            }

            return sb.ToString();
        }

        public static string IdentifyProfile(List<ICadCurve3D> cadCurves)
        {
            string profileType = "generic";

            if (IsRectangle(cadCurves))
            {
                profileType = "rectangle";
            }
            else if (IsCircle(cadCurves))
            {
                profileType = "circle";
            }

            return profileType;
        }

        public static bool IsRectangle(List<ICadCurve3D> cadCurves)
        {
            if (cadCurves.Count != 4)
            {
                return false; // Um retângulo precisa de 4 lados.
            }

            List<CadLine3D> lines = cadCurves.OfType<CadLine3D>().ToList();

            if (lines.Count != 4)
            {
                return false; // Todos os lados devem ser linhas.
            }

            // Verifica os comprimentos
            double length1 = lines[0].Length;
            double length2 = lines[1].Length;
            double length3 = lines[2].Length;
            double length4 = lines[3].Length;

            // Verifica se lados opostos são iguais
            if (Math.Abs(length1 - length3) > 1e-6 || Math.Abs(length2 - length4) > 1e-6)
            {
                return false;
            }

            // Verifica os ângulos
            if (!AreAnglesRight(lines))
            {
                return false;
            }

            return true;
        }

        private static bool AreAnglesRight(List<CadLine3D> lines)
        {
            // Verifica se todos os ângulos são de 90 graus
            for (int i = 0; i < lines.Count; i++)
            {
                CadLine3D line1 = lines[i];
                CadLine3D line2 = lines[(i + 1) % lines.Count]; // próximo lado

                CadPoint3D direction1 = (line1.EndPoint - line1.StartPoint).Normalize();
                CadPoint3D direction2 = (line2.EndPoint - line2.StartPoint).Normalize();

                // Verifica o produto escalar para ângulo reto
                if (Math.Abs(direction1.DotProduct(direction2)) > 1e-6)
                {
                    return false; // ângulo não é reto
                }
            }

            return true;
        }

        public static bool IsCircle(List<ICadCurve3D> cadCurves)
        {
            // Se o CurveArray contém exatamente 1 arco
            if (cadCurves.Count == 1)
            {
                if (cadCurves.FirstOrDefault() != null && cadCurves.FirstOrDefault()?.CadCurveType == "arc")
                {
                    return true;
                }

                return false; // Não é um círculo se não for um arco
            }
            // Se o CurveArray contém exatamente 2 arcos
            else if (cadCurves.Count == 2)
            {
                // Obtém os arcos
                CadArc3D[] arcs = cadCurves.OfType<CadArc3D>().ToArray();

                if (arcs.Length != 2)
                {
                    return false; // Deve conter exatamente 2 arcos
                }

                // Verifica se os raios dos arcos são iguais
                double radius1 = arcs[0].Radius;
                double radius2 = arcs[1].Radius;

                if (Math.Abs(radius1 - radius2) > 1e-6)
                {
                    return false; // Raios diferentes, não é um círculo
                }

                // Verifica se as extremidades dos arcos estão conectadas
                if (!AreEndpointsConnected(arcs[0], arcs[1]))
                {
                    return false; // As extremidades não estão conectadas
                }

                // Se passou nas verificações, é um círculo
                return true;
            }

            // Se não houver arcos ou mais de 2 arcos, não é um círculo
            return false;
        }

        private static bool AreEndpointsConnected(CadArc3D arc1, CadArc3D arc2)
        {
            // Verifica se o ponto final do primeiro arco é igual ao ponto inicial do segundo arco
            // ou se o ponto inicial do primeiro arco é igual ao ponto final do segundo arco
            CadPoint3D startPointArc1 = arc1.StartPoint;
            CadPoint3D endPointArc1 = arc1.EndPoint;
            CadPoint3D startPointArc2 = arc2.StartPoint;
            CadPoint3D endPointArc2 = arc2.EndPoint;

            return (endPointArc1.IsAlmostEqualTo(startPointArc2) || startPointArc1.IsAlmostEqualTo(endPointArc2));
        }

        public static List<(ICadCurve3D, double)> GetCadCurvesRotation(List<ICadCurve3D> cadCurves)
        {
            // Vetor unitário do eixo X
            CadPoint3D xAxis = new CadPoint3D(1, 0, 0);

            List<(ICadCurve3D, double)> curvesWithAngle = new List<(ICadCurve3D, double)>();

            // Percorre todas as linhas e encontra a que tem o menor ângulo com o eixo X
            foreach (var curve in cadCurves)
            {
                double angle = 0;

                if (curve is CadLine3D line)
                {
                    // Calcula o vetor de direção da linha
                    CadPoint3D direction = line.EndPoint - line.StartPoint;
                    direction = direction.Normalize(); // Normaliza o vetor

                    // Calcula o produto escalar entre o vetor de direção e o eixo X
                    double dotProduct = direction.DotProduct(xAxis);

                    // Calcula o ângulo entre o vetor de direção e o eixo X
                    angle = Math.Acos(dotProduct); // O resultado está em radianos
                }

                curvesWithAngle.Add((curve, angle));
            }

            // Ordena a lista com base no ângulo (segundo item da tupla)
            var orderedCurvesWithAngle = curvesWithAngle.OrderBy(tuple => tuple.Item2).ToList();

            return orderedCurvesWithAngle;
        }
    }

    #region CLASSES BASES

    #region 2D

    /// <summary>
    /// Interface de marcação para representar curvas 2D no CAD, como arcos, linhas e polilinhas.
    /// </summary>
    public interface ICadCurve2D
    {
        public string CadCurveType { get; }
    }

    /// <summary>
    /// Representa um ponto ou vetor no plano 2D.
    /// </summary>
    public class CadPoint2D
    {
        /// <summary>
        /// Coordenada X do ponto.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Coordenada Y do ponto.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="CadPoint2D"/> com valores padrão (0,0).
        /// </summary>
        public CadPoint2D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="CadPoint2D"/> com valores fornecidos para X e Y.
        /// </summary>
        /// <param name="xCoord">Valor da coordenada X.</param>
        /// <param name="yCoord">Valor da coordenada Y.</param>
        public CadPoint2D(double xCoord, double yCoord)
        {
            X = xCoord;
            Y = yCoord;
        }

        /// <summary>
        /// Sobrecarga do operador de adição para somar dois pontos ou vetores.
        /// </summary>
        /// <param name="p1">Primeiro ponto ou vetor.</param>
        /// <param name="p2">Segundo ponto ou vetor.</param>
        /// <returns>A soma dos dois pontos ou vetores.</returns>
        public static CadPoint2D operator +(CadPoint2D p1, CadPoint2D p2)
        {
            return new CadPoint2D(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// Sobrecarga do operador de subtração para subtrair dois pontos ou vetores.
        /// </summary>
        /// <param name="p1">Primeiro ponto ou vetor.</param>
        /// <param name="p2">Segundo ponto ou vetor.</param>
        /// <returns>A subtração dos dois pontos ou vetores.</returns>
        public static CadPoint2D operator -(CadPoint2D p1, CadPoint2D p2)
        {
            return new CadPoint2D(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// Sobrecarga do operador de multiplicação para multiplicar um ponto ou vetor por um escalar.
        /// </summary>
        /// <param name="p">Ponto ou vetor.</param>
        /// <param name="scalar">Valor escalar.</param>
        /// <returns>O resultado da multiplicação do ponto ou vetor pelo escalar.</returns>
        public static CadPoint2D operator *(CadPoint2D p, double scalar)
        {
            return new CadPoint2D(p.X * scalar, p.Y * scalar);
        }

        /// <summary>
        /// Calcula o produto escalar entre dois vetores.
        /// </summary>
        /// <param name="v1">Primeiro vetor.</param>
        /// <param name="v2">Segundo vetor.</param>
        /// <returns>O produto escalar entre os dois vetores.</returns>
        public static double Dot(CadPoint2D v1, CadPoint2D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        /// <summary>
        /// Calcula a distância entre dois pontos no plano 2D.
        /// </summary>
        /// <param name="p2">O segundo ponto.</param>
        /// <returns>A distância entre este ponto e o ponto <paramref name="p2"/>.</returns>
        public double DistanceTo(CadPoint2D p2)
        {
            return Math.Sqrt((this.X - p2.X) * (this.X - p2.X) + (this.Y - p2.Y) * (this.Y - p2.Y));
        }

        /// <summary>
        /// Obtém o comprimento ou magnitude do vetor.
        /// </summary>
        public double Length => Math.Sqrt(X * X + Y * Y);

        /// <summary>
        /// Normaliza o vetor (reduzindo seu comprimento para 1, mantendo a direção).
        /// </summary>
        /// <returns>Um novo <see cref="CadPoint2D"/> que representa o vetor normalizado.</returns>
        public CadPoint2D Normalize()
        {
            double length = this.Length;
            return new CadPoint2D(X / length, Y / length);
        }

        /// <summary>
        /// Calcula a média entre dois pontos.
        /// </summary>
        /// <param name="a">Primeiro ponto.</param>
        /// <param name="b">Segundo ponto.</param>
        /// <returns>A média dos dois pontos.</returns>
        public static CadPoint2D Average(CadPoint2D a, CadPoint2D b)
        {
            return new CadPoint2D((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }
    }

    /// <summary>
    /// Representa uma linha em 2D.
    /// </summary>
    public class CadLine2D : ICadCurve2D
    {
        /// <summary>
        /// Discriminador de tipo
        /// </summary>
        public string CadCurveType { get; } = "line";

        /// <summary>
        /// Ponto inicial da linha.
        /// </summary>
        public CadPoint2D StartPoint { get; set; }

        /// <summary>
        /// Ponto final da linha.
        /// </summary>
        public CadPoint2D EndPoint { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadLine2D"/>.
        /// </summary>
        /// <param name="startPoint">Ponto inicial do arco.</param>
        /// <param name="endPoint">Ponto final do arco.</param>
        public CadLine2D(CadPoint2D startPoint, CadPoint2D endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }

    /// <summary>
    /// Representa um arco em 2D.
    /// </summary>
    public class CadArc2D : ICadCurve2D
    {
        /// <summary>
        /// Discriminador de tipo
        /// </summary>
        public string CadCurveType { get; } = "arc";

        /// <summary>
        /// Ponto inicial do arco.
        /// </summary>
        public CadPoint2D StartPoint { get; set; }

        /// <summary>
        /// Ponto central do arco.
        /// </summary>
        public CadPoint2D MiddlePoint { get; set; }

        /// <summary>
        /// Ponto final do arco.
        /// </summary>
        public CadPoint2D EndPoint { get; set; }

        /// <summary>
        /// Centro do arco.
        /// </summary>
        public CadPoint2D Center { get; set; }

        /// <summary>
        /// Ângulo inicial do arco em relação ao eixo X do Sistema de Coordenadas do Objeto (OCS).
        /// Ângulos positivos são medidos no sentido anti-horário olhando em direção ao eixo Z para a origem.
        /// Ângulos negativos ainda desenham o arco no sentido anti-horário, mas medem na direção oposta.
        /// </summary>
        public double StartAngle { get; set; }

        /// <summary>
        /// Ângulo final do arco em relação ao eixo X do Sistema de Coordenadas do Objeto (OCS).
        /// Ângulos positivos são medidos no sentido anti-horário olhando em direção ao eixo Z para a origem.
        /// Ângulos negativos ainda desenham o arco no sentido anti-horário, mas medem na direção oposta.
        /// </summary>
        public double EndAngle { get; set; }

        /// <summary>
        /// Raio do arco.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadArc2D"/>.
        /// </summary>
        /// <param name="startPoint">Ponto inicial do arco.</param>
        /// <param name="endPoint">Ponto final do arco.</param>
        /// <param name="middlePoint">Ponto final do arco.</param>
        /// <param name="center">Centro do arco.</param>
        /// <param name="radius">Raio do arco.</param>
        public CadArc2D(CadPoint2D startPoint, CadPoint2D endPoint, CadPoint2D center, CadPoint2D middlePoint, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            MiddlePoint = middlePoint;
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadArc2D"/>.
        /// </summary>
        /// <param name="startPoint">Ponto inicial do arco.</param>
        /// <param name="endPoint">Ponto final do arco.</param>
        /// <param name="center">Centro do arco.</param>
        /// <param name="startAngle">Ângulo inicial do arco.</param>
        /// <param name="endAngle">Ângulo final do arco.</param>
        /// <param name="radius">Raio do arco.</param>
        public CadArc2D(CadPoint2D startPoint, CadPoint2D endPoint, CadPoint2D center, CadPoint2D middlePoint, double startAngle, double endAngle, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            MiddlePoint = middlePoint;
            Center = center;
            StartAngle = startAngle;
            EndAngle = endAngle;
            Radius = radius;
        }
    }

    /// <summary>
    /// Representa uma polilinha em 2D.
    /// </summary>
    public class CadPolyline2D : ICadCurve2D
    {
        /// <summary>
        /// Discriminador de tipo
        /// </summary>
        public string CadCurveType { get; } = "polyline";

        /// <summary>
        /// Lista de pontos da polilinha.
        /// </summary>
        public List<CadPoint2D> Points { get; set; } = new List<CadPoint2D>();

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPolyline2D"/>.
        /// </summary>
        public CadPolyline2D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPolyline2D"/>.
        /// </summary>
        /// <param name="points">Lista de pontos da polilinha.</param>
        public CadPolyline2D(List<CadPoint2D> points)
        {
            Points = points;
        }
    }

    /// <summary>
    /// Representa uma policurva em 2D.
    /// </summary>
    public class CadPolycurve2D : ICadCurve2D
    {
        /// <summary>
        /// Discriminador de tipo
        /// </summary>
        public string CadCurveType { get; } = "polycurve";

        /// <summary>
        /// Lista de curvas da policurva.
        /// </summary>
        public List<ICadCurve2D> Segments { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPolycurve2D"/>.
        /// </summary>
        /// <param name="segments">Lista de pontos da polilinha.</param>
        public CadPolycurve2D(List<ICadCurve2D> segments)
        {
            Segments = segments;
        }
    }

    /// <summary>
    /// Representa uma policurva em 2D com vazios internos.
    /// </summary>
    public class CadPolycurveWithVoids2D : ICadCurve2D
    {
        /// <summary>
        /// Discriminador de tipo
        /// </summary>
        public string CadCurveType { get; } = "polycurvewithvoids";

        /// <summary>
        /// Policurva externa
        /// </summary>
        public CadPolycurve2D OuterCurves { get; set; }

        /// <summary>
        /// Lista de policurvas internas representando os vazios
        /// </summary>
        public List<CadPolycurve2D> InnerCurves { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPolycurve2D"/>.
        /// </summary>
        /// <param name="outerCurves">Policurva externa.</param>
        /// <param name="innerCurves">Lista de policurvas internas representando os vazios.</param>
        public CadPolycurveWithVoids2D(CadPolycurve2D outerCurves, List<CadPolycurve2D> innerCurves)
        {
            OuterCurves = outerCurves;
            InnerCurves = innerCurves;
        }
    }

    #endregion

    #region 3D

    /// <summary>
    /// Interface de marcação para representar curvas 3D no CAD, como arcos, linhas e polilinhas.
    /// </summary>
    public interface ICadCurve3D
    {
        public string CadCurveType { get; }
    }

    /// <summary>
    /// Representa um ponto em um espaço tridimensional.
    /// </summary>
    public class CadPoint3D
    {
        /// <summary>
        /// Coordenada X do ponto.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Coordenada Y do ponto.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Coordenada Z do ponto.
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Tolerância para comparação
        /// </summary>
        private const double Tolerance = 1e-6;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPoint3D"/>.
        /// </summary>
        public CadPoint3D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadPoint3D"/> com coordenadas especificadas.
        /// </summary>
        /// <param name="x">A coordenada X do ponto.</param>
        /// <param name="y">A coordenada Y do ponto.</param>
        /// <param name="z">A coordenada Z do ponto.</param>
        public CadPoint3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Verifica se este ponto é quase igual a outro, dentro de uma tolerância.
        /// </summary>
        /// <param name="other">Outro ponto a ser comparado.</param>
        /// <returns>Verdadeiro se os pontos são quase iguais, falso caso contrário.</returns>
        public bool IsAlmostEqualTo(CadPoint3D other)
        {
            return Math.Abs(X - other.X) < Tolerance &&
                   Math.Abs(Y - other.Y) < Tolerance &&
                   Math.Abs(Z - other.Z) < Tolerance;
        }

        /// <summary>
        /// Normaliza o ponto, transformando suas coordenadas em uma unidade de comprimento igual a 1.
        /// Retorna um novo vetor normalizado.
        /// </summary>
        public CadPoint3D Normalize()
        {
            double length = Math.Sqrt(X * X + Y * Y + Z * Z);
            if (length > 0) // Evitar divisão por zero
            {
                return new CadPoint3D(X / length, Y / length, Z / length);
            }
            return new CadPoint3D(0, 0, 0); // Retorna um vetor zero se a normalização não for possível
        }

        /// <summary>
        /// Calcula a magnitude (comprimento) do vetor.
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Arredonda os valores das coordenadas
        /// </summary>
        public CadPoint3D RoundCoordinates(int digits = 0)
        {
            return new CadPoint3D(Math.Round(X, digits), Math.Round(Y, digits), Math.Round(Z, digits));
        }

        /// <summary>
        /// Calcula o produto escalar entre dois vetores.
        /// </summary>
        /// <param name="other">Outro vetor.</param>
        /// <returns>O produto escalar.</returns>
        public double DotProduct(CadPoint3D other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// Calcula o produto vetorial (cross product) entre dois vetores.
        /// </summary>
        public CadPoint3D CrossProduct(CadPoint3D other)
        {
            return new CadPoint3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }

        /// <summary>
        /// Sobrecarga do operador de adição para somar dois pontos ou vetores.
        /// </summary>
        public static CadPoint3D operator +(CadPoint3D p1, CadPoint3D p2)
        {
            return new CadPoint3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        /// <summary>
        /// Sobrecarga do operador de subtração para subtrair dois pontos ou vetores.
        /// </summary>
        public static CadPoint3D operator -(CadPoint3D p1, CadPoint3D p2)
        {
            return new CadPoint3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        /// <summary>
        /// Sobrecarga do operador de multiplicação escalar.
        /// </summary>
        public static CadPoint3D operator *(CadPoint3D p, double scalar)
        {
            return new CadPoint3D(p.X * scalar, p.Y * scalar, p.Z * scalar);
        }

        /// <summary>
        /// Sobrecarga do operador de divisão escalar.
        /// </summary>
        public static CadPoint3D operator /(CadPoint3D p, double scalar)
        {
            // Verifica se o escalar é diferente de zero para evitar divisão por zero
            if (Math.Abs(scalar) < 1e-9)
            {
                //throw new DivideByZeroException("O escalar para a divisão não pode ser zero.");
                return p;
            }

            return new CadPoint3D(p.X / scalar, p.Y / scalar, p.Z / scalar);
        }

        /// <summary>
        /// Retorna o ângulo entre este vetor e o vetor especificado em radianos.
        /// </summary>
        public double AngleTo(CadPoint3D other)
        {
            double dotProduct = this.DotProduct(other);
            double lengthsMultiplication = this.Length() * other.Length();

            // Evita divisão por zero
            if (lengthsMultiplication == 0)
                throw new InvalidOperationException("A magnitude de um ou ambos os vetores é zero.");

            // Cálculo do ângulo via arcosseno (cos^-1)
            double cosTheta = dotProduct / lengthsMultiplication;

            // Garante que o valor esteja no intervalo [-1, 1] para evitar erros de precisão
            cosTheta = Math.Max(-1, Math.Min(1, cosTheta));

            return Math.Acos(cosTheta); // Retorna o ângulo em radianos
        }

        /// <summary>
        /// Retorna o ângulo entre este vetor e o vetor especificado projetado no plano definido pelo vetor normal.
        /// </summary>
        public double AngleOnPlaneTo(CadPoint3D other, CadPoint3D normal)
        {
            // Projeção dos vetores no plano ortogonal ao vetor normal
            CadPoint3D thisProjected = this - (normal * (this.DotProduct(normal) / normal.Length()));
            CadPoint3D otherProjected = other - (normal * (other.DotProduct(normal) / normal.Length()));

            // O ângulo entre os vetores projetados
            double angle = thisProjected.AngleTo(otherProjected);

            // Verifica a direção do ângulo em relação ao vetor normal
            CadPoint3D crossProduct = thisProjected.CrossProduct(otherProjected);

            if (crossProduct.DotProduct(normal) < 0)
            {
                angle = 2 * Math.PI - angle; // Ângulo no sentido horário
            }

            return angle;
        }

        // Override dos métodos Equals e GetHashCode
        public override bool Equals(object obj)
        {
            if (obj is CadPoint3D other)
            {
                return IsAlmostEqualTo(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (X, Y, Z).GetHashCode();
        }
    }

    /// <summary>
    /// Representa uma linha em um espaço tridimensional.
    /// </summary>
    public class CadLine3D : ICadCurve3D
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "line";

        /// <summary>
        /// Ponto inicial da linha.
        /// </summary>
        public CadPoint3D StartPoint { get; set; }

        /// <summary>
        /// Ponto final da linha.
        /// </summary>
        public CadPoint3D EndPoint { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadLine3D"/>.
        /// </summary>
        public CadLine3D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadLine3D"/> com pontos especificados.
        /// </summary>
        /// <param name="startPoint">O ponto inicial da linha.</param>
        /// <param name="endPoint">O ponto final da linha.</param>
        public CadLine3D(CadPoint3D startPoint, CadPoint3D endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        /// <summary>
        /// Calcula o comprimento da linha.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(
                    Math.Pow(EndPoint.X - StartPoint.X, 2) +
                    Math.Pow(EndPoint.Y - StartPoint.Y, 2) +
                    Math.Pow(EndPoint.Z - StartPoint.Z, 2));
            }
        }

        /// <summary>
        /// Retorna o vetor de direção da linha, normalizado.
        /// </summary>
        public CadPoint3D Direction
        {
            get
            {
                CadPoint3D direction = EndPoint - StartPoint;
                return direction.Normalize(); // Retorna o vetor unitário
            }
        }
    }

    /// <summary>
    /// Representa um retângulo em um espaço tridimensional.
    /// </summary>
    public class CadRectangle3D : ICadCurve3D
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "rectangle";

        /// <summary>
        /// Comprimento ao longo do eixo X.
        /// </summary>
        public double LengthX { get; set; }

        /// <summary>
        /// Comprimento ao longo do eixo Y.
        /// </summary>
        public double LengthY { get; set; }

        /// <summary>
        /// Ponto 1 do retângulo.
        /// </summary>
        public CadPoint3D Point1 { get; set; }

        /// <summary>
        /// Ponto 2 do retângulo.
        /// </summary>
        public CadPoint3D Point2 { get; set; }

        /// <summary>
        /// Ponto 3 do retângulo.
        /// </summary>
        public CadPoint3D Point3 { get; set; }

        /// <summary>
        /// Ponto 4 do retângulo.
        /// </summary>
        public CadPoint3D Point4 { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadRectangle3D"/>.
        /// </summary>
        public CadRectangle3D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadRectangle3D"/> com os pontos especificados.
        /// </summary>
        /// <param name="point1">O primeiro ponto do retângulo.</param>
        /// <param name="point2">O segundo ponto do retângulo.</param>
        /// <param name="point3">O terceiro ponto do retângulo.</param>
        /// <param name="point4">O quarto ponto do retângulo.</param>
        public CadRectangle3D(CadPoint3D point1, CadPoint3D point2, CadPoint3D point3, CadPoint3D point4)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
            Point4 = point4;
        }
    }

    /// <summary>
    /// Representa um arco em um espaço tridimensional.
    /// </summary>
    public class CadArc3D : ICadCurve3D
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "arc";

        /// <summary>
        /// Ponto inicial do arco.
        /// </summary>
        public CadPoint3D StartPoint { get; set; }

        /// <summary>
        /// Ponto final do arco.
        /// </summary>
        public CadPoint3D EndPoint { get; set; }

        /// <summary>
        /// Centro do arco.
        /// </summary>
        public CadPoint3D Center { get; set; }

        /// <summary>
        /// Ponto médio do arco.
        /// </summary>
        public CadPoint3D Middle { get; set; }

        /// <summary>
        /// Normal do arco, representando a orientação.
        /// </summary>
        public CadPoint3D Normal { get; set; }

        /// <summary>
        /// Raio do arco.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Ângulo inicial do arco em relação ao eixo X do Sistema de Coordenadas do Objeto (OCS).
        /// Ângulos positivos são medidos no sentido anti-horário olhando em direção ao eixo Z para a origem.
        /// Ângulos negativos ainda desenham o arco no sentido anti-horário, mas medem na direção oposta.
        /// </summary>
        public double StartAngle { get; set; }

        /// <summary>
        /// Ângulo final do arco em relação ao eixo X do Sistema de Coordenadas do Objeto (OCS).
        /// Ângulos positivos são medidos no sentido anti-horário olhando em direção ao eixo Z para a origem.
        /// Ângulos negativos ainda desenham o arco no sentido anti-horário, mas medem na direção oposta.
        /// </summary>
        public double EndAngle { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadArc3D"/>.
        /// </summary>
        public CadArc3D()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadArc3D"/> com pontos e propriedades especificadas.
        /// </summary>
        /// <param name="startPoint">O ponto inicial do arco.</param>
        /// <param name="endPoint">O ponto final do arco.</param>
        /// <param name="center">O centro do arco.</param>
        /// <param name="middle">O ponto médio do arco.</param>
        /// <param name="normal">A normal do arco.</param>
        /// <param name="radius">O raio do arco.</param>
        public CadArc3D(CadPoint3D startPoint, CadPoint3D endPoint, CadPoint3D center, CadPoint3D middle, CadPoint3D normal, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Center = center;
            Middle = middle;
            Normal = normal;
            Radius = radius;
        }

        public CadArc3D(CadPoint3D startPoint, CadPoint3D endPoint, CadPoint3D center, CadPoint3D middle, CadPoint3D normal, double startAngle, double endAngle, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Middle = middle;
            Center = center;
            Normal = normal;
            StartAngle = startAngle;
            EndAngle = endAngle;
            Radius = radius;
        }


    }

    /// <summary>
    /// Representa uma matriz 3D para transformações homogêneas.
    /// </summary>
    public class CadMatrix3D
    {
        /// <summary>
        /// Primeiro elemento da matriz.
        /// </summary>
        public double M11 { get; set; }

        /// <summary>
        /// Segundo elemento da matriz.
        /// </summary>
        public double M12 { get; set; }

        /// <summary>
        /// Terceiro elemento da matriz.
        /// </summary>
        public double M13 { get; set; }

        /// <summary>
        /// Quarto elemento da matriz (geralmente usado para translações).
        /// </summary>
        public double M14 { get; set; }

        /// <summary>
        /// Quinto elemento da matriz.
        /// </summary>
        public double M21 { get; set; }

        /// <summary>
        /// Sexto elemento da matriz.
        /// </summary>
        public double M22 { get; set; }

        /// <summary>
        /// Sétimo elemento da matriz.
        /// </summary>
        public double M23 { get; set; }

        /// <summary>
        /// Oitavo elemento da matriz (geralmente usado para translações).
        /// </summary>
        public double M24 { get; set; }

        /// <summary>
        /// Nono elemento da matriz.
        /// </summary>
        public double M31 { get; set; }

        /// <summary>
        /// Décimo elemento da matriz.
        /// </summary>
        public double M32 { get; set; }

        /// <summary>
        /// Décimo primeiro elemento da matriz.
        /// </summary>
        public double M33 { get; set; }

        /// <summary>
        /// Décimo segundo elemento da matriz (geralmente usado para translações).
        /// </summary>
        public double M34 { get; set; }

        /// <summary>
        /// Deslocamento em X.
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Deslocamento em Y.
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// Deslocamento em Z.
        /// </summary>
        public double OffsetZ { get; set; }

        /// <summary>
        /// Décimo quarto elemento da matriz (geralmente 1 em matrizes homogêneas).
        /// </summary>
        public double M44 { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadMatrix3D"/> como uma matriz identidade.
        /// </summary>
        public CadMatrix3D()
        {
            M11 = M22 = M33 = M44 = 1.0; // Inicializa a matriz identidade
            M12 = M13 = M14 = 0.0;
            M21 = M23 = M24 = 0.0;
            M31 = M32 = M34 = 0.0;
            OffsetX = OffsetY = OffsetZ = 0.0;
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CadMatrix3D"/> com valores especificados.
        /// </summary>
        /// <param name="values">Uma matriz 4x4 representada como um array bidimensional.</param>
        public CadMatrix3D(double[,] values)
        {
            M11 = values[0, 0];
            M12 = values[0, 1];
            M13 = values[0, 2];
            M14 = values[0, 3];

            M21 = values[1, 0];
            M22 = values[1, 1];
            M23 = values[1, 2];
            M24 = values[1, 3];

            M31 = values[2, 0];
            M32 = values[2, 1];
            M33 = values[2, 2];
            M34 = values[2, 3];

            OffsetX = values[3, 0];
            OffsetY = values[3, 1];
            OffsetZ = values[3, 2];
            M44 = values[3, 3]; // Normalmente 1.0 para matrizes homogêneas
        }




        /// <summary>
        /// Multiplica esta matriz por outra matriz.
        /// </summary>
        /// <param name="other">A outra matriz a ser multiplicada.</param>
        /// <returns>A matriz resultante da multiplicação.</returns>
        public CadMatrix3D Multiply(CadMatrix3D other)
        {
            var result = new CadMatrix3D();

            result.M11 = M11 * other.M11 + M12 * other.M21 + M13 * other.M31;
            result.M12 = M11 * other.M12 + M12 * other.M22 + M13 * other.M32;
            result.M13 = M11 * other.M13 + M12 * other.M23 + M13 * other.M33;
            result.M14 = M11 * other.M14 + M12 * other.M24 + M13 * other.M34 + OffsetX;

            result.M21 = M21 * other.M11 + M22 * other.M21 + M23 * other.M31;
            result.M22 = M21 * other.M12 + M22 * other.M22 + M23 * other.M32;
            result.M23 = M21 * other.M13 + M22 * other.M23 + M23 * other.M33;
            result.M24 = M21 * other.M14 + M22 * other.M24 + M23 * other.M34 + OffsetY;

            result.M31 = M31 * other.M11 + M32 * other.M21 + M33 * other.M31;
            result.M32 = M31 * other.M12 + M32 * other.M22 + M33 * other.M32;
            result.M33 = M31 * other.M13 + M32 * other.M23 + M33 * other.M33;
            result.M34 = M31 * other.M14 + M32 * other.M24 + M33 * other.M34 + OffsetZ;

            result.M44 = 1; // Normalmente mantido como 1 em transformações homogêneas

            return result;
        }

        /// <summary>
        /// Aplica a matriz a um ponto 3D.
        /// </summary>
        /// <param name="point">O ponto a ser transformado.</param>
        /// <returns>O ponto transformado.</returns>
        public CadPoint3D TransformPoint(CadPoint3D point)
        {
            double[] vector = new double[] { point.X, point.Y, point.Z, 1 };
            double[] result = new double[4];

            result[0] = M11 * vector[0] + M12 * vector[1] + M13 * vector[2] + M14 + OffsetX;
            result[1] = M21 * vector[0] + M22 * vector[1] + M23 * vector[2] + M24 + OffsetY;
            result[2] = M31 * vector[0] + M32 * vector[1] + M33 * vector[2] + M34 + OffsetZ;
            result[3] = 1; // Para manter o ponto homogêneo

            return new CadPoint3D(result[0], result[1], result[2]);
        }

        // Se o Xbim estiver disponível, inclua este construtor e método relacionados ao XbimMatrix3D
#if XBIM_AVAILABLE

        /// <summary>
        /// Converte esta matriz para uma matriz do tipo XbimMatrix3D.
        /// </summary>
        /// <returns>A matriz convertida para XbimMatrix3D.</returns>
        public XbimMatrix3D ToXbimMatrix3D()
        {
            return new XbimMatrix3D
            {
                M11 = this.M11,
                M12 = this.M12,
                M13 = this.M13,
                M14 = this.M14,

                M21 = this.M21,
                M22 = this.M22,
                M23 = this.M23,
                M24 = this.M24,

                M31 = this.M31,
                M32 = this.M32,
                M33 = this.M33,
                M34 = this.M34,

                OffsetX = this.OffsetX,
                OffsetY = this.OffsetY,
                OffsetZ = this.OffsetZ,
                M44 = this.M44
            };
        }

        /// <summary>
        /// Construtor que cria uma instância de CadMatrix3D a partir de um XbimMatrix3D.
        /// </summary>
        /// <param name="matrix">A matriz XbimMatrix3D a ser convertida.</param>
        public CadMatrix3D(XbimMatrix3D matrix)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;
            M14 = matrix.M14;
            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;
            M24 = matrix.M24;
            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
            M34 = matrix.M34;
            OffsetX = matrix.OffsetX;
            OffsetY = matrix.OffsetY;
            OffsetZ = matrix.OffsetZ;
            M44 = matrix.M44;
        }

#endif
    }

    #endregion

    #region Revit

    // Converter para milimetros as unidades internas do Revit
    public interface IRevitForm
    {
        public bool IsSolid { get; set; }
        public string CadCurveType { get; }
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
    }

    public class RevitFormExtrusion : IRevitForm
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "extrusion";
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
        public double StartOffset { get; set; }
        public double EndOffset { get; set; }
        public bool IsSolid { get; set; }
        public List<List<ICadCurve3D>> Profile { get; } = new List<List<ICadCurve3D>>();
        public RevitFormExtrusion() { }
        public RevitFormExtrusion(CadPoint3D boundingBoxMax, CadPoint3D boundingBoxMin, double startOffset, double endOffset, bool isSolid, List<List<ICadCurve3D>> profile)
        {
            BoundingBoxMax = boundingBoxMax;
            BoundingBoxMin = boundingBoxMin;
            StartOffset = startOffset;
            EndOffset = endOffset;
            IsSolid = isSolid;
            Profile = profile;
        }
    }

    public class RevitFormBlend : IRevitForm
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "blend";
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
        public double BottomOffset { get; set; }
        public double TopOffset { get; set; }
        public bool IsSolid { get; set; }
        public List<List<ICadCurve3D>> BottomProfile { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> TopProfile { get; } = new List<List<ICadCurve3D>>();
        public RevitFormBlend() { }
        public RevitFormBlend(CadPoint3D boundingBoxMax, CadPoint3D boundingBoxMin, double bottomOffset, double topOffset, bool isSolid, List<List<ICadCurve3D>> bottomProfile, List<List<ICadCurve3D>> topProfile)
        {
            BoundingBoxMax = boundingBoxMax;
            BoundingBoxMin = boundingBoxMin;
            BottomOffset = bottomOffset;
            TopOffset = topOffset;
            IsSolid = isSolid;
            BottomProfile = bottomProfile;
            TopProfile = topProfile;
        }
    }

    public class RevitFormRevolution : IRevitForm
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "revolution";
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
        public CadLine3D Axis { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public bool IsSolid { get; set; }
        public List<List<ICadCurve3D>> Profile { get; } = new List<List<ICadCurve3D>>();
        public RevitFormRevolution() { }
        public RevitFormRevolution(CadPoint3D boundingBoxMax, CadPoint3D boundingBoxMin, CadLine3D axis, double startAngle, double endAngle, bool isSolid, List<List<ICadCurve3D>> profile)
        {
            BoundingBoxMax = boundingBoxMax;
            BoundingBoxMin = boundingBoxMin;
            Axis = axis;
            StartAngle = startAngle;
            EndAngle = endAngle;
            IsSolid = isSolid;
            Profile = profile;
        }
    }

    public class RevitFormSweep : IRevitForm
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "revolution";
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
        public List<List<ICadCurve3D>> Path3d { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> PathSketch { get; } = new List<List<ICadCurve3D>>();
        public bool IsSolid { get; set; }
        public List<List<ICadCurve3D>> ProfileSketch { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> ProfileSymbol { get; } = new List<List<ICadCurve3D>>();
        public RevitFormSweep() { }

        public RevitFormSweep(CadPoint3D boundingBoxMax, CadPoint3D boundingBoxMin, List<List<ICadCurve3D>> path3d, List<List<ICadCurve3D>> pathSketch, bool isSolid, List<List<ICadCurve3D>> profileSketch, List<List<ICadCurve3D>> profileSymbol)
        {
            BoundingBoxMax = boundingBoxMax;
            BoundingBoxMin = boundingBoxMin;
            Path3d = path3d;
            PathSketch = pathSketch;
            IsSolid = isSolid;
            ProfileSketch = profileSketch;
            ProfileSymbol = profileSymbol;
        }
    }

    public class RevitFormSweptBlend : IRevitForm
    {
        /// <summary>
        /// Discriminador de tipo.
        /// </summary>
        public string CadCurveType { get; } = "revolution";
        public CadPoint3D BoundingBoxMax { get; set; }
        public CadPoint3D BoundingBoxMin { get; set; }
        public List<ICadCurve3D> SelectedPath { get; } = new List<ICadCurve3D>();
        public List<ICadCurve3D> PathSketch { get; } = new List<ICadCurve3D>();
        public bool IsSolid { get; set; }
        public List<List<ICadCurve3D>> TopProfile { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> TopProfileSymbol { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> BottomProfile { get; } = new List<List<ICadCurve3D>>();
        public List<List<ICadCurve3D>> BottomProfileSymbol { get; } = new List<List<ICadCurve3D>>();
        public RevitFormSweptBlend() { }
        public RevitFormSweptBlend(CadPoint3D boundingBoxMax, CadPoint3D boundingBoxMin, List<ICadCurve3D> selectedPath, List<ICadCurve3D> pathSketch, bool isSolid,
            List<List<ICadCurve3D>> topProfile, List<List<ICadCurve3D>> topProfileSymbol,
            List<List<ICadCurve3D>> bottomProfile, List<List<ICadCurve3D>> bottomProfileSymbol)
        {
            BoundingBoxMax = boundingBoxMax;
            BoundingBoxMin = boundingBoxMin;
            SelectedPath = selectedPath;
            PathSketch = pathSketch;
            IsSolid = isSolid;
            TopProfile = topProfile;
            TopProfileSymbol = topProfileSymbol;
            BottomProfile = bottomProfile;
            BottomProfileSymbol = bottomProfileSymbol;
        }
    }

    #endregion

    public enum CurveType
    {
        Line,
        Arc,
        Circle
    }

    public class CadCurve
    {
        public CurveType CurveType;
        public CadPoint3D StartPoint { get; set; }
        public CadPoint3D EndPoint { get; set; }
        public CadPoint3D Center { get; set; }
        public CadPoint3D Middle { get; set; }
        public CadPoint3D Normal { get; set; }
        public double Radius { get; set; }

        public CadCurve()
        {
        }
    }

    public class CadLine : CadCurve
    {
        public CadLine()
        {
        }
        public CadLine(CadPoint3D startPoint, CadPoint3D endPoint)
        {
            CurveType = CurveType.Line;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }

    public class CadArc : CadCurve
    {
        public CadArc()
        {
        }
        public CadArc(CadPoint3D startPoint, CadPoint3D endPoint, CadPoint3D center, CadPoint3D middle, CadPoint3D normal, double radius)
        {
            CurveType = CurveType.Arc;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Center = center;
            Middle = middle;
            Normal = normal;
            Radius = radius;
        }
    }

    public class CadCircle : CadCurve
    {
        public CadCircle()
        {
        }
        public CadCircle(CadPoint3D center, CadPoint3D normal, double radius)
        {
            CurveType = CurveType.Circle;
            Center = center;
            Normal = normal;
            Radius = radius;
        }
    }

    #endregion

    #region CLASSE CADGEOMETRY E SUBCLASSES

    public abstract class CadGeometry
    {
    }
    public class LineGeometry : CadGeometry
    {
        public CadLine Line { get; set; }
        public LineGeometry()
        {
        }
        public LineGeometry(CadLine line)
        {
            Line = line;
        }

    }
    public class ArcGeometry : CadGeometry
    {
        public CadArc Arc { get; set; }
        public ArcGeometry()
        {
        }
        public ArcGeometry(CadArc arc)
        {
            Arc = arc;
        }

    }
    public class CircleGeometry : CadGeometry
    {
        public CadCircle CadCircle { get; set; }
        public CircleGeometry()
        {
        }
        public CircleGeometry(CadCircle cadCircle)
        {
            CadCircle = cadCircle;
        }
    }
    public class PolylineGeometry : CadGeometry
    {
        public List<CadCurve> ListCurves { get; set; }

        public PolylineGeometry()
        {
        }
        public PolylineGeometry(List<CadCurve> listCurves)
        {
            ListCurves = listCurves;
        }
    }
    public class PointGeometry : CadGeometry
    {
        public CadPoint3D Point { get; set; }
        public PointGeometry()
        {
        }
        public PointGeometry(CadPoint3D point)
        {
            Point = point;
        }
    }
    public class BlockGeometry : CadGeometry
    {
        public CadPoint3D Point { get; set; }
        public BlockGeometry()
        {
        }
        public BlockGeometry(CadPoint3D point)
        {
            Point = point;
        }
    }
    public class LoopsGeometry : CadGeometry
    {
        public List<List<CadCurve>> ListListCurves { get; set; }
        public LoopsGeometry()
        {
        }
        public LoopsGeometry(List<List<CadCurve>> listListCurves)
        {
            ListListCurves = listListCurves;
        }
    }

    public class BoundingBoxGeometry : CadGeometry
    {
        public List<CadPoint3D> BoundingBoxPoints { get; set; }
        public BoundingBoxGeometry()
        {
        }
        public BoundingBoxGeometry(List<CadPoint3D> boundingBoxPoints)
        {
            BoundingBoxPoints = boundingBoxPoints;
        }
    }
    #endregion
}
