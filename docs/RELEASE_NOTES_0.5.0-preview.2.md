# Md3.Avalonia 0.5.0-preview.2 更新说明

## 文档与包元数据

- 更新 NuGet 版本、包数量、目标框架与发行日期等说明，并将安装示例统一为 `0.5.0-preview.2`。
- 更新 README、NuGet 包指南、入门指南、兼容性说明和发布验证记录，确保英文与中文文档一致。
- 将 NuGet `PackageReleaseNotes` 更新为 `0.5.0-preview.2`，内容涵盖 Avalonia 11 迁移、包身份、Gallery 品牌资源、文档完善和发行信息校对。
- 在共享构建属性中为全部包加入 GitHub 源代码仓库地址与 `git` 类型，生成的 `.nuspec` 将包含 `<repository>` 元素；已发布的 `.1` 包无法原地补写此元数据。

## 版本元数据

- 17 个 NuGet 包项目的版本统一更新为 `0.5.0-preview.2`。
- Android Gallery 显示版本、发行工作流默认版本及包构建脚本示例同步更新。

## Avalonia 11 与包身份

- 全部控件、Gallery、测试与打包项目迁移到 Avalonia `11.3.13`；这是所有保留的第三方集成共同支持的最低版本，其中 DataGrid `11.3.13` 的依赖下限最高。
- 当前 17 个 NuGet 包统一使用 `Md3.Avalonia11` / `Md3.Avalonia11.*` 身份；程序集、命名空间、源码路径与 `avares://` 资源 URI 保持不变。
- 保留所有具有 Avalonia 11 兼容版本的第三方主题，并按 Avalonia 11 API 调整窗口装饰、Android 宿主、焦点事件、字形查找、模板遍历、弹层、自动化 peers 与位图保存。
- OlivierJC.AvaloniaRichEditor 没有 Avalonia 11 兼容版本，因此移除其第三方主题包、Gallery 集成、测试、DocFX 与打包入口；Extra 中独立的 `MdRichEditor` 控件不受影响。
- 桌面 Gallery 显式使用 `Tmds.DBus.Protocol 0.21.3` 修复版本，避免 Avalonia.FreeDesktop `11.3.13` 的 `0.21.2` 下限还原出具有已知高危漏洞的版本；Avalonia 最低基线保持 `11.3.13`。

## Gallery 品牌资源

- 主导航、紧凑品牌位、窗口图标与托盘图标统一使用仓库根目录的 `logo.png`，移除旧的紫色星形托盘资源。

本次变更包括 Avalonia 11 迁移、最低共同依赖基线、Gallery 品牌资源、文档完善与版本元数据校对。
