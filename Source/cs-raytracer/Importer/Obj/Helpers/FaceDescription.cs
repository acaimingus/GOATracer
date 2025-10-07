using System.Collections.Generic;

namespace TracerUIMockup.Importer.Obj.Helpers;

public class FaceDescription
{
    public List<int> Indices;

    public FaceDescription(List<int> indices)
    {
        this.Indices = indices;
    }
}