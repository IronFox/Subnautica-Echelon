using FMOD;
using System;

namespace Subnautica_Echelon.Adapters
{
    internal class FModException : Exception
    {
        public RESULT Result { get; }


        public FModException(string message, RESULT result) : base(message)
        {
            Result = result;
        }
    }
}