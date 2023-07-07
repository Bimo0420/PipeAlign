using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeAlign
{
    public class PipeData
    {
        public static Dictionary<String, double> ChanelType = new Dictionary<String, double>()
        {
            {"В стальных футлярах и ж.б. обойме", 0},
            {"Бесканальная прокладка на ж.б. основании", -0.065},
            {"В канале типа НКЛ МКЛ на ж.б. основании", -0.135},
            {"В монолитном ж.б. канале", -0.093},
            {"В сборном канале на ж.б. основании", -0.093},
        };
    }
}
