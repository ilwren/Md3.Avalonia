# 规范验证说明

仓库包含针对部分 Material Design 3 令牌、几何、渲染与动效行为的验证工具。验证结果用于回归与差异追踪，不构成 Material Design 或 Flutter 官方认证，也不能替代真实平台验收。

## 基线与覆盖

- `spec-snapshot/manifest.json` 记录外部来源及版本；`spec-snapshot/tokens.json` 保存本地验证使用的期望值；
- `spec-snapshot/conformance-policy.json` 配置验证层、阈值、豁免及 `enforce` 模式；
- `docs/spec-baseline-manifest.json` 固定引用基线；`docs/spec-test-matrix.json` 记录需求、平台、测试依据与状态。

基线快照不由本仓库的控件资源自动生成。修改快照时应同时审查来源和对应验证。

## 验证层

| 层级 | 入口 | 检查内容 |
|---|---|---|
| L0 | `scripts/lint-design-tokens.py` | AXAML 令牌、资源引用、资源包含与期望值映射 |
| L1 | `MdTokenConformanceTests` | 运行时资源系统中的令牌值 |
| L2 | `MdGeometryConformanceTests` | 控件模板布局及几何约束 |
| L3 | `MdVisualGoldenTests` | 已提交参考图的渲染差异 |
| L5 | `MdMotionConformanceTests` | 弹簧参数与动效运行行为 |

当前策略为 `enforce`：高置信度且未豁免的差异会阻止对应检查通过；中置信度差异仍作为提示。缺少金图时会生成候选结果，不自动将候选加入基线。

## 运行

```bash
# 静态检查，无需 .NET SDK
python3 scripts/lint-design-tokens.py

# 规范相关的 Headless 测试
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release \
  --filter "FullyQualifiedName~Md3.Avalonia.HeadlessTests.Spec"

# 完整验证脚本；有 .NET SDK 时会运行测试，否则将其标记为 not_run
python3 scripts/run-spec-verification.py
```

测试报告写入 `artifacts/spec/`；`run-spec-verification.py` 生成的机器可读结果写入 `docs/reports/test-spec-execution-results.json`。这些结果随运行时生成，不应作为固定版本的当前状态提交。

## 结果边界

- Headless/Skia 测试不验证 Android、Windows、macOS 或 Linux 桌面后端的完整行为；
- 未执行的设备、辅助技术及平台检查应保留为未验证，不得以编译成功替代；
- 令牌或几何测试通过只说明相应基线及检查通过，不意味着所有组件完全符合规范；
- 机器报告不输出或背书整体合规率。
