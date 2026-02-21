using Rvl.Host.App.Core.Model;

namespace Rvl.Host.App.Core.Interfaces;

public interface IRvlHostCliInput
{
    ConsoleModifiers Modifiers { get; }
    ConsoleKey Key { get; }
    RvlCommandData Command { get; }
}

