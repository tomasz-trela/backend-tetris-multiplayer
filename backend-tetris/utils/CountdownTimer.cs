namespace backend_tetris.utils;

using System;
using System.Timers;

public class CountdownTimer : IDisposable
{
    private readonly Timer _timer;
    public int TimeLeft { get; private set; }
    public event Action<int>? Tick;
    public event Action? CountdownFinished;

    public CountdownTimer(int startSeconds)
    {
        TimeLeft = startSeconds;
        _timer = new Timer(1000);
        _timer.Elapsed += OnElapsed;
        _timer.AutoReset = true;
    }

    public void Start()
    {
        if (!_timer.Enabled)
            _timer.Start();
    }

    private void OnElapsed(object? sender, ElapsedEventArgs e)
    {
        TimeLeft--;
        
        Console.WriteLine($"TimeLeft: {TimeLeft}");

        Tick?.Invoke(TimeLeft);

        if (TimeLeft > 0) return;
        Dispose();
        CountdownFinished?.Invoke();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
