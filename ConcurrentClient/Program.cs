using System;
using System.Runtime.CompilerServices;

await HelloAsync("Hello, World!");


async Task HelloAsync(string name, int time = 10, CancellationToken token = default)
{
    await Task.Delay(10, token);
    Console.WriteLine($"Thread{Environment.CurrentManagedThreadId} HelloAsync {name},{DateTime.Now:HH:mm:ss.fff}");
}

class HelloAsyncStateMachine(string name,int time, CancellationToken token) : IAsyncStateMachine
{
    public int _state;
    public AsyncTaskMethodBuilder _builder;
    public string _name = name;
    public int _time = time;
    public CancellationToken _token = token;
    private TaskAwaiter _awaiter;

    public void MoveNext()
    {
        int num = _state;
        try
        {
            TaskAwaiter awaiter;
            if (num != 0)
            {
                awaiter = Task.Delay(10, _token).GetAwaiter();
                if (!awaiter.IsCompleted)
                {
                    num = _state = 0;
                    _awaiter = awaiter;
                    //_awaiter = default;
                    var stateMachine = this;
                    _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
                }
                return;
            }
            awaiter = _awaiter;
            _awaiter = default;
            num = _state = -1;
            awaiter.GetResult();
            Console.WriteLine($"Thread{Environment.CurrentManagedThreadId} HelloAsync {_name},{DateTime.Now:HH:mm:ss.fff}");
        }
        catch (Exception ex)
        {
            _state = -2;
            _builder.SetException(ex);
            return;
        }
        _state = -2;
        _builder.SetResult();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        throw new NotImplementedException();
    }
}