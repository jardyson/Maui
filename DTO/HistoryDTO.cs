namespace SLDTO
{
    public class HistoryDTO<T>
    {
        public Int64 id { get; set; }
        public T jasonvalue { get; set; }
        public DateTime CreateDate { get; set; }
        public Int64? solutionid { get; set; }
    }
}
