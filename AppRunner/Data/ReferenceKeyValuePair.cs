using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRunner.Data
{
    public record class ReferenceKeyValuePair<TKey, TValue>
    {
        public ReferenceKeyValuePair() { }

        public ReferenceKeyValuePair(TKey? key, TValue? value)
        {
            Key = key;
            Value = value;
        }

        public TKey? Key { get; set; }
        public TValue? Value { get; set; }
    }
}
