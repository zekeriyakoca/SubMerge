using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Submerge.Engine.Model
{
    public record Entry(TimeSpan Start, TimeSpan End, string Text1, string Text2) { };

}
