using System;
using TorgiGovMongoServer.BuilderApp;
using TorgiGovMongoServer.Parsers;

namespace TorgiGovMongoServer.Executor
{
public class Executor
    {
        public Executor(Arguments arg)
        {
            switch (arg)
            {
                case Arguments.Curr:
                    _parser = new ParserTorgiGov();
                    break;
                case Arguments.Last:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
            }
        }

        private readonly IParser _parser;

        public void ExecuteParser()
        {
            try
            {
                _parser.Parsing();
            }
            catch (Exception e)
            {
                Logger.Log.Logger("Exception in parsing()", e);
            }
        }
    }
}