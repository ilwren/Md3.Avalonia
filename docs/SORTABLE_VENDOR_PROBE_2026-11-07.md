# Sortable.Avalonia vendor probe / 第三方拖拽库探针（2026-11-07）

[English](#english) | [简体中文](#简体中文)

## English

### Decision

**Defer integration.** `Sortable.Avalonia` is an attached behavior for ordinary Avalonia
`ItemsControl`s, not a themed board control; the host owns card/list presentation. It has no
upstream control-theme seam that warrants a new Material theme package. Its 2.3.1 snapshot targets
Avalonia 12.0.0 and declares AOT compatibility, but it predates a confirmed retention fix. The
2.4.0 and 2.4.1 source snapshots instead require Avalonia 12.1.3, above this repository's 12.1.2
baseline. We will neither silently upgrade Avalonia nor ship the known-leaking 2.3.1 library in a
product package. The research probe references 2.3.1 **only from the headless test project**.

The fixed source snapshots are [2.3.1](https://github.com/russkyc/sortable-avalonia/tree/52631c35d902fbf3dff5f8e801d9da049a5edd24),
[2.4.0](https://github.com/russkyc/sortable-avalonia/tree/87859b3406c504c4785aa1650129697b203281e6) and
[2.4.1](https://github.com/russkyc/sortable-avalonia/tree/de439ffb835eb14fc7fcf2a2f014df4e2456043f).
All are MIT, copyright 2026 Russell Camo. No vendor source has been copied into this repository.

### Evidence and limitations

* [Upstream issue #25](https://github.com/russkyc/sortable-avalonia/issues/25) reports retention
  in 2.3.1; [commit 4c5af085](https://github.com/russkyc/sortable-avalonia/commit/4c5af085b6c8f6407227b1a484b8dd4fea03d50e)
  introduces weak tracking after 2.3.1 and is included in the 2.4.0 source tag. In the 2.3.1
  source, static tracking holds enabled `ItemsControl`s even if detached. Explicitly disabling
  **both** `Sortable` and `Droppable` unregisters that tracking, but this is not a general proof
  of leak-free usage or a recommendation to deploy the old version.
* The 2.3.1 [`ApplyDropMutation`](https://github.com/russkyc/sortable-avalonia/blob/52631c35d902fbf3dff5f8e801d9da049a5edd24/Sortable.Avalonia/SortableMutationHelper.cs)
  inserts before validating/removing the old index: an invalid old index returns `false` after
  changing the target. Rejection before mutation is different from atomic rollback on a stale or
  invalid index. The same code is present in the inspected 2.4.1 snapshot; it must not be treated
  as transactional. The host owns validation and safe collection mutations.
* Both inspected snapshots log the dragged item's string representation to standard output in
  drop code. This can disclose sensitive item text. Version 2.4.1 is not a complete fix for
  mutation safety or logging merely because it fixes retention and newer touch handling.
* We found no keyboard/automation reorder implementation in the 2.3.1 behavior source. Hosts need
  a separate, accessible move/reorder path; a raw drag surface is not an accessible board.
  The later 2.4.1 tag describes additional touch/capture fixes, not present in the old tag.
* `scripts/check-sortable-probe-package.py` checks the restored NuGet identity and declared Avalonia
  dependency in CI. `SortableVendorProbeTests` characterizes the actual test package: accepted and
  rejected mutations, a stale-index partial mutation, the strong-reference lifecycle, and headless mouse cross-list drags for acceptance, rejection and group isolation. These are narrow probes, not a blanket certification of input
  devices, threading, accessibility, visual styling or production safety. Tests clean up tracking.

No Gallery example or vendor theme package is added, because those would imply support for a
version with known blockers. The Gallery's existing AOT smoke does **not** reference this test-only
package. Upstream `IsAotCompatible` is a declaration, not this project's native-AOT test result;
we have not run the vendor's drag path under native AOT. Before any future integration, verify
published NuGet dependencies, 12.1.3 baseline upgrade consequences, leak and touch fixes,
transaction-safe mutations, logging/privacy, keyboard alternatives, real devices and native AOT.

## 简体中文

### 决策

**暂缓集成。** `Sortable.Avalonia` 是附加在普通 `ItemsControl` 上的行为，而不是自带外观的看板控件；列表和卡片由宿主负责，没有足以建立独立 Material 主题包的上游主题缝。2.3.1 源码引用 Avalonia 12.0.0 并声明兼容 AOT，但早于上游已确认的引用泄漏修复；2.4.0/2.4.1 源码则要求 Avalonia 12.1.3，高于本仓库 12.1.2。不能悄悄升级基线，也不能把已知泄漏版本作为产品依赖。2.3.1 仅供 headless 测试工程调查使用，不进入 Gallery 或任何可分发包。

固定版本、MIT 许可和来源见英文节；本仓库未复制上游代码。

### 证据与边界

* 上游 [#25](https://github.com/russkyc/sortable-avalonia/issues/25) 报告 2.3.1 的泄漏，修复提交在 2.3.1 之后，并被 2.4.0 源码标签包含。2.3.1 的静态跟踪会持有启用的控件；显式关闭 `Sortable` **及** `Droppable` 会移除该跟踪，但不等于证明所有路径无泄漏，也不是对旧版生产使用的推荐。
* 2.3.1 的 `ApplyDropMutation` 先向目标插入，再处理源索引；源索引无效时，即使返回 `false`，目标也可能已经改变。预先拒绝与操作失败后的原子回滚不是一回事。审查的 2.4.1 快照仍有相同写法，不能将其当作事务；宿主需负责验证和安全的集合变更。
* 两个所审查快照都在标准输出记录拖动对象的字符串表示，存在敏感内容泄露风险。源码中未发现 2.3.1 自带的键盘/自动化重排路径，宿主需提供可访问的替代操作；2.4.1 还包含旧版没有的触摸与捕获修复。
* CI 的 NuGet 元数据检查及 `SortableVendorProbeTests` 针对实际测试包，覆盖接收/拒绝、无效源索引、引用跟踪，以及 headless 鼠标跨列表拖拽的接收、拒绝与组隔离。它们不是对所有设备、线程、无障碍或生产安全的认证。

因为存在明确阻塞，本轮不添加 Gallery 示例或 vendor 主题。现有 Gallery 原生 AOT 冒烟**不包含**这个仅测试依赖；上游的 `IsAotCompatible` 声明不等同于本仓库完成原生 AOT 拖拽测试。后续仍需核验实际 NuGet 依赖、升级 12.1.3 的影响、泄漏/触摸修复、事务与日志、键盘替代、真实设备和原生 AOT。
