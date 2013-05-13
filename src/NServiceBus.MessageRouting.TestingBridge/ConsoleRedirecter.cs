using System;
using System.IO;

namespace NServiceBus.MessageRouting.TestingBridge
{
    public class ConsoleRedirecter : MarshalByRefObject
    {
        public void SetConsoleOut(TextWriter consoleOut)
        {
            Console.SetOut(consoleOut);
        }
    }
}