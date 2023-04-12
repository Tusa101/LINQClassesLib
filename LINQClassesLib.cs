using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
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
        FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd);
        /// <summary>
        /// Распределение файлов по расширениям
        /// </summary>
        /// <param name="diDirectory"> Директория для поиска </param>
        /// <returns> Коллекцию элементов с ключом - 
        /// расширением файла и значением - количеством этих файлов</returns>
        Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory);
    }
    public struct FileStruct
    {
        public string Name;
        public DateTime CreationTime;
        public FileStruct(string name, DateTime creationTime)
        {
            Name = name;
            CreationTime = creationTime;
        }
    }
    public class LinqSolver : ISolver
    {
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            FileInfo[] files = diDirectory.GetFiles();
            longextNameLength = files.Max(file => file.Name.Length);
            int temp = longextNameLength;
            var maxName = (from file in files
                          where file.Name.Length == temp
                          select file.Name).First();
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
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            FileInfo[] files = diDirectory.GetFiles();
            var setOfFilesByInterval = from file in files
                               where file.CreationTime >= dtStart && file.CreationTime  <= dtEnd
                               orderby file.CreationTime ascending
                               select file;
            var tempArr = setOfFilesByInterval.ToArray();
            FileStruct[] fileStruct = new FileStruct[tempArr.Length];
            for (int i = 0; i < setOfFilesByInterval.Count(); i++)
            {
                fileStruct[i] = new FileStruct(tempArr[i].Name, tempArr[i].CreationTime);
            }
            return fileStruct;
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
            GC.Collect();
            FileInfo[] files = diDirectory.GetFiles();
            string[] filesNames = new string[files.Length];
            for (int i = 0; i < filesNames.Length; i++)
            {
                filesNames[i] = files[i].Name;
            }
            string[] longestNames = new string[number>filesNames.Length? filesNames.Length:number];
            string fiTemp;
            for (int i = 0; i < filesNames.Length; i++)
            {
                for (int j = i; j < filesNames.Length - 1; j++)
                {
                    if (filesNames[j].Length > filesNames[j+1].Length)
                    {
                        fiTemp = filesNames[j];
                        filesNames[j] = filesNames[j+1];
                        filesNames[j+1] = fiTemp;
                    }
                }
            }
            int ind = filesNames.Length - number > 0? filesNames.Length - number:0;
            for (int i = 0; i < longestNames.Length; i++)
            {
                longestNames[i] = filesNames[ind];
                ind++;
            }
            return longestNames;
        }

        
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            FileInfo[] files = diDirectory.GetFiles();
            List<FileStruct> fileStruct = new();
            for (int i = 0; i < files.Length; i++)
            {
                fileStruct.Add(new FileStruct ( files[i].Name, files[i].CreationTime ));
            }
            FileStruct fsTemp;
            for (int i = 0; i < fileStruct.Count; i++)
            {
                for (int j = i; j < fileStruct.Count - 1; j++)
                {
                    if (fileStruct[j].CreationTime > fileStruct[j + 1].CreationTime)
                    {
                        fsTemp = fileStruct[j];
                        fileStruct[j] = fileStruct[j + 1];
                        fileStruct[j + 1] = fsTemp;
                    }
                }
            }
            int indStart = -1;
            int indEnd = -1;
            for (int i = 0; i < fileStruct.Count; i++)
            {
                if (files[i].CreationTime <= dtEnd)
                {
                    indEnd = i;
                }
                if (indStart == -1 && fileStruct[i].CreationTime >= dtStart)
                {
                    indStart = i;
                }
            }
            FileStruct[] setOfFiles;
            if (indStart >= 0 && indEnd > 0)
            {
                if (indStart == indEnd)
                {
                    setOfFiles = new FileStruct[1];
                    setOfFiles[0] = fileStruct[indStart];
                }
                else
                {
                    setOfFiles = new FileStruct[indEnd - indStart + 1];
                    int ind = 0;
                    for (int i = indStart; i <= indEnd; i++)
                    {
                        setOfFiles[ind] = fileStruct[i];
                        ind++;
                    }
                }
                return setOfFiles;
            }
            else
            {
                return new FileStruct[0];
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
        private long[,] _times = new long[2,4];
        public bool checkedLINQ = false;
        public long[,] Times
        {
            get
            {
                return _times;
            }
            set
            {
                _times = value;
            }
        }

        public ISolver Solver { private get; set; }


        public DirectorySolver(ISolver solver)
        {
            Solver = solver;
        }
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var res = Solver.GetLongestName(diDirectory, out longextNameLength);
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            if (!checkedLINQ)
            {
                _times[0, 0] = stopwatch.ElapsedMilliseconds;    
            }
            else 
            {
                _times[1, 0] = stopwatch.ElapsedMilliseconds;
            }

            return res;
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var res = Solver.GetLongestNames(diDirectory, number);
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            if (!checkedLINQ)
            {
                _times[0, 1] = stopwatch.ElapsedMilliseconds;
            }
            else
            {
                _times[1, 1] = stopwatch.ElapsedMilliseconds;
            }
            return res;
        }
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var res = Solver.GetSetOfFilesByInterval(diDirectory, dtStart, dtEnd);
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            if (!checkedLINQ)
            {
                _times[0, 2] = stopwatch.ElapsedMilliseconds;
            }
            else
            {
                _times[1, 2] = stopwatch.ElapsedMilliseconds;
            }
            return res;
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var res = Solver.ExtensionAllocation(diDirectory);
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            if (!checkedLINQ)
            {
                _times[0, 3] = stopwatch.ElapsedMilliseconds;
            }
            else
            {
                _times[1, 3] = stopwatch.ElapsedMilliseconds;
            }
            return res;
        }
        
    }
}