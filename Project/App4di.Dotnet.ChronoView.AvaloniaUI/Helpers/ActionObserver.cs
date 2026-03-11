/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;

/// <summary>
/// Minimal IObserver adapter so we don't depend on System.Reactive Subscribe(Action&lt;T&gt;) extension overloads.
/// </summary>
public sealed class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;
    private readonly Action<Exception>? _onError;
    private readonly Action? _onCompleted;

    public ActionObserver(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null)
    {
        _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
        _onError = onError;
        _onCompleted = onCompleted;
    }

    public void OnNext(T value) => _onNext(value);

    public void OnError(Exception error) => _onError?.Invoke(error);

    public void OnCompleted() => _onCompleted?.Invoke();
}
