# 迅雷数据库TaskDb.dat结构说明

## 主要表结构

### TaskBase表
```sql
CREATE TABLE `TaskBase` (
  `TaskId` BIGINT,
  `Type` INT,
  `Status` INT,
  `SavePath` NVARCHAR(2000),
  `CreationTime` BIGINT,
  `CompletionTime` BIGINT,
  `FailureErrorCode` INT,
  `Url` NVARCHAR(2000),
  `Name` NVARCHAR(2000)
);
```

### 其他相关表
```sql
CREATE TABLE `AccelerateTask` (
  `LocalTaskId` BIGINT,
  `AcclerateTaskId` BIGINT,
  `LocalSubFileIndex` INT,
  `UserData` NVARCHAR,
  primary key(LocalTaskId,LocalSubFileIndex)
);

CREATE TABLE `PanPluginUploadHistory` (
  `id` integer not null primary key autoincrement,
  `name` varchar(255) default '',
  `uploadType` varchar(255) default 'resume',
  `filePath` varchar(255) default '',
  `icon` varchar(255) default '',
  `type` varchar(255) default '',
  `size` integer default '0',
  `parentId` varchar(255) default '',
  `progress` integer default '0',
  `token` json default '',
  `createTime` varchar(255) default CURRENT_TIMESTAMP,
  `completeTime` varchar(255),
  `updateTime` varchar(255) default CURRENT_TIMESTAMP,
  `uploadStatus` varchar(255) default 'pending',
  `userId` varchar(255) default '',
  `version` integer default '0',
  `fileId` varchar(255) default '',
  `hash` varchar(255) default '',
  `baseDir` varchar(255) default '',
  `driveFileExist` boolean default '1',
  `localFileExist` boolean default '1',
  `uploadNetSpeed` varchar(255) default '0',
  `mime_type` varchar(255) default '',
  `from` varchar(255) default '',
  `errMsg` varchar(255) default '',
  `extras` json default '{}'
);

CREATE TABLE `user_file` (
  `kind` varchar(16),
  `name` varchar(1024),
  `id` varchar(40),
  `parent_id` varchar(40),
  `user_id` varchar(128),
  `size` varchar(20),
  `revision` varchar(20),
  `file_extension` text,
  `mime_type` varchar(128),
  `starred` boolean,
  `web_content_link` text,
  `created_time` varchar(40),
  `modified_time` varchar(40),
  `icon_link` text,
  `thumbnail_link` text,
  `md5_checksum` text,
  `hash` text,
  `links` json,
  `phase` varchar(32),
  `audit` json,
  `medias` json,
  `folder_type` varchar(16),
  `trashed` boolean,
  `delete_time` varchar(40),
  `original_url` text,
  `params` json,
  `original_file_index` varchar(40),
  `space` text,
  `apps` json default '[]',
  primary key (`id`)
);
```

## 字段说明

### TaskBase表字段
- `TaskId`: 任务ID，BT/磁力任务中相同ID表示属于同一父任务
- `Type`: 任务类型
  - 0: HTTP/FTP
  - 1: BT
  - 2: 磁力
  - 3: 电驴
- `Status`: 任务状态
  - 5: 下载中
  - 7: 已删除
  - 8: 已完成
- `SavePath`: 保存路径
- `CreationTime`: 创建时间(Unix时间戳)
- `CompletionTime`: 完成时间(Unix时间戳)
- `Url`: 下载URL
  - 包含下载源信息、文件ID、用户ID等
  - 格式示例: `http://...?fid=...&mid=666&threshold=251&tid=...`
- `Name`: 任务名称
  - 包含文件名和可能的乱码字符

## 表关系分析

### TaskBase与AccelerateTask
- 通过`TaskId`与`LocalTaskId`关联
- AccelerateTask表可能用于记录加速任务信息

### 数据示例
```
TaskId: 116800995
Type: 0 (HTTP/FTP)
Status: 5 (下载中)
Url: http://...?fid=wtwt3Lj5lKijxfNjdRnMt2y2A3aXT1MI...
Name: 3DMGAME-Samurai.Shodown.V.Special.Neo.Geo.25th.Anniversary.Edition.RIP-Unleashed.rar
```

### 其他表说明
- `AccelerateTask`: 猜测为加速任务关联表
- `PanPluginUploadHistory`: 猜测为迅雷云盘上传历史记录
- `user_file`: 猜测为用户文件信息表