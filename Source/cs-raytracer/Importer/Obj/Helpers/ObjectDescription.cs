using System.Collections.Generic;

namespace TracerUIMockup.Importer.Obj.Helpers;

public class ObjectDescription
{
    public string objectName;
    public List<FaceDescription> FacePoints;

    public ObjectDescription()
    {
        FacePoints = new List<FaceDescription>();
    }
}