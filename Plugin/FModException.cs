using FMOD;
using System;

namespace Subnautica_Echelon
{
    internal class FModException : Exception
    {
        public RESULT Result { get; }


        public FModException(string message, RESULT result) : base(message)
        {
            this.Result = result;
        }
    }
}