# Md3.Avalonia 0.5.0-preview.2 更新说明（草案）

**状态：尚未发布。** 当前 NuGet.org 上最新已发布版本仍为 [`0.5.0-preview.1`](https://www.nuget.org/packages/Md3.Avalonia/0.5.0-preview.1)。在 `0.5.0-preview.2` 实际发布前，安装示例继续使用 `.1`。

## 文档与包元数据

- 校正 NuGet 发布状态、版本号、包数量、目标框架与发行日期等说明；区分已发布的 `.1` 和当前尚未发布的 `.2` 源码版本。
- 更新 README、NuGet 包指南、入门指南、兼容性说明和发布验证记录；安装命令明确指定已发布版本。
- 将 NuGet `PackageReleaseNotes` 改为 `0.5.0-preview.2`，内容聚焦于文档完善和发行信息校对。
- 在共享构建属性中为全部包加入 GitHub 源代码仓库地址与 `git` 类型，后续生成的 `.nuspec` 将包含 `<repository>` 元素；已发布的 `.1` 包无法原地补写此元数据。

## 版本元数据

- 18 个 NuGet 包项目的版本统一更新为 `0.5.0-preview.2`。
- Android Gallery 显示版本、发行工作流默认版本及包构建脚本示例同步更新。

本草案记录的是文档与版本元数据调整；在 `.2` 发布前，请勿将其当作已发布版本或据此安装 NuGet 包。
