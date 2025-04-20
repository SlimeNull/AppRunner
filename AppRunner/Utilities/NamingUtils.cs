using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRunner.Utilities
{
    public static class NamingUtils
    {
        public static string CreateCopyName(string originName, Predicate<string> nameValidator)
        {
            var baseName = originName;

            StringBuilder sb = new StringBuilder();

            for (int i = originName.Length - 1; i >= 0; i--)
            {
                var currentChar = originName[i];
                if (currentChar < '0' || currentChar > '9')
                {
                    baseName = originName.Substring(0, i + 1);
                    break;
                }

                sb.Insert(0, currentChar);
            }

            int currentNumber = 0;
            if (sb.Length > 0)
            {
                currentNumber = int.Parse(sb.ToString());
            }

            if (!baseName.EndsWith("_Copy"))
            {
                baseName = originName;
                currentNumber = 0;
            }
            else
            {
                baseName = baseName.Substring(0, baseName.Length - 5);
            }

            string newName;

            do
            {
                newName = baseName + "_Copy";
                currentNumber++;

                if (currentNumber > 1)
                {
                    newName += currentNumber;
                }
            }
            while (nameValidator != null && !nameValidator.Invoke(newName));

            return newName;
        }
    }
}
