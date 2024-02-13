namespace StackerBot.Types;

public class Union<T1, T2>(byte index, T1? t1, T2? t2) : Union<T1, T2, byte?>(index, t1, t2, null) {
  public static implicit operator Union<T1, T2>(T1 t1) => new(1, t1, default);
  public static implicit operator Union<T1, T2>(T2 t2) => new(2, default, t2);

  public T Switch<T>(Func<T1, T> function1, Func<T2, T> function2) {
    return HandleSwitch(
      obj => function1((T1) obj),
      obj => function2((T2) obj)
    );
  }
}

public class Union<T1, T2, T3>(byte index, T1? t1, T2? t2, T3? t3)
  : Union<T1, T2, T3, byte?>(index, t1, t2, t3, null) {
  public static implicit operator Union<T1, T2, T3>(T1 t1) => new(1, t1, default, default);
  public static implicit operator Union<T1, T2, T3>(T2 t2) => new(2, default, t2, default);
  public static implicit operator Union<T1, T2, T3>(T3 t3) => new(3, default, default, t3);

  public static implicit operator Union<T1, T2, T3>(Union<T1, T2> union) {
    return new Union<T1, T2, T3>(union._index, union._t1, union._t2, default);
  }

  public static implicit operator Union<T1, T3>(Union<T1, T2, T3> union) {
    return new Union<T1, T3>(union._index, union._t1, union._t3);
  }

  public T Switch<T>(Func<T1, T> function1, Func<T2, T> function2, Func<T3, T> function3) {
    return HandleSwitch(
      obj => function1((T1) obj),
      obj => function2((T2) obj),
      obj => function3((T3) obj)
    );
  }
}

public class Union<T1, T2, T3, T4>(byte index, T1? t1, T2? t2, T3? t3, T4? t4) {
  protected readonly T1? _t1 = t1;
  protected readonly T2? _t2 = t2;
  protected readonly T3? _t3 = t3;
  protected readonly T4? _t4 = t4;
  protected readonly byte _index = index;

  public static implicit operator Union<T1, T2, T3, T4>(T1 t1) => new(1, t1, default, default, default);
  public static implicit operator Union<T1, T2, T3, T4>(T2 t2) => new(2, default, t2, default, default);
  public static implicit operator Union<T1, T2, T3, T4>(T3 t3) => new(3, default, default, t3, default);
  public static implicit operator Union<T1, T2, T3, T4>(T4 t4) => new(4, default, default, default, t4);

  public static implicit operator Union<T1, T2, T3, T4>(Union<T1, T2, T3> union) {
    return new Union<T1, T2, T3, T4>(union._index, union._t1, union._t2, union._t3, default);
  }

  public static implicit operator Union<T1, T2, T4>(Union<T1, T2, T3, T4> union) {
    return new Union<T1, T2, T4>(union._index, union._t1, union._t2, union._t4);
  }

  public new Type GetType() {
    return _index switch {
      1 => _t1?.GetType() ?? throw new Exception("invalid union state"),
      2 => _t2?.GetType() ?? throw new Exception("invalid union state"),
      3 => _t3?.GetType() ?? throw new Exception("invalid union state"),
      4 => _t4?.GetType() ?? throw new Exception("invalid union state"),
      _ => throw new Exception("invalid union state")
    };
  }

  public bool IsType(Type type) {
    return _index switch {
      1 => _t1 != null && _t1.GetType() == type,
      2 => _t2 != null && _t2.GetType() == type,
      3 => _t3 != null && _t3.GetType() == type,
      4 => _t4 != null && _t4.GetType() == type,
      _ => throw new Exception("invalid union state")
    };
  }

  public bool IsNotType(Type type) {
    return _index switch {
      1 => _t1 != null && _t1.GetType() != type,
      2 => _t2 != null && _t2.GetType() != type,
      3 => _t3 != null && _t3.GetType() != type,
      4 => _t4 != null && _t4.GetType() != type,
      _ => throw new Exception("invalid union state")
    };
  }

  public T1 GetT1 => _t1 ?? throw new Exception("invalid union state");
  public T2 GetT2 => _t2 ?? throw new Exception("invalid union state");
  public T3 GetT3 => _t3 ?? throw new Exception("invalid union state");
  public T4 GetT4 => _t4 ?? throw new Exception("invalid union state");

  public T Switch<T>(Func<T1, T> function1, Func<T2, T> function2, Func<T3, T> function3, Func<T4, T> function4) {
    return HandleSwitch(
      obj => function1((T1) obj),
      obj => function2((T2) obj),
      obj => function3((T3) obj),
      obj => function4((T4) obj)
    );
  }

  protected T HandleSwitch<T>(params Func<object, T>[] functions) {
    if (_index > functions.Length) {
      throw new Exception("invalid union state");
    }

    var value = GetValue(_index);

    if (value == null) {
      throw new Exception("invalid union state");
    }

    return functions[_index - 1](value);
  }

  private object? GetValue(int index) {
    return index switch {
      1 => _t1,
      2 => _t2,
      3 => _t3,
      4 => _t4,
      _ => throw new Exception("invalid union state"),
    };
  }
}
