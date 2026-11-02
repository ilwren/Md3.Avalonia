# Md3.Avalonia 0.5.6-preview.1 发布说明（草案）

## 版本规则

本轮从 `0.5.0-preview.1` 升至 `0.5.6-preview.1`。补丁位增加 6，分别对应六个新增的独立控件：

1. `MdCommitGraph`
2. `MdContributionGrid`
3. `MdPagination`
4. `MdMaskedTextBox`
5. `MdFileSystemPicker`
6. `MdGridSplitter`

`MdCarousel` 和 `MdStepper` 是已有控件的能力扩展，因此不重复计入版本增量。此前尚未发布的 `MdVideoPlayer` 与 `Md3.Avalonia.LibVLC` 已完整删除，也不计入新增控件。

## 新增控件

- **`MdCommitGraph`**：支持多分支与 merge 拓扑、连续 lane 连线、HEAD/标签、提交选择和 `CommitClicked`。
- **`MdContributionGrid`**：按日期显示 contribution heatmap，支持可配置单元格尺寸、触摸目标、日期选择、点击事件和 tooltip。
- **`MdPagination`**：支持数字与 dots 模式、前后翻页、窗口化页码以及水平/垂直 dots 视觉。
- **`MdMaskedTextBox`**：提供 mask、literal、prompt、完成状态和 Material filled/outlined 字段外观。
- **`MdFileSystemPicker`**：直接浏览真实文件系统路径，支持文件/文件夹模式、筛选、多选、可取消选择、本地化、Standard/Powerline breadcrumb 和响应式布局；不调用 Avalonia `StorageProvider`。
- **`MdGridSplitter`**：保留 Avalonia 原生行列调整、约束、键盘与 preview 行为，并增加 Material 状态层、透明命中轨道、可选 handle 和等分重置。

## 现有控件改进

- `MdStepper` 增加水平布局。
- `MdCarousel`、Pagination dots 和相关 Gallery 交互得到多轮触摸、切页与动效修正。
- `MdTextBox` 与 `MdSearchBar` 显式同步 logical `CaretIndex` 和视觉 `TextPresenter` caret，覆盖反向选择、Shift 选择、双击、真实 pointer drag 后继续输入。
- SimpleDialog、About/License dialog、Gallery 图标与托盘、自定义路径选择等场景得到修正。

## 已删除

此前开发中的视频播放功能未进入本版本：

- 删除 `MdVideoPlayer`、`MdPlaybackRequest` 和 `MdPlaybackRequestEventArgs`；
- 删除 `Md3.Avalonia.LibVLC` 项目及 LibVLCSharp 依赖；
- 删除 Gallery 视频页面、本地 MP4、生成/启动脚本、测试与研究文档；
- NuGet 包数量恢复为 18，全部继续声明 trimming/AOT 兼容。

## 安装

以下版本需在发布到 NuGet 后才能安装：

```xml
<PackageReference Include="Md3.Avalonia" Version="0.5.6-preview.1" />
<PackageReference Include="Md3.Avalonia.Icons" Version="0.5.6-preview.1" />
```

所有仓库内 NuGet 项目统一使用 `0.5.6-preview.1`。发布前仍需完成 `docs/RELEASE_VALIDATION.md` 中的平台与辅助技术人工验收。
