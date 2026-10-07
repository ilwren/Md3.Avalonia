# 本地验证速查

## 拉取
    git fetch origin && git checkout main
    # 或者，带 PR 上下文： gh pr checkout <PR 编号>

## 1. 静态层：不需要 .NET SDK，约 0.1 秒
    python3 scripts/lint-design-tokens.py      # 令牌 lint，报告写到 artifacts/spec/token-lint.md
    python3 scripts/verify-fonts.py            # 字体 SHA / 表 / 字形门禁
    python3 scripts/sync-spec-tokens.py --offline   # 只校验映射表自洽（去掉 --offline 会联网对拍 androidx）

## 2. 构建 + 全量测试：需要 .NET 10 SDK（TFM net10.0，仓库无 global.json，任意 10.x 均可）
    dotnet restore tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj
    dotnet test    tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release

## 3. 只跑 4 个一致性测试
    dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release \
      --filter "FullyQualifiedName~Md3.Avalonia.HeadlessTests.Spec"

## 4. 看结果（artifacts/ 已 gitignore，每次运行重写）
    artifacts/spec/token-lint.md         L0 静态 lint
    artifacts/spec/L1-tokens.md          L1 令牌
    artifacts/spec/L2-geometry.md        L2 几何 + 48dp
    artifacts/spec/L3-visual.md          L3 金图
    artifacts/spec/L5-motion-physics.md  L5 弹簧物理
    artifacts/spec/L5-motion-runtime.md  L5 运行时
    artifacts/spec/baselines-new/*.png   L3 候选图（人工审阅后才拷进 Spec/baselines/）

## 注意事项
* 不要 `dotnet build Md3.Avalonia.sln` —— solution 里含 Gallery.Android，
  缺 android workload 会直接失败。CI 就是按项目 restore/build 的。
* policy 现在是 `enforce`：出现高置信偏差会让测试失败。
  本地想先盘点不想挂，把 spec-snapshot/conformance-policy.json 的 "mode" 临时改回 "bootstrap"。
* 预览 PNG 默认不写盘。需要重新生成： MD3_WRITE_PREVIEWS=1 dotnet test ...
* 武装一张金图：审阅 artifacts/spec/baselines-new/<名字>.png，确认无误后
  拷到 tests/Md3.Avalonia.HeadlessTests/Spec/baselines/<名字>.png 并提交。
  基线 OS 是 Linux；在 Windows/macOS 上含文字的场景不会被门禁。
