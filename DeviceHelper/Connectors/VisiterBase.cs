using Global.Interface;
using GlobalDeclar;

namespace DeviceHelper
{
    public abstract class VisiterBase
    {
        public int IsErrNotify { get; set; }
        Dictionary<FlowEnum, Func<object, string>> receivedFlowRelation { get; set; } = new Dictionary<FlowEnum, Func<object, string>>();

        public void AddFlowFun(FlowEnum flow, Func<object, string> func)
        {
            receivedFlowRelation.Add(flow, func);
        }
    }
}
