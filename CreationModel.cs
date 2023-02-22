using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //создаем стены и получаем их список
            List<Wall> walls = CreateInstance.CreateWalls(commandData);

            //получаем первый уровень, нужен для создания двери
            Level level1 = CreateInstance.GetLevels(commandData).Where(x => x.Name.Equals("Уровень 1")) as Level;

            //получаем верхний уровень для создания крыши
            Level level2 = CreateInstance.GetLevels(commandData).Where(x => x.Name.Equals("Уровень 2")) as Level;

          

            //устанавливаем дверь в первую стену в списке
            CreateInstance.AddDoor(commandData, level1,walls[0]);

            //создаем окна для трех стен кроме первой
            CreateInstance.AddWindows(commandData, level1, walls.GetRange(1, 3));

            //создаем крышу
            CreateInstance.AddRoof2(commandData, walls);

            return Result.Succeeded;
        }
    }
}
