using System.Collections.Immutable;
using System.Linq;

namespace LINQClassesLib
{
    public interface ISolver
    {
        /// <summary>
        /// Самое длинное название файла без учета пути к файлу
        /// </summary>
        /// <param name="diDirectory"> Директория для поиска</param>
        /// <param name="longextNameLength"> Длина максимального по длине названия </param>
        /// <returns> Название самого длинного названия файла в директории </returns>
        string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength);
        /// <summary>
        /// Поиск N файлов наибильшего размера
        /// </summary>
        /// <param name="diDirectory"> Директория для поиска </param>
        /// <param name="number"> Число искомых файлов</param>
        /// <returns> Массив имен самых больших файлов </returns>
        string[] GetLongestNames(DirectoryInfo diDirectory, int number);
        /// <summary>
        /// Наибольшая группа файлов, созданных в фиксированный интервал времени
        /// </summary>
        /// <param name="diDirectory"> Директория для поиска</param>
        /// <param name="dtStart"> Начало интервала </param>
        /// <param name="dtEnd"> Конец интервала </param>
        /// <returns> Массив имен файлов, созданных в данный интервал </returns>
        FileInfo[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd);
        /// <summary>
        /// Распределение файлов по расширениям
        /// </summary>
        /// <param name="diDirectory"> Директория для поиска </param>
        /// <returns> Коллекцию элементов с ключом - 
        /// расширением файла и значением - количеством этих файлов</returns>
        Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory);
    }
    public class LinqSolver : ISolver
    {
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            FileInfo[] files = diDirectory.GetFiles();
            var maxName = files.Max(file => file.Name);
            longextNameLength = maxName.Length;
            return maxName;
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            FileInfo[] files = diDirectory.GetFiles();
            var orderedFiles = from file in files
                               orderby file.Name.Length ascending
                               select file;
            var topN = orderedFiles.TakeLast(number);
            var topNArr = topN.ToArray();
            string[] topNNames = new string[number];
            for (int i = 0; i < topNNames.Length; i++)
            {
                topNNames[i] = topNArr[i].Name;
            }
            return topNNames;
        }
        public FileInfo[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            FileInfo[] files = diDirectory.GetFiles();
            var setOfFilesByInterval = from file in files
                               where file.CreationTime >= dtStart && file.CreationTime  <= dtEnd
                               orderby file.CreationTime ascending
                               select file;
            return setOfFilesByInterval.ToArray();
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            FileInfo[] files = diDirectory.GetFiles();
            var extAlloc = files.GroupBy(file => file.Extension)
                                .Select(alloc => 
                                        new { 
                                                Extension = alloc.Key, 
                                                Count = alloc.Count() 
                                            })
                                .OrderBy(cnt => cnt.Count);
            return extAlloc.ToDictionary(e => e.Extension, c => c.Count);
        }
    }
    public class NoLinqSolver : ISolver
    {
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            FileInfo[] files = diDirectory.GetFiles();
            longextNameLength = -1;
            int maxLengthIndex = -1;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Length > longextNameLength)
                {
                    longextNameLength = files[i].Name.Length;
                    maxLengthIndex = i;
                }
            }
            return files[maxLengthIndex].Name;
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            FileInfo[] files = diDirectory.GetFiles();
            string[] longestNames = new string[number];
            FileInfo fiTemp;
            for (int i = 0; i < files.Length; i++)
            {
                for (int j = i; j < files.Length - 1; j++)
                {
                    if (files[j].Length > files[j+1].Length)
                    {
                        fiTemp = files[j];
                        files[j] = files[j+1];
                        files[j+1] = fiTemp;
                    }
                }
            }
            int ind = files.Length - number - 1;
            for (int i = 0; i < longestNames.Length; i++)
            {
                longestNames[i] = files[ind].Name;
            }

            return longestNames;
        }
        public FileInfo[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            FileInfo[] files = diDirectory.GetFiles();
            FileInfo fiTemp;
            for (int i = 0; i < files.Length; i++)
            {
                for (int j = i; j < files.Length - 1; j++)
                {
                    if (files[j].CreationTime > files[j + 1].CreationTime)
                    {
                        fiTemp = files[j];
                        files[j] = files[j + 1];
                        files[j + 1] = fiTemp;
                    }
                }
            }
            int indStart = -1;
            int indEnd = -1;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].CreationTime <= dtEnd)
                {
                    indEnd = i;
                }
                if (indStart == -1 && files[i].CreationTime >= dtStart)
                {
                    indStart = i;
                }
            }
            FileInfo[] setOfFiles;
            if (indStart > 0 && indEnd > 0)
            {
                if (indStart == indEnd)
                {
                    setOfFiles = new FileInfo[1];
                    setOfFiles[0] = files[indStart];
                }
                else
                {
                    setOfFiles = new FileInfo[indEnd - indStart + 1];
                    int ind = 0;
                    for (int i = indStart; i <= indEnd; i++)
                    {
                        setOfFiles[ind] = files[i];
                    }
                }
                return setOfFiles;
            }
            else
            {
                return new FileInfo[0];
            }
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            var extAlloc = new Dictionary<string, int>();
            foreach (FileInfo file in diDirectory.GetFiles()) 
            {
                if (!extAlloc.ContainsKey(file.Extension))
                {
                    extAlloc.Add(file.Extension, 1);
                }
                else
                {
                    extAlloc[file.Extension]++;
                }
            }
            return extAlloc;
        }
    }


    public class DirectorySolver
    {

        public ISolver Solver { private get; set; }

        public DirectorySolver(ISolver solver)
        {
            Solver = solver;
        }
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            return Solver.GetLongestName(diDirectory, out longextNameLength);
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            return Solver.GetLongestNames(diDirectory, number);
        }
        public FileInfo[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            return Solver.GetSetOfFilesByInterval(diDirectory, dtStart, dtEnd);
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            return Solver.ExtensionAllocation(diDirectory);
        }
        
    }
}