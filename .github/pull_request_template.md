## Summary / 变更摘要

<!-- Describe what changed and why. / 请说明改了什么以及原因。 -->

## Related issue / 关联 Issue

<!-- Use Fixes #123 or Related to #123 when applicable. / 如适用，填写 Fixes #123 或 Related to #123。 -->

## NuGet package inventory / NuGet 包清单

This repository currently maintains 17 NuGet package projects. If this PR adds, removes, or renames a package, update the package docs and both issue-form package lists.
当前仓库维护 17 个 NuGet 包项目。如果本 PR 新增、移除或重命名包，请同步更新包文档和两个 Issue 表单中的包选项。

## Change type / 变更类型

- [ ] Bug fix / 缺陷修复
- [ ] Feature / 功能
- [ ] Documentation / 文档
- [ ] Refactor / 重构
- [ ] Build or CI / 构建或 CI

## Validation / 验证

<!-- List commands run and relevant results. If checks were not run, explain why. / 列出执行的命令和结果；未运行时请说明原因。 -->

- [ ] Relevant `dotnet build` / `dotnet test`
- [ ] `python3 scripts/check-api-doc-coverage.py` (when public API or API docs change)
- [ ] `python3 scripts/lint-design-tokens.py` (when theme resources or tokens change)
- [ ] Not run (explain below) / 未运行（请说明）

## Platforms checked / 已验证平台

- [ ] Windows
- [ ] macOS
- [ ] Linux
- [ ] Android
- [ ] Not applicable or not tested (explain above) / 不适用或未测试（请在上方说明）

## Screenshots or attachments / 截图或附件

<!-- For visual changes, include before/after screenshots or a short recording. For non-visual changes, write “N/A”. / UI 变更请附前后截图或短录屏；非视觉变更填写 N/A。 -->

## Checklist / 检查清单

- [ ] I reviewed the diff and kept the change focused. / 已检查 diff，变更范围明确。
- [ ] Public API changes include XML documentation and API reference updates. / 公共 API 变更已补充 XML 注释和 API 文档。
- [ ] User-facing behavior changes include relevant tests and documentation. / 面向用户的行为变更已补充相关测试与文档。
- [ ] If package IDs or count changed, I updated the 17-package inventory and issue-form options. / 若包 ID 或数量有变化，已同步更新 17 包清单与 Issue 表单选项。
- [ ] I did not include secrets, generated build output, or unrelated changes. / 未提交密钥、构建产物或无关变更。
