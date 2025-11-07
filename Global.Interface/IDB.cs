using GlobalDeclar;
using SLDTO;
using System.Collections.ObjectModel;

namespace Global.Interface
{
    public interface IDB<T, FlowModel>
    {
        public FlowModel model { get; }
        ObservableCollection<IPointtableView> Points { get; set; }
        /// <summary>
        /// 今日产生的记录
        /// </summary>
        ObservableCollection<HistoryDTO<T>> TodayHistory { get; set; }
        /// <summary>
        /// 历史查询记录
        /// </summary>
        ObservableCollection<HistoryDTO<T>> History { get; set; }

        public ObservableCollection<item> IPs { get; set; }
        public ObservableCollection<item> Prints { get; set; }
        public ObservableCollection<item> QRCodeGuns { get; set; }
    }
}