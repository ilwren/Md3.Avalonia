# 发布验证清单

本文记录发布前需要执行的自动化检查和人工平台验收。CI 结果与设备验收应基于准备发布的提交，不沿用历史运行结果。

## 自动化检查

CI 工作流覆盖以下项目：

| 检查 | 命令或工作流 |
|---|---|
| 字体来源与二进制校验 | `python3 scripts/verify-fonts.py` |
| API 类型与接口文档覆盖 | `python3 scripts/check-api-doc-coverage.py` |
| 设计令牌检查 | `python3 scripts/lint-design-tokens.py` |
| 桌面 Gallery 与 Headless 测试 | `.github/workflows/ci.yml` |
| 六个 NuGet 包 | `scripts/build-nuget.sh`、`.github/workflows/build-nuget.yml` |
| 多平台 Gallery 构建 | `.github/workflows/build-gallery.yml` |
| Android UI 回归 | `.github/workflows/android-test.yml`，单独执行且不替代真机验收 |

发布前核对当前提交上的 CI 结果，并确认包版本、包依赖、XML API 文档与第三方声明一致。规范验证的范围和限制见 [`SPEC_VERIFICATION.md`](SPEC_VERIFICATION.md)。

## 人工验收

对本次发布涉及的平台执行适用项目，并记录设备、系统版本、测试人员与日期。未执行的项目保持未签署状态。

- [ ] Android ARM64：安装启动、触摸与滚动、IME、弹层、系统返回、旋转及恢复。
- [ ] Windows：窗口 chrome、键盘与指针操作、DPI 切换、Narrator。
- [ ] macOS：窗口与输入行为、VoiceOver。
- [ ] Linux：X11/Wayland 下的启动与输入、Orca。
- [ ] Light、Dark、System 主题及中英文界面。
- [ ] 高对比度与减少动态效果设置（适用时）。

## AOT 与裁剪

六个包声明 `IsTrimmable` 与 `IsAotCompatible`。现有 AOT 探测工作流针对 Linux 桌面 Gallery；只有实际运行目标 RID 的发布与启动检查通过后，才记录该目标的 AOT 验证结果。不得将一个平台的结果推定为其他平台已验证。