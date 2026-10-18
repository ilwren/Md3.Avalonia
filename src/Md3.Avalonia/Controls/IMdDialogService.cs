namespace Md3.Avalonia.Controls;

/// <summary>
/// UI-independent entry point used by view models to show a dialog in an attached
/// <see cref="MdDialogHost"/>. Inject this instead of handing a view model a reference to the
/// control: the view model supplies a dialog model, and the host resolves its view from a matching
/// data template.
/// 供 ViewModel 在已附加的 <see cref="MdDialogHost"/> 中显示对话框的 UI 无关入口。注入此服务，避免 ViewModel 持有控件引用：ViewModel 传入对话框模型，宿主通过匹配的数据模板解析视图。
/// </summary>
/// <remarks>
/// <para>Requests are serialized: <see cref="ShowAsync(object, CancellationToken)"/> waits behind
/// the active dialog, and requests made before a host is attached remain queued. Cancellation can
/// cancel a queued or active request and completes it with <see langword="null"/>. Closing with
/// <see cref="Close(object)"/> returns its result; scrim dismissal, Escape, and the Android back
/// gesture complete with <see langword="null"/>.</para>
/// <para>For model-based views, register a matching template in <see cref="MdDialogHost.DataTemplates"/>
/// or another visible <c>DataTemplates</c> collection. For dependency injection, assign the same
/// <see cref="MdDialogService"/> instance to <see cref="MdDialogHost.Service"/> and register it as
/// <see cref="IMdDialogService"/>. The host can also use its private service when no shared service
/// is assigned.</para>
/// <para><see cref="MdDialogServiceExtensions.ShowAsync{TResult}(IMdDialogService, object, CancellationToken)"/>
/// is a typed extension method, not an interface member; it returns the result as
/// <c>TResult</c> or <see langword="default"/> when dismissed without a matching result.</para>
/// <para>请求按顺序处理：已有对话框时，<see cref="ShowAsync(object, CancellationToken)"/> 会等待；宿主尚未附加时，请求会保留在队列中。取消令牌可以取消排队中或正在显示的请求，结果为 <see langword="null"/>。调用 <see cref="Close(object)"/> 会返回指定结果；点击遮罩、按 Escape 或使用 Android 返回手势关闭时，结果为 <see langword="null"/>。</para>
/// <para>对于模型视图，请在 <see cref="MdDialogHost.DataTemplates"/> 或其他可见的 <c>DataTemplates</c> 集合中注册匹配模板。使用依赖注入时，将同一个 <see cref="MdDialogService"/> 实例赋给 <see cref="MdDialogHost.Service"/>，并注册为 <see cref="IMdDialogService"/>。若未指定共享服务，宿主也可以使用自己的私有服务。</para>
/// <para><see cref="MdDialogServiceExtensions.ShowAsync{TResult}(IMdDialogService, object, CancellationToken)"/> 是强类型扩展方法而非接口成员；它会将结果转换为 <c>TResult</c>，若关闭时没有匹配类型的结果则返回 <see langword="default"/>。</para>
/// </remarks>
/// <example>
/// <para>Register one shared service for the host and view models. The dialog view is selected by
/// model type from the host's visible data templates; call <see cref="Close(object)"/> to return
/// a value to the caller.</para>
/// <para>为宿主与 ViewModel 注册同一个服务。宿主根据模型类型从可见的数据模板中选择对话框视图；调用 <see cref="Close(object)"/> 将结果返回给调用方。</para>
/// <code language="csharp"><![CDATA[
/// var dialogs = new MdDialogService();
/// services.AddSingleton<IMdDialogService>(dialogs);
/// dialogHost.Service = dialogs;
///
/// var confirmed = await dialogs.ShowAsync<bool>(confirmationModel, cancellationToken);
/// if (confirmed)
/// {
///     // Perform the confirmed action. / 执行已确认的操作。
/// }
/// ]]></code>
/// </example>
public interface IMdDialogService
{
    /// <summary>True while a dialog requested through this service is displayed. 通过此服务请求的对话框正在显示时为 true。</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Shows <paramref name="dialog"/> and completes with the value passed to <see cref="Close(object)"/>.
    /// Completes with null when dismissed by the scrim, Escape, the Android back gesture, or
    /// <paramref name="cancellationToken"/>. A request made while another dialog is open — or
    /// before a host is attached — waits its turn rather than replacing it.
    /// 显示 <paramref name="dialog"/>，并以 <see cref="Close(object)"/> 传入的值完成。点击遮罩、Escape、Android 返回或 <paramref name="cancellationToken"/> 取消时结果为 null。已有对话框显示或宿主尚未附加时，请求会等待，不会替换当前对话框。
    /// </summary>
    Task<object?> ShowAsync(object dialog, CancellationToken cancellationToken = default);

    /// <summary>Shows a dialog without waiting for its result. 显示对话框但不等待结果。</summary>
    void Show(object dialog);

    /// <summary>
    /// Swaps the displayed dialog for <paramref name="dialog"/>, completing the one it replaces
    /// with null. Use this for a step inside one flow — an about box opening its license list, a
    /// wizard advancing a page — where <see cref="ShowAsync(object, CancellationToken)"/> would
    /// queue the new dialog behind the old one and reveal it only after the old one closes.
    /// 用 <paramref name="dialog"/> 替换当前对话框，并使被替换请求以 null 完成。适用于同一流程中的步骤切换（例如从关于窗口打开许可列表或向导切换页面）；若使用 <see cref="ShowAsync(object, CancellationToken)"/>，新对话框会排在当前项之后。
    /// </summary>
    /// <remarks>
    /// The replacement jumps ahead of anything already queued, so it is displayed next even when
    /// other requests are waiting. With nothing displayed this behaves like <see cref="ShowAsync(object, CancellationToken)"/>.
    /// 替换请求优先于已排队请求显示；没有当前对话框时，其行为与 <see cref="ShowAsync(object, CancellationToken)"/> 相同。
    /// </remarks>
    Task<object?> ReplaceAsync(object dialog, CancellationToken cancellationToken = default);

    /// <summary>Replaces the displayed dialog without waiting for the new one's result. 替换当前对话框，不等待新对话框的结果。</summary>
    void Replace(object dialog);

    /// <summary>Closes the displayed dialog, completing its pending <see cref="ShowAsync(object, CancellationToken)"/> with <paramref name="result"/>. 关闭当前对话框，并以 <paramref name="result"/> 完成对应的 <see cref="ShowAsync(object, CancellationToken)"/> 请求。</summary>
    void Close(object? result = null);
}

/// <summary>Typed conveniences over <see cref="IMdDialogService"/>. 为 <see cref="IMdDialogService"/> 提供强类型便捷扩展。</summary>
public static class MdDialogServiceExtensions
{
    /// <summary>
    /// Shows a dialog and returns its result as <typeparamref name="TResult"/>, or
    /// <c>default</c> when dismissed without a value of that type.
    /// 显示对话框并将结果转换为 <typeparamref name="TResult"/>；若关闭时没有该类型的结果，则返回 <c>default</c>。
    /// </summary>
    public static async Task<TResult?> ShowAsync<TResult>(
        this IMdDialogService service, object dialog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.ShowAsync(dialog, cancellationToken).ConfigureAwait(true) is TResult result
            ? result
            : default;
    }
}
