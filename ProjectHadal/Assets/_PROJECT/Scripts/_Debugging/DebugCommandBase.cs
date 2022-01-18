using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    private string _commandID;
    private string _commandDesc;
    private string _commandFormat;

    public string ID => _commandID;
    public string Desc => _commandDesc;
    public string Format => _commandFormat;

    public DebugCommandBase(string id, string desc, string format)
    {
        _commandID = id;
        _commandDesc = desc;
        _commandFormat = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action _command;

    public DebugCommand(string id, string desc, string format, Action command) : base(id, desc, format)
    {
        _command = command;
    }

    public void Invoke()
    {
        _command.Invoke();
    }
}

public class DebugCommand<T1> : DebugCommandBase
{
    private Action<T1> _command;
    public DebugCommand(string id, string desc, string format, Action<T1> command) : base(id, desc, format)
    {
        _command = command;
    }
    
    public void Invoke(T1 value)
    {
        _command.Invoke(value);
    }
}
