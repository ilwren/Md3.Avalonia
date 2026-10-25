namespace Md3.Avalonia.Controls;

/// <summary>
/// UI-independent entry point used by view models to enqueue messages in an attached
/// <see cref="MdSnackbarHost"/>. The host presents queued messages one at a time.
/// 供 ViewModel 向已附加的 <see cref="MdSnackbarHost"/> 排队提交消息的 UI 无关入口。宿主逐条呈现队列中的消息。
/// </summary>
/// <remarks>
/// <para>The service queues message models; it does not create or attach visual controls. A request
/// remains pending until an attached host presents and closes it. For dependency injection, assign
/// the same <see cref="MdSnackbarService"/> instance to <see cref="MdSnackbarHost.Service"/> and
/// register it as <see cref="IMdSnackbarService"/>. A host can also be called directly; when its
/// shared service is unset it uses a private service.</para>
/// <para><see cref="ShowAsync(MdSnackbarMessage)"/> completes with
/// <see cref="MdSnackbarResult.ActionInvoked"/> when the message action is invoked and with
/// <see cref="MdSnackbarResult.Dismissed"/> when the message is dismissed or times out. Use
/// <see cref="MdSnackbarMessage.ActionContent"/>, <see cref="MdSnackbarMessage.ActionCommand"/>,
/// and <see cref="MdSnackbarMessage.ActionCommandParameter"/> to configure its action.
/// <see cref="MdSnackbarMessage.IsDismissible"/> controls the separate dismiss affordance. A
/// message with an action remains open regardless of <see cref="MdSnackbarMessage.Duration"/>.</para>
/// <para>服务负责排队消息模型，不会创建或附加视觉控件。请求会保持等待，直到已附加的宿主呈现并关闭该消息。使用依赖注入时，将同一个 <see cref="MdSnackbarService"/> 实例赋给 <see cref="MdSnackbarHost.Service"/>，并注册为 <see cref="IMdSnackbarService"/>。也可以直接调用宿主；未设置共享服务时，宿主会使用自己的私有服务。</para>
/// <para><see cref="ShowAsync(MdSnackbarMessage)"/> 在消息操作被触发时以 <see cref="MdSnackbarResult.ActionInvoked"/> 完成，在消息被关闭或超时时以 <see cref="MdSnackbarResult.Dismissed"/> 完成。可通过 <see cref="MdSnackbarMessage.ActionContent"/>、<see cref="MdSnackbarMessage.ActionCommand"/> 和 <see cref="MdSnackbarMessage.ActionCommandParameter"/> 配置操作。<see cref="MdSnackbarMessage.IsDismissible"/> 控制是否显示独立的关闭入口；带操作按钮的消息不会因 <see cref="MdSnackbarMessage.Duration"/> 到期而关闭。</para>
/// </remarks>
/// <example>
/// <para>Share one service between the host and view models, then await the action result.</para>
/// <para>宿主与 ViewModel 共用同一个服务，并等待操作结果。</para>
/// <code language="csharp"><![CDATA[
/// var snackbars = new MdSnackbarService();
/// services.AddSingleton<IMdSnackbarService>(snackbars);
/// snackbarHost.Service = snackbars;
///
/// var result = await snackbars.ShowAsync(new MdSnackbarMessage("Draft archived / 草稿已归档")
/// {
///     ActionContent = "Undo / 撤销",
///     ActionCommand = undoCommand,
///     IsDismissible = true
/// });
///
/// if (result == MdSnackbarResult.ActionInvoked)
/// {
///     // Restore the draft. / 恢复草稿。
/// }
/// ]]></code>
/// </example>
public interface IMdSnackbarService
{
    /// <summary>Queues a message and completes when its action is invoked or it is dismissed. 将消息加入队列，并在操作触发或消息关闭时完成。</summary>
    Task<MdSnackbarResult> ShowAsync(MdSnackbarMessage message);

    /// <summary>Queues a message without waiting for its result. 将消息加入队列，不等待结果。</summary>
    void Show(MdSnackbarMessage message);

    /// <summary>Dismisses the currently displayed message, if any. 关闭当前显示的消息（如有）。</summary>
    void Dismiss();
}
