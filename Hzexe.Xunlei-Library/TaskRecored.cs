using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzexe.Xunlei_Library
{
    /// <summary>
    /// 迅雷下载任务记录类，用于表示迅雷下载任务的基本信息
    /// </summary>
    public class TaskRecored
    {
        /// <summary>
        /// 内部构造函数，从TaskBase对象创建TaskRecored实例
        /// </summary>
        /// <param name="b">TaskBase对象，包含任务的基本信息</param>
        internal TaskRecored(Prot.TaskBase b)
        {
            TaskId = b.TaskId;
            Type = b.Type;
            Status = (DownloadStatus)b.Status;
            SavePath = b.SavePath?.TrimEnd('\0');
            CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(b.CreationTime).LocalDateTime;
            CompletionTime = DateTimeOffset.FromUnixTimeMilliseconds(b.CompletionTime).LocalDateTime;
            FailureErrorCode = b.FailureErrorCode;
            Name = b.Name?.TrimEnd('\0');
            Url = b.Url?.TrimEnd('\0');
            
            // 添加TotalReceiveSize和ResourceSize字段的映射
            TotalReceiveSize = b.TotalReceiveSize;
            ResourceSize = b.ResourceSize;
            
            // 添加其他相关字段的映射
            TotalSendSize = b.TotalSendSize;
            TotalReceiveValidSize = b.TotalReceiveValidSize;
            TotalUploadSize = b.TotalUploadSize;
            FileCreated = b.FileCreated;
            DownloadingPeriod = b.DownloadingPeriod;
            RemovingToRecycleTime = b.RemovingToRecycleTime;
            ReferenceUrl = b.ReferenceUrl?.TrimEnd('\0');
            Cid = b.Cid;
            Gcid = b.Gcid;
            Description = b.Description?.TrimEnd('\0');
            CategoryId = b.CategoryId;
            ResourceQueryCid = b.ResourceQueryCid;
            CreationRequestType = b.CreationRequestType;
            StartMode = b.StartMode;
            NamingType = b.NamingType;
            StatisticsReferenceUrl = b.StatisticsReferenceUrl?.TrimEnd('\0');
            UserRead = b.UserRead;
            FileSafetyFlag = b.FileSafetyFlag;
            Playable = b.Playable;
            BlockInfo = b.BlockInfo;
            OpenOnComplete = b.OpenOnComplete;
            SpecialType = b.SpecialType;
            Proxy = b.Proxy;
            OriginReceiveSize = b.OriginReceiveSize;
            P2pReceiveSize = b.P2pReceiveSize;
            P2sReceiveSize = b.P2sReceiveSize;
            OfflineReceiveSize = b.OfflineReceiveSize;
            VipReceiveSize = b.VipReceiveSize;
            VipResourceEnableNecessary = b.VipResourceEnableNecessary;
            ConsumedVipSize = b.ConsumedVipSize;
            Forbidden = b.Forbidden;
            OptionalChannelDataSize = b.OptionalChannelDataSize;
            OwnerProductId = b.OwnerProductId;
            UserData = b.UserData;
            UrlCodePage = b.UrlCodePage;
            ReferenceUrlCodePage = b.ReferenceUrlCodePage;
            StatisticsReferenceUrlCodePage = b.StatisticsReferenceUrlCodePage;
            GroupTaskId = b.GroupTaskId;
            DownloadSubTask = b.DownloadSubTask;
            TagValue = b.TagValue;
            InnerNatReceiveSize = b.InnerNatReceiveSize;
            AdditionFlag = b.AdditionFlag;
            ProductInfo = b.ProductInfo?.TrimEnd('\0');
            Origin = b.Origin?.TrimEnd('\0');
            FreeDcdnReceiveSize = b.FreeDcdnReceiveSize;
            PlayPosition = b.PlayPosition;
            Duration = b.Duration;
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public TaskRecored() { 
        
        }

        /// <summary>
        /// 任务唯一标识符
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 任务类型(1:HTTP 2:BT 3:磁力)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 任务状态(5:下载中 7:已删除 8:已完成)
        /// </summary>
        public DownloadStatus Status { get; set; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string? SavePath { get; set; }

        /// <summary>
        /// 总接收大小(字节)
        /// </summary>
        public long TotalReceiveSize { get; set; }

        /// <summary>
        /// 总发送大小(字节)
        /// </summary>
        public long TotalSendSize { get; set; }

        /// <summary>
        /// 有效接收大小(字节)
        /// </summary>
        public long TotalReceiveValidSize { get; set; }

        /// <summary>
        /// 总上传大小(字节)
        /// </summary>
        public long TotalUploadSize { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 文件是否已创建(0:未创建 1:已创建)
        /// </summary>
        public int FileCreated { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime CompletionTime { get; set; }

        /// <summary>
        /// 下载周期
        /// </summary>
        public long DownloadingPeriod { get; set; }

        /// <summary>
        /// 移入回收站时间(时间戳)
        /// </summary>
        public long RemovingToRecycleTime { get; set; }

        /// <summary>
        /// 失败错误代码
        /// </summary>
        public int FailureErrorCode { get; set; }

        /// <summary>
        /// 下载链接
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 参考链接
        /// </summary>
        public string? ReferenceUrl { get; set; }

        /// <summary>
        /// 资源大小(字节)
        /// </summary>
        public long ResourceSize { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 内容ID(BLOB类型)
        /// </summary>
        public byte[]? Cid { get; set; }

        /// <summary>
        /// 全局内容ID(BLOB类型)
        /// </summary>
        public byte[]? Gcid { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 资源查询内容ID(BLOB类型)
        /// </summary>
        public byte[]? ResourceQueryCid { get; set; }

        /// <summary>
        /// 创建请求类型
        /// </summary>
        public int CreationRequestType { get; set; }

        /// <summary>
        /// 启动模式
        /// </summary>
        public int StartMode { get; set; }

        /// <summary>
        /// 命名类型
        /// </summary>
        public int NamingType { get; set; }

        /// <summary>
        /// 统计参考链接
        /// </summary>
        public string? StatisticsReferenceUrl { get; set; }

        /// <summary>
        /// 用户已读标记
        /// </summary>
        public int UserRead { get; set; }

        /// <summary>
        /// 文件安全标记
        /// </summary>
        public int FileSafetyFlag { get; set; }

        /// <summary>
        /// 可播放标记
        /// </summary>
        public int Playable { get; set; }

        /// <summary>
        /// 块信息(BLOB类型)
        /// </summary>
        public byte[]? BlockInfo { get; set; }

        /// <summary>
        /// 完成后打开标记
        /// </summary>
        public int OpenOnComplete { get; set; }

        /// <summary>
        /// 特殊类型
        /// </summary>
        public int SpecialType { get; set; }

        /// <summary>
        /// 代理信息(BLOB类型)
        /// </summary>
        public byte[]? Proxy { get; set; }

        /// <summary>
        /// 原始接收大小
        /// </summary>
        public long OriginReceiveSize { get; set; }

        /// <summary>
        /// P2P接收大小
        /// </summary>
        public long P2pReceiveSize { get; set; }

        /// <summary>
        /// P2S接收大小
        /// </summary>
        public long P2sReceiveSize { get; set; }

        /// <summary>
        /// 离线接收大小
        /// </summary>
        public long OfflineReceiveSize { get; set; }

        /// <summary>
        /// VIP接收大小
        /// </summary>
        public long VipReceiveSize { get; set; }

        /// <summary>
        /// VIP资源启用必要标记
        /// </summary>
        public int VipResourceEnableNecessary { get; set; }

        /// <summary>
        /// 消耗的VIP大小
        /// </summary>
        public long ConsumedVipSize { get; set; }

        /// <summary>
        /// 禁止标记
        /// </summary>
        public int Forbidden { get; set; }

        /// <summary>
        /// 可选通道数据大小(BLOB类型)
        /// </summary>
        public byte[]? OptionalChannelDataSize { get; set; }

        /// <summary>
        /// 所有者产品ID
        /// </summary>
        public int OwnerProductId { get; set; }

        /// <summary>
        /// 用户数据(BLOB类型)
        /// </summary>
        public byte[]? UserData { get; set; }

        /// <summary>
        /// URL代码页
        /// </summary>
        public int UrlCodePage { get; set; }

        /// <summary>
        /// 参考链接代码页
        /// </summary>
        public int ReferenceUrlCodePage { get; set; }

        /// <summary>
        /// 统计参考链接代码页
        /// </summary>
        public int StatisticsReferenceUrlCodePage { get; set; }

        /// <summary>
        /// 组任务ID
        /// </summary>
        public long GroupTaskId { get; set; }

        /// <summary>
        /// 下载子任务
        /// </summary>
        public int DownloadSubTask { get; set; }

        /// <summary>
        /// 标签值
        /// </summary>
        public int TagValue { get; set; }

        /// <summary>
        /// 内部NAT接收大小
        /// </summary>
        public long InnerNatReceiveSize { get; set; }

        /// <summary>
        /// 附加标记
        /// </summary>
        public int AdditionFlag { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public string? ProductInfo { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string? Origin { get; set; }

        /// <summary>
        /// 免费DCDN接收大小
        /// </summary>
        public long FreeDcdnReceiveSize { get; set; }

        /// <summary>
        /// 播放位置
        /// </summary>
        public int PlayPosition { get; set; }

        /// <summary>
        /// 持续时间
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 状态变更时间(时间戳)
        /// </summary>
        public long StatusChangeTime { get; set; }
    }
}
