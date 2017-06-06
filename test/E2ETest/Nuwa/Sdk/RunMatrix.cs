using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuwa.Sdk
{
    public class RunMatrix
    {
        private Dictionary<string, RunMatrixEntry> _entries;

        public RunMatrix()
        {
            _entries = new Dictionary<string, RunMatrix>();
        }
    }
}
