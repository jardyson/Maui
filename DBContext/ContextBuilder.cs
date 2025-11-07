using Microsoft.EntityFrameworkCore;

namespace DBContext
{
    public class ContextBuilder
    {
        static ContextBuilder _;
        public static ContextBuilder Instance => _ ?? (_ = new ContextBuilder());
        //public SLContext BuildSL()
        //{
        //    var str = @"server=localhost;user=root;password=admin;database=sl;sslmode=none";
        //    var contextOptions = new DbContextOptionsBuilder<SLContext>()
        //        .UseMySql(str, ServerVersion.AutoDetect(str)).Options;

        //    return new SLContext(contextOptions);
        //}
    }
}
