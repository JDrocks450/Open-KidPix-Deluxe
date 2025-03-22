using KidPix.API.Importer.Mohawk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer
{
    public class MHWKGeneralImporter : MHWKResourceImporterBase
    {
        public override KidPixResource? Import(Stream Stream, ResourceTableEntry ParentEntry) => new GenericKidPixResource(ParentEntry, Stream);
    }
}
