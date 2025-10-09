# 迅雷数据库TaskDb.dat表结构说明

## TaskBase表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| TaskId | BIGINT | 1 |  |  | 任务唯一标识 |
| Type | INT | 0 |  |  | 任务类型(1:HTTP 2:BT 3:磁力) |
| Status | INT | 0 |  |  | 任务状态(5:下载中 7:已删除 8:已完成) |
| StatusChangeTime | BIGINT | 0 |  |  | 状态变更时间(时间戳) |
| SavePath | NVARCHAR | 0 |  |  | 文件保存路径 |
| TotalReceiveSize | BIGINT | 0 |  |  | 总接收大小(字节) |
| TotalSendSize | BIGINT | 0 |  |  | 总发送大小(字节) |
| TotalReceiveValidSize | BIGINT | 0 |  |  | 有效接收大小(字节) |
| TotalUploadSize | BIGINT | 0 |  |  | 总上传大小(字节) |
| CreationTime | BIGINT | 0 |  |  | 创建时间(时间戳) |
| FileCreated | INT | 0 |  |  | 文件是否已创建 |
| CompletionTime | BIGINT | 0 |  |  | 完成时间(时间戳) |
| DownloadingPeriod | BIGINT | 0 |  |  | 下载周期 |
| RemovingToRecycleTime | BIGINT | 0 |  |  | 移入回收站时间 |
| FailureErrorCode | INT | 0 |  |  | 失败错误代码 |
| Url | NVARCHAR | 0 |  |  | 下载链接 |
| ReferenceUrl | NVARCHAR | 0 |  |  | 参考链接 |
| ResourceSize | BIGINT | 0 |  |  | 资源大小(字节) |
| Name | NVARCHAR | 0 |  |  | 文件名 |
| Cid | BLOB | 0 |  |  | 内容ID |
| Gcid | BLOB | 0 |  |  | 全局内容ID |
| Description | NVARCHAR | 0 |  |  | 描述信息 |
| CategoryId | INT | 0 |  |  | 分类ID |
| ResourceQueryCid | BLOB | 0 |  |  | 资源查询内容ID |
| CreationRequestType | INT | 0 |  |  | 创建请求类型 |
| StartMode | INT | 0 |  |  | 启动模式 |
| NamingType | INT | 0 |  |  | 命名类型 |
| StatisticsReferenceUrl | NVARCHAR | 0 |  |  | 统计参考链接 |
| UserRead | INT | 0 |  |  | 用户已读标记 |
| FileSafetyFlag | INT | 0 |  |  | 文件安全标记 |
| Playable | INT | 0 |  |  | 可播放标记 |
| BlockInfo | BLOB | 0 |  |  | 块信息 |
| OpenOnComplete | INT | 0 |  |  | 完成后打开标记 |
| SpecialType | INT | 0 |  |  | 特殊类型 |
| Proxy | BLOB | 0 |  |  | 代理信息 |
| OriginReceiveSize | BIGINT | 0 |  |  | 原始接收大小 |
| P2pReceiveSize | BIGINT | 0 |  |  | P2P接收大小 |
| P2sReceiveSize | BIGINT | 0 |  |  | P2S接收大小 |
| OfflineReceiveSize | BIGINT | 0 |  |  | 离线接收大小 |
| VipReceiveSize | BIGINT | 0 |  |  | VIP接收大小 |
| VipResourceEnableNecessary | INT | 0 |  |  | VIP资源启用必要标记 |
| ConsumedVipSize | BIGINT | 0 |  |  | 消耗的VIP大小 |
| Forbidden | INT | 0 |  |  | 禁止标记 |
| OptionalChannelDataSize | BLOB | 0 |  |  | 可选通道数据大小 |
| OwnerProductId | INT | 0 |  |  | 所有者产品ID |
| UserData | BLOB | 0 |  |  | 用户数据 |
| UrlCodePage | INT | 0 |  |  | URL代码页 |
| ReferenceUrlCodePage | INT | 0 |  |  | 参考链接代码页 |
| StatisticsReferenceUrlCodePage | INT | 0 |  |  | 统计参考链接代码页 |
| GroupTaskId | BIGINT | 0 |  |  | 组任务ID |
| DownloadSubTask | INT | 0 |  |  | 下载子任务 |
| TagValue | INT | 0 |  |  | 标签值 |
| InnerNatReceiveSize | BIGINT | 0 |  |  | 内部NAT接收大小 |
| AdditionFlag | INT | 0 |  |  | 附加标记 |
| ProductInfo | NVARCHAR | 0 |  |  | 产品信息 |
| Origin | NVARCHAR | 0 |  |  | 来源 |
| FreeDcdnReceiveSize | BIGINT | 0 |  |  | 免费DCDN接收大小 |
| PlayPosition | INT | 0 |  |  | 播放位置 |
| Duration | INT | 0 |  |  | 持续时间 |

## AccelerateTask表
| 字段名 | 类型 | 主键 | 非空 | 默认值 |
|--------|------|------|------|--------|
| LocalTaskId | BIGINT | 1 |  |  |
| LocalSubFileIndex | INT | 1 |  |  |
| ... | ... | ... | ... | ... |

## BtFile表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| BtTaskId | BIGINT | 0 |  |  | BT任务ID |
| Status | INT | 0 |  |  | 文件状态 |
| FileIndex | INT | 0 |  |  | 文件索引 |
| FilePath | NVARCHAR | 0 |  |  | 文件路径(可能包含乱码) |
| ... | ... | ... | ... | ... | ... |

## ThunderPanTask表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| id | INTEGER | 1 |  |  | 主键ID |
| TaskId | BIGINT | 1 |  |  | 关联任务ID |
| PanTaskId | varchar(255) | 0 |  |  | 云盘任务ID |
| ... | ... | ... | ... | ... | ... |

## user_file表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| kind | varchar(16) | 0 |  |  | 文件类型(file/folder) |
| name | varchar(1024) | 0 |  |  | 文件名/文件夹名 |
| id | varchar(40) | 1 |  |  | 文件唯一ID |
| parent_id | varchar(40) | 0 |  |  | 父文件夹ID |
| user_id | varchar(128) | 0 |  |  | 用户ID |
| size | varchar(20) | 0 |  |  | 文件大小 |
| web_content_link | TEXT | 0 |  |  | 文件下载链接 |
| thumbnail_link | TEXT | 0 |  |  | 缩略图链接 |
| ... | ... | ... | ... | ... | ... |

## Category表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| CategoryId | INT | 0 |  | 1 | 分类ID |
| ParentId | INT | 0 |  | 0 | 父分类ID |
| NextId | INT | 0 |  | 0 | 下一个分类ID |
| Status | INT | 0 |  | 0 | 分类状态 |
| Name | NVARCHAR | 0 |  | 0 | 分类名称 |
| Description | NVARCHAR | 0 |  | 0 | 分类描述 |
| DefaultPath | NVARCHAR | 0 |  | 0 | 默认保存路径 |
| IconIndex | INT | 0 |  | 0 | 图标索引 |
| IconPath | NVARCHAR | 0 |  | 0 | 图标路径 |
| UserName | NVARCHAR | 0 |  | 0 | 用户名(猜测) |
| Password | NVARCHAR | 0 |  | 0 | 密码(猜测) |
| AccountPassword | BLOB | 0 |  | 0 | 账户密码(加密) |
| Password_v2 | BLOB | 0 |  | 0 | 密码V2(加密) |

## Module表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| ModuleId | BIGINT | 1 |  |  | 模块ID |
| Name | NVARCHAR | 0 |  |  | 模块名称 |
| Version | NVARCHAR | 0 |  |  | 模块版本 |
| Description | NVARCHAR | 0 |  |  | 模块描述 |
| ... | ... | ... | ... | ... | ... |

## P2spTask表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| TaskId | BIGINT | 1 |  |  | 任务ID |
| ... | ... | ... | ... | ... | ... |

## EmuleTask表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| TaskId | BIGINT | 1 |  |  | 任务ID |
| FileHash | BLOB | 0 |  |  | 文件哈希 |
| ConfigureFilePath | NVARCHAR | 0 |  |  | 配置文件路径 |
| VideoHeadFirstTime | INT | 0 |  |  | 视频头优先时间 |
| VideoHeadFirstStatus | INT | 0 |  |  | 视频头优先状态 |

## GroupTask表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| TaskId | BIGINT | 1 |  |  | 任务ID |
| SubTaskOrder | BLOB | 0 |  |  | 子任务顺序 |
| DownloadInOrder | INT | 0 |  |  | 是否顺序下载 |
| DownloadingSubTaskCount | INT | 0 |  |  | 正在下载的子任务数 |
| SmartMoveTimeout | INT | 0 |  |  | 智能移动超时时间 |

## BaoTuan表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| taskId | BIGINT | 1 |  |  | 任务ID |
| userId | NVARCHAR | 1 |  |  | 用户ID |
| extData | NVARCHAR | 0 |  |  | 扩展数据 |

## PanPluginUploadHistory表
| 字段名 | 类型 | 主键 | 非空 | 默认值 | 备注 |
|--------|------|------|------|--------|------|
| id | INTEGER | 1 | 1 |  | 主键ID |
| name | VARCHAR | 0 |  | '' | 文件名 |
| uploadType | VARCHAR | 0 |  | 'resume' | 上传类型 |
| filePath | VARCHAR | 0 |  | '' | 文件路径 |
| ... | ... | ... | ... | ... | ... |

注：部分字段含义为猜测，实际功能可能需要进一步验证。