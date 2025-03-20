using KidPix.API.Importer.Mohawk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer
{
    /// <summary>
    /// Base class for all importer resources
    /// </summary>
    public abstract class KidPixResource 
    {
        public ResourceTableEntry ParentEntry
        {
            get;
        }

        internal KidPixResource(ResourceTableEntry ParentEntry)
        {
            this.ParentEntry = ParentEntry;
        }
    }
}
