namespace SLDTO
{
    public class BaseStoreDTO<T>
        where T : class
    {
        public Int64 id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public T jason { get; set; }
        public Int64 solutionid { get; set; }
        public string remark { get; set; }
        public DateTime createdate { get; set; }
    }
}
