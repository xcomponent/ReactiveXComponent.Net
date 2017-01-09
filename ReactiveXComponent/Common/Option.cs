using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReactiveXComponent.Common
{
    public class Option<T>
    {
        public enum Tag
        {
            None,
            Some
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Tag Case { get; set; }

        public T[] Fields { get; set; }

        public Option()
        {
            
        }

        public Option(T value)
        {
            Case = Tag.Some;
            Fields = new T[] {value};
        }

        public Option(T[] values)
        {
            Case = Tag.Some;
            Fields = values;
        }
    }
}
