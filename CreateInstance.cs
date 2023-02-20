using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    public class CreateInstance
    {
        public static List<Wall> CreateWalls(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = GetLevels(commandData)
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();

            Level level2 = GetLevels(commandData)
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();

            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }

            transaction.Commit();
            return walls;
        }

        public static void AddDoor(ExternalCommandData commandData, Level level1, Wall wall)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 2134 мм"))
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
                doorType.Activate();


            Transaction transaction = new Transaction(doc, "Создание двери");
            transaction.Start();
            doc.Create.NewFamilyInstance(point, doorType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            transaction.Commit();
        }

        public static void AddWindows(ExternalCommandData commandData, Level level1, List<Wall> walls)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 1830 мм"))
                .Where(x => x.FamilyName.Equals("Фиксированные"))
                .FirstOrDefault();

            List<XYZ> points = new List<XYZ>();

            foreach (var wall in walls)
            {
                LocationCurve hostCurve = wall.Location as LocationCurve;
                XYZ point1 = hostCurve.Curve.GetEndPoint(0);
                XYZ point2 = hostCurve.Curve.GetEndPoint(1);
                XYZ point = (point1 + point2) / 2;
                points.Add(point);
            }


            if (!windowType.IsActive)
                windowType.Activate();


            Transaction transaction = new Transaction(doc, "Создание двери");
            transaction.Start();

            for (int i = 0; i < walls.Count; i++)
            {
                //сразу поднимем каждое окно на 1 фут))
                doc.Create.NewFamilyInstance(points[i], windowType, walls[i], level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural).get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(1);
            }    

            transaction.Commit();
        }


        public static List<Level> GetLevels(ExternalCommandData commandData) 
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> listLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            return listLevel;
        }
           
    }
}
