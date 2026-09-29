using Md3.Avalonia.Icons;

namespace Md3.Avalonia.Controls;

/// <summary>Strongly named code points from the official Material Symbols Rounded catalog.</summary>
public static class MdSymbols
{
    private static string? Glyph(string value) => MdExternalMaterialSymbols.EnsureConfigured()
        ? MdExternalMaterialSymbols.ResolveGlyph(value)
        : null;

    public static string? Add => Glyph("\ue145");
    public static string? ArrowBack => Glyph("\ue5c4");
    public static string? ArrowDropDown => Glyph("\ue5c5");
    public static string? ArrowDropUp => Glyph("\ue5c7");
    public static string? Check => Glyph("\ue668");
    public static string? Close => Glyph("\ue5cd");
    public static string? Delete => Glyph("\ue92e");
    public static string? Description => Glyph("\ue873");
    public static string? Download => Glyph("\uf090");
    public static string? Edit => Glyph("\uf097");
    public static string? ExpandLess => Glyph("\ue5ce");
    public static string? ExpandMore => Glyph("\ue5cf");
    public static string? Favorite => Glyph("\ue87e");
    public static string? Home => Glyph("\ue9b2");
    public static string? Info => Glyph("\ue88e");
    public static string? Link => Glyph("\ue250");
    public static string? Mail => Glyph("\ue159");
    public static string? Menu => Glyph("\ue5d2");
    public static string? MoreVert => Glyph("\ue5d4");
    public static string? Notifications => Glyph("\ue7f5");
    public static string? Person => Glyph("\uf0d3");
    public static string? Photo => Glyph("\ue693");
    public static string? Refresh => Glyph("\ue5d5");
    public static string? Remove => Glyph("\ue15b");
    public static string? Search => Glyph("\uef7a");
    public static string? Settings => Glyph("\ue8b8");
    public static string? Share => Glyph("\ue80d");
    public static string? Star => Glyph("\uf09a");
    public static string? Upload => Glyph("\uf09b");
    public static string? Warning => Glyph("\uf083");
    public static string? AccountCircle => Glyph("\uf20b");
    public static string? AttachFile => Glyph("\ue226");
    public static string? Bolt => Glyph("\uea0b");
    public static string? Bookmark => Glyph("\ue8e7");
    public static string? CalendarMonth => Glyph("\uebcc");
    public static string? Cancel => Glyph("\ue888");
    public static string? ChevronLeft => Glyph("\ue5cb");
    public static string? ChevronRight => Glyph("\ue5cc");
    public static string? Code => Glyph("\ue86f");
    public static string? ContentCopy => Glyph("\ue14d");
    public static string? DarkMode => Glyph("\ue51c");
    public static string? Done => Glyph("\ue876");
    public static string? Today => Glyph("\ue8df");
    public static string? FilterAlt => Glyph("\uef4f");
    public static string? Help => Glyph("\ue8fd");
    public static string? Language => Glyph("\uea07");
    public static string? LightMode => Glyph("\ue518");
    public static string? Lock => Glyph("\ue899");
    public static string? MotionPhotosOn => Glyph("\ue9c1");
    public static string? Palette => Glyph("\ue40a");
    public static string? Pause => Glyph("\ue034");
    public static string? PlayArrow => Glyph("\ue037");
    public static string? SkipPrevious => Glyph("\ue045");
    public static string? SkipNext => Glyph("\ue044");
    public static string? Replay10 => Glyph("\ue059");
    public static string? Forward10 => Glyph("\ue056");
    public static string? VolumeUp => Glyph("\ue050");
    public static string? VolumeOff => Glyph("\ue04f");
    public static string? Fullscreen => Glyph("\ue5d0");
    public static string? FullscreenExit => Glyph("\ue5d1");
    public static string? RotateRight => Glyph("\ue419");
    public static string? ZoomIn => Glyph("\ue8ff");
    public static string? ZoomOut => Glyph("\ue900");
    public static string? Slideshow => Glyph("\ue41b");
    public static string? Image => Glyph("\ue3f4");
    public static string? MusicNote => Glyph("\ue405");
    public static string? Schedule => Glyph("\uefd6");
    public static string? ShoppingCart => Glyph("\ue8cc");
    public static string? Sort => Glyph("\ue164");
    public static string? TouchApp => Glyph("\ue913");
    public static string? Tune => Glyph("\ue429");
    public static string? Visibility => Glyph("\ue8f4");
    public static string? VisibilityOff => Glyph("\ue8f5");
    public static string? Work => Glyph("\ue943");
}
