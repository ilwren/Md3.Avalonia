namespace Md3.Avalonia.Controls;

/// <summary>Imperative carousel controller for view code, tests, and MVVM adapter services.</summary>
public sealed class MdCarouselController
{
    private WeakReference<MdCarousel>? _carousel;
    public MdCarousel? Carousel => _carousel?.TryGetTarget(out var carousel) == true ? carousel : null;
    public int SelectedIndex { get => Carousel?.SelectedIndex ?? -1; set { if (Carousel is { } carousel) carousel.ScrollTo(value); } }
    public bool Next() => Carousel?.MoveNext() == true;
    public bool Previous() => Carousel?.MovePrevious() == true;
    public void StartAutoPlay() => Carousel?.StartAutoPlay();
    public void StopAutoPlay() => Carousel?.StopAutoPlay();
    internal void Attach(MdCarousel carousel) => _carousel = new WeakReference<MdCarousel>(carousel);
    internal void Detach(MdCarousel carousel) { if (ReferenceEquals(Carousel, carousel)) _carousel = null; }
}
