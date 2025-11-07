using GlobalDeclar;

namespace Global.Interface
{
    public interface IModelView
    {
        static IModelView Instance { get; set; }
        /// <summary>
        /// 监控Modbus的状态值
        /// </summary>
        int? MoniotrReadValue { get; set; }
        FlowEnum FlowType { get; set; }
        string Check(string str1, string str2, string str3, string str4);
    }
}
