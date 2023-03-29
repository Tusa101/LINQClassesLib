namespace LINQClassesLib
{
    interface ISolver
    {
        /// <summary>
        /// Самое длинное название файла без учета пути к файлу
        /// </summary>
        /// <param name="fiDir"> Директория для поиска</param>
        /// <returns> Название самого длинного названия файла в директории </returns>
        string GetLongestName(FileInfo fiDir);
        /// <summary>
        /// Поиск N файлов наибильшего размера
        /// </summary>
        /// <param name="fiDir"> Директория для поиска </param>
        /// <param name="number"> Число искомых файлов</param>
        /// <returns> Массив имен самых больших файлов </returns>
        string[] GetLongestNamesNum(FileInfo fiDir, int number);
        /// <summary>
        /// Наибольшая группа файлов, созданных в фиксированный интервал времени
        /// </summary>
        /// <param name="fiDir"> Директория для поиска</param>
        /// <param name="dtStart"> Начало интервала </param>
        /// <param name="dtEnd"> Конец интервала </param>
        /// <returns> Массив имен файлов, созданных в данный интервал </returns>
        string[] GetSetOfFilesByInterval(FileInfo fiDir, DateTime dtStart, DateTime dtEnd);
        /// <summary>
        /// Распределение файлов по расширениям
        /// </summary>
        /// <param name="fiDir"> Директория для поиска </param>
        /// <returns> Коллекцию элементов с ключом - 
        /// расширением файла и значением - количеством этих файлов</returns>
        Dictionary<string, int> ExtensionAllocation(FileInfo fiDir);
    }
    public class LinqSolver : ISolver
    {
        public string GetLongestName(FileInfo fiDir)
        {
            return "";
        }
        public string[] GetLongestNamesNum(FileInfo fiDir, int number)
        {
            return new string[number];
        }
        public string[] GetSetOfFilesByInterval(FileInfo fiDir, DateTime dtStart, DateTime dtEnd)
        {
            return new string[0];
        }
        public Dictionary<string, int> ExtensionAllocation(FileInfo fiDir)
        { 
            return new Dictionary<string, int>(); 
        }
    }
    public class NoLinqSolver : ISolver
    {
        public string GetLongestName(FileInfo fiDir)
        {
            return "";
        }
        public string[] GetLongestNamesNum(FileInfo fiDir, int number)
        {
            return new string[number];
        }
        public string[] GetSetOfFilesByInterval(FileInfo fiDir, DateTime dtStart, DateTime dtEnd)
        {
            return new string[0];
        }
        public Dictionary<string, int> ExtensionAllocation(FileInfo fiDir)
        {
            return new Dictionary<string, int>();
        }
    }
}