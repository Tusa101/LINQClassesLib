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
            Stopwatch stopwatch = new();
            stopwatch.Start();
            FileInfo[] files = diDirectory.GetFiles();
            longextNameLength = files.Max(file => file.Name.Length);
            int temp = longextNameLength;
            var maxName = (from file in files
                          where file.Name.Length == temp
                          select file.Name).First();
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return maxName;
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
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
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return topNNames;
        }
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
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
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return fileStruct;
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            FileInfo[] files = diDirectory.GetFiles();
            var extAlloc = files.GroupBy(file => file.Extension)
                                .Select(alloc => 
                                        new { 
                                                Extension = alloc.Key, 
                                                Count = alloc.Count() 
                                            })
                                .OrderBy(cnt => cnt.Count);
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return extAlloc.ToDictionary(e => e.Extension, c => c.Count);
        }
    }
    public class NoLinqSolver : ISolver
    {
        public string GetLongestName(DirectoryInfo diDirectory, out int longextNameLength)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
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
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return files[maxLengthIndex].Name;
        }
        public string[] GetLongestNames(DirectoryInfo diDirectory, int number)
        {
            GC.Collect();
            Stopwatch stopwatch = new();
            stopwatch.Start();
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
                Debug.WriteLine(longestNames[i]);

            }
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
            return longestNames;
        }

        
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
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
                stopwatch.Stop();
                Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
                return setOfFiles;
            }
            else
            {
                stopwatch.Stop();
                Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
                return new FileStruct[0];
            }

        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

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
            stopwatch.Stop();
            Debug.WriteLine("Time elapsed " + stopwatch.ElapsedMilliseconds);
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
        public FileStruct[] GetSetOfFilesByInterval(DirectoryInfo diDirectory, DateTime dtStart, DateTime dtEnd)
        {
            return Solver.GetSetOfFilesByInterval(diDirectory, dtStart, dtEnd);
        }
        public Dictionary<string, int> ExtensionAllocation(DirectoryInfo diDirectory)
        {
            return Solver.ExtensionAllocation(diDirectory);
        }
        
    }
}