using Rvl.Display.Core.Model;

namespace Rvl.Display.Core.Interfaces;

public interface IRvlHostCliInput
{
    ConsoleModifiers Modifiers { get; }
    ConsoleKey Key { get; }
    RvlCommandData Command { get; }
}

