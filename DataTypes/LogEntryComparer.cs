using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes
{
    public class LogEntryKeyComparer : IComparer<string>
    {
        public  int Compare(string x, string y)
        {
            // Split the key into three parts: date (fr-FR format), time, and username.
            string[] lhsbuffer = x.Split(' ');
            string[] rhsbuffer = y.Split(' ');

            // Build datetime values on the date and the time
            DateTime dateValueLHS = DateTime.Parse(lhsbuffer[0] + ' ' + lhsbuffer[1]/*, new CultureInfo("fr-FR", false)*/);
            DateTime dateValueRHS = DateTime.Parse(rhsbuffer[0] + ' ' + rhsbuffer[1]/*, new CultureInfo("fr-FR", false)*/);

            // Compare the date and time, most of the time (haha get it?) datetimes will be unique.
            int result = DateTime.Compare(dateValueRHS, dateValueLHS);
            //int result = DateTime.Compare(dateValueLHS, dateValueRHS); <-- WHY IS THIS WRONG???

            // On the off chance that the drops were at exactly the same time, the order is decided based on username. 
            // On the infinitely miniscule chance that both drops are from the same person, string's compare will
            // return the zero needed.
            if (result == 0)
                return string.Compare(lhsbuffer[2], rhsbuffer[2]);
            else
                return result;
       }
    }
}