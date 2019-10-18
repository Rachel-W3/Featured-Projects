using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grov
{
    class PQNode<T>
    {
        #region properties
        // ************* Properties ************* //

        public int Priority { get; set; }
        public T Data { get; set; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public PQNode(int priority, T data)
        {
            Priority = priority;
            Data = data;
        }
        #endregion
    }
}
