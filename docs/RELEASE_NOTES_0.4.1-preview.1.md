# Md3.Avalonia 0.4.1-preview.1 release notes

发布日期：2026-10-06

这是一个**缺陷修复版本**。重点是上一轮 Gallery 实跑评审中反馈的四类桌面端显示问题，以及新增一个
opt-in 的富文本主题包和已验证的原生 AOT 支持。

> 版本号从 `0.4.0-preview.1` 进到 `0.4.1-preview.1`：没有破坏性改动，公共 API 面只做了加法，
> 所以走 patch 位。**仍为预览版**——API 在首个稳定版前仍可能做保持兼容的打磨。

## 修复

### Carousel 在宽窗口下只占半条轨道

排布规则在**任何窗口宽度下都只产出「一个大项 + 一个中项」**，第三项起一律压到 56 DIP 的小 keyline。
实测 1188 DIP 的视口里，四个 item 只占 584 DIP，剩下 605 DIP 全空：

```
修复前   280 | 8 | 168 | 8 | 56 | 8 | 56 | ──── 空 605 ────
```

这套规则本身没错，只是**只为手机窄屏写的**——所以 Android 上看着正常，桌面上就是半空加一排细条。

`MdCarousel` 现在会求解视口放得下几个大项，并且从焦点项**向尾部**递减而不是对称递减。对称窗口会发出
`2 × largeCount − 1` 个大项，正好撑爆刚解出来的那个数。大项数为 1 时新规则与旧规则逐字等价，**窄屏
行为分毫未动**。

### 无边框窗口四角是方的

主题特意把窗口自身设成透明，好让合成器把圆角从客户区里切出来，圆角表面交给 `PART_WindowFrame` 画。
但这个 Border 把自己的 `Background` **绑回了那个刚被设成透明的窗口背景**，于是它什么都没画。平台一旦
拒绝透明，不透明的 `TransparencyBackgroundFallback` 就会把整个方形窗口刷满——这就是截图里的方角。

现在 frame 画 `TransparencyBackgroundFallback`：语义上正是「本窗口的不透明底色」，已主题化为 Material
surface，调用方仍可覆盖。

### 图像对比滑块与指针脱节

手柄骑在分隔线上。当分隔线位于最左/最上时，**手柄有一半被裁到控件之外**，指针够得着的每一个像素都落在
同一侧，于是按下时拾取的偏移是单向的，最大可达半个手柄宽。拖拽是有意设计成按位移量跟踪的（抓偏了不让
分隔线跳到光标下），所以这个偏移会被完整保留到整个拖拽结束。

现在保留的偏移被**裁剪到分隔线到较近一侧边缘的距离**：中段几乎等于半个控件，手感不变；两端恰好为 0，
自动退化为吸附。一条规则同时满足两种情形。

### 桌面端 picker 弹窗收不到滚轮和按键

桌面上弹窗是独立的 `PopupRoot`，在它内部抛出的事件止步于该 root，不会冒泡回控件；Android 把弹窗渲染进
`TopLevel` overlay，所以同样的手势在那边一直是好的，在 Windows 上却是死的。弹窗表面现在转发进同一套
处理器，`MdTimePicker` 也会按指针位置决定调整的是小时列还是分钟列。

### 其它

- 页面滚动时桌面弹窗会被关闭，而不是停在原地、锚点跑掉。
- Cascader 下拉改为与锚点左边缘对齐，不再居中。

## 新增

- **`Md3.Avalonia.RichEditor`** —— [AvaloniaRichEditor](https://github.com/centwon/AvaloniaRichEditor)
  的 Material Design 3 主题，opt-in。它是唯一引入该依赖的包，包总数到六个。
- **原生 AOT 已验证可用。** 六个包全部声明 `IsAotCompatible`，在原有 IL2xxx 裁剪分析器之外打开了
  IL3xxx 分析器。`.github/workflows/aot-probe.yml` 用 `PublishAot=true` 发布桌面 Gallery，并且
  **不把「步骤成功」当作答案**：它断言产物是原生 ELF 可执行文件、输出目录里没有托管 dll 残留（防止
  apphost 冒充），然后在 xvfb 下把那个二进制真正跑起来。原生 AOT 是在运行期因反射元数据被裁而失败的，
  不是在编译期，所以最后这一步才是结论的承重点。
  未覆盖：Windows / macOS 发布，以及 Android（运行时模型不同）。
- `MdBreadcrumb` 新增 `Powerline` 变体。
- [`docs/DIALOG_SERVICE.md`](DIALOG_SERVICE.md)：如何通过 `IMdDialogService` 在 ViewModel 中弹出
  对话框，含组合根接线。

## Gallery

- Usage 区不再显示占位代码。普查发现 32 个页面只有 XAML、7 个只有 C#，缺失的那一页会渲染成一个空框；
  现在缺失的标签直接隐藏。
- 480 DIP 窗口下会横向溢出的 34 个页面中，29 个已能自适应（约 50 处固定 `Width` 改为 `MaxWidth`）。
  剩余 5 个在测试中用**双向断言**钉住——既不会被悄悄遗忘，回流后也不会被悄悄留在清单里。
- 本地化表新增 366 条中文词条。

## 测试

渲染后的像素现在真的被读了。`UseHeadlessDrawing=false` 意味着这些测试跑的是真实 Skia，但此前三十多处
`CaptureRenderedFrame()` 调用**无一例外只断言了「帧不为 null」**。圆角类缺陷曾据此被判为「headless 不可
观测」——这个判断是错的，上面那个无边框窗口的缺陷正是这样找到的。

## 升级

无破坏性改动，直接改版本号即可：

```xml
<PackageReference Include="Md3.Avalonia" Version="0.4.1-preview.1" />
```

如果你显式依赖 carousel 的 item 宽度（例如写死了「第二项是中号」这类假设），注意宽窗口下现在会铺出更多
大项；窄窗口下的结果与 0.4.0-preview.1 完全一致。
