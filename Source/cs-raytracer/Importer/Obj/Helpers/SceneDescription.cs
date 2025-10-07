using System.Collections.Generic;

namespace TracerUIMockup.Importer.Obj.Helpers;

public class SceneDescription
{
    public string? FileName;   
    public List<ObjectDescription>? ObjectDescriptions;
    public List<VertexPoint>? VertexPoints;

    public SceneDescription(string fileName)
    {
        FileName = fileName;
        VertexPoints = new List<VertexPoint>();
        ObjectDescriptions = new List<ObjectDescription>();
    }
    
    
}