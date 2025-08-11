using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SHC_Rebalancer;
public class GifImage : Image
{
    private GifBitmapDecoder? _decoder;
    private readonly List<TimeSpan> _delays = [];
    private int _frameIndex;
    private DispatcherTimer? _timer;
    private int _loopCount;     // 0 = infinite loops
    private int _loopsPlayed;

    public GifImage()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    static GifImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GifImage), new FrameworkPropertyMetadata(typeof(GifImage)));
    }

    #region Dependency Properties

    public Uri Source
    {
        get => (Uri)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(GifImage),
            new PropertyMetadata(null, OnSourceChanged)
        );

    public bool AutoStart
    {
        get => (bool)GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }
    public static readonly DependencyProperty AutoStartProperty
        = DependencyProperty.Register(
            nameof(AutoStart),
            typeof(bool),
            typeof(GifImage),
            new PropertyMetadata(true)
        );

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }
    public static readonly DependencyProperty IsPlayingProperty
        = DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(GifImage),
            new PropertyMetadata(false, OnIsPlayingChanged)
        );

    public double SpeedRatio
    {
        get => (double)GetValue(SpeedRatioProperty);
        set => SetValue(SpeedRatioProperty, value);
    }
    public static readonly DependencyProperty SpeedRatioProperty
        = DependencyProperty.Register(
            nameof(SpeedRatio),
            typeof(double),
            typeof(GifImage),
            new PropertyMetadata(1.0, OnSpeedRatioChanged)
        );
    #endregion

    #region DP Handlers
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gi = (GifImage)d;
        gi.StopInternal();
        gi.LoadGif();
        if (gi.AutoStart) gi.Start();
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gi = (GifImage)d;
        if ((bool)e.NewValue) gi.StartInternal();
        else gi.StopInternal();
    }

    private static void OnSpeedRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gi = (GifImage)d;
        if (gi._timer != null) gi.ScheduleNextTick();
    }
    #endregion

    #region Public API
    public void Start() => IsPlaying = true;
    public void Stop() => IsPlaying = false;
    public void Reset()
    {
        Stop();
        _frameIndex = 0;
        _loopsPlayed = 0;
        if (_decoder != null && _decoder.Frames.Count > 0)
            base.Source = _decoder.Frames[0];
    }
    #endregion

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AutoStart && _decoder != null) StartInternal();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopInternal();
    }

    private void LoadGif()
    {
        _decoder = null;
        _delays.Clear();
        _frameIndex = 0;
        _loopCount = 0;
        _loopsPlayed = 0;
        base.Source = null;

        if (Source == null) return;

        Stream stream;
        if (Source.IsAbsoluteUri && Source.Scheme != "pack")
        {
            stream = File.OpenRead(Source.LocalPath);
        }
        else
        {
            var sri = Application.GetResourceStream(Source);
            if (sri == null) return;
            stream = sri.Stream;
        }

        using (stream)
        {
            _decoder = new GifBitmapDecoder(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            if (_decoder.Frames.Count == 0) return;

            base.Source = _decoder.Frames[0];

            foreach (var f in _decoder.Frames)
            {
                ushort delayCs = 0;
                if (f.Metadata is BitmapMetadata meta && meta.ContainsQuery("/grctlext/Delay"))
                    delayCs = (ushort)(meta.GetQuery("/grctlext/Delay") ?? (ushort)0);

                var ms = Math.Max(10, delayCs * 10);
                _delays.Add(TimeSpan.FromMilliseconds(ms));
            }

            try
            {
                if (_decoder.Metadata is BitmapMetadata containerMeta && containerMeta.ContainsQuery("/appext/Application"))
                {
                    if (containerMeta.ContainsQuery("/appext/Data"))
                    {
                        var bytes = (byte[])containerMeta.GetQuery("/appext/Data");
                        if (bytes != null && bytes.Length >= 3 && bytes[0] == 0x01)
                            _loopCount = bytes[1] | (bytes[2] << 8); // 0 = infinite loops
                    }
                }
            }
            catch
            {
                _loopCount = 0;
            }
        }
    }

    private void StartInternal()
    {
        if (_decoder == null || _decoder.Frames.Count == 0) return;

        _timer ??= new DispatcherTimer(DispatcherPriority.Render);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;

        ScheduleNextTick();
        _timer.Start();
        IsPlaying = true;
    }

    private void StopInternal()
    {
        _timer?.Stop();
        IsPlaying = false;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_decoder == null) return;

        _frameIndex++;
        if (_frameIndex >= _decoder.Frames.Count)
        {
            _frameIndex = 0;
            _loopsPlayed++;

            if (_loopCount > 0 && _loopsPlayed >= _loopCount)
            {
                StopInternal();
                return;
            }
        }

        base.Source = _decoder.Frames[_frameIndex];
        ScheduleNextTick();
    }

    private void ScheduleNextTick()
    {
        if (_timer == null || _decoder == null) return;
        var delay = _delays.Count > 0 ? _delays[_frameIndex % _delays.Count] : TimeSpan.FromMilliseconds(100);
        if (SpeedRatio <= 0) SpeedRatio = 0.0001; // avoid division by zero
        _timer.Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds / SpeedRatio);
    }
}
