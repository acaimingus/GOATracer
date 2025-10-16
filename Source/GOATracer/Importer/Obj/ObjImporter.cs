using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace GOATracer.Importer.Obj;

public static class ObjImporter
{
    /// <summary>
    /// Parses a Wavefront .obj file and converts it into the internal 3D scene representation
    /// </summary>
    /// <param name="filePath">Path to the .obj file to import</param>
    /// <returns>ImportedSceneDescription containing all vertices, faces, and objects from the file</returns>
    public static ImportedSceneDescription ImportModel(string filePath)
    {
        // Parse the .obj file and separate it into individual 3D objects
        var wavefrontObjects = SplitFileByObjects(filePath);

        // Create a description for the scene
        var sceneDescription = new ImportedSceneDescription(Path.GetFileName(filePath));
        
        // Process each 3D object found in the file
        foreach (var wavefrontObject in wavefrontObjects)
        {
            // Create a container for this object's geometry data
            var objectDescription = new ObjectDescription();

            foreach (var line in wavefrontObject)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Get the position of the first space to determine where the first word ends
                var firstSpaceIndex = line.IndexOf(' ');
                
                // Extract the .obj command type (o, v, f, etc.)
                var firstWord = line.Substring(0, firstSpaceIndex);

                // Parse different types of .obj file data based on the command
                switch (firstWord)
                {
                    // Object command found
                    case "o":
                        // Object name definition - extract everything after "o "
                        var nameOnly = line[(firstSpaceIndex + 1)..];
                        objectDescription.ObjectName = nameOnly;
                        break;
                    // Vertex command found
                    case "v":
                        // Vertex definition - parse 3D coordinates (x, y, z)
                        var coordinatesOnly = line[(firstSpaceIndex + 1)..];
                        
                        // Split "x y z" into individual coordinate strings
                        var splitStringCoordinates = coordinatesOnly.Split(' ');
                        
                        // Convert coordinate strings to numbers
                        var coordinates = new float[3];
                        
                        // Use InvariantCulture to handle decimal points correctly regardless of system locale
                        coordinates[0] = float.Parse(splitStringCoordinates[0], CultureInfo.InvariantCulture);
                        coordinates[1] = float.Parse(splitStringCoordinates[1], CultureInfo.InvariantCulture);
                        coordinates[2] = float.Parse(splitStringCoordinates[2], CultureInfo.InvariantCulture);
                        
                        // Store this vertex in our scene's master vertex list
                        sceneDescription.VertexPoints?.Add(new VertexPoint(new Vector3(coordinates[0], coordinates[1], coordinates[2])));
                        break;
                    // Face command found
                    case "f":
                        // Face definition - defines which vertices connect to form a polygon
                        var indicesOnly = line[(firstSpaceIndex + 1)..];
                        
                        // Split into individual vertex references
                        var splitStringIndices = indicesOnly.Split(' ');
                        
                        var indices = new List<int>();

                        // Extract just the vertex indices (ignore texture/normal data after '/')
                        foreach (var index in splitStringIndices)
                        {
                            var vertexIndexOnly = index.Split('/');
                            indices.Add(int.Parse(vertexIndexOnly[0]));
                        }
                        
                        // Add this face to the current object
                        objectDescription.FacePoints.Add(new ObjectFace(indices));
                        break;
                }
            }
            
            // Add this completed object to our scene
            sceneDescription.ObjectDescriptions?.Add(objectDescription);
        }
        
        return sceneDescription;
    }

    /// <summary>
    /// Splits a .obj file into separate sections, one for each 3D object defined in the file
    /// </summary>
    /// <param name="filePath">Path to the .obj file to process</param>
    /// <returns>List of line groups, where each group contains all data for one 3D object</returns>
    private static List<List<string>> SplitFileByObjects(string filePath)
    {
                
        // Open the file for reading
        using var streamReader = new StreamReader(filePath);
        
        // Track how many objects we've found
        var objectCount = 0;
        // Store each object as a separate list of lines
        var objects = new List<List<string>>();
        // Collect lines for the current object being processed
        var currentFileSection = new List<string>();
        
        // Process the file line by line
        string? line;
        while ((line = streamReader.ReadLine()) != null)
        {
            // Skip empty lines
            if (string.IsNullOrEmpty(line))
            {
                continue;    
            }
            
            // Check what type of .obj command this line contains
            var firstWord = line.Substring(0, line.IndexOf(' '));
            
            // "o" command starts a new 3D object definition
            if (firstWord == "o")
            {
                // If this isn't the first object, save the previous object's data
                if (objectCount != 0)
                {
                    objects.Add(currentFileSection);
                    
                }
                // Up the object count
                ++objectCount;
                
                // Start collecting lines for this new object
                currentFileSection = [];
            }
            
            // Add this line to the current object's data
            currentFileSection.Add(line);
        }
        // Don't forget to add the final object when we reach the end of the file
        objects.Add(currentFileSection);

        return objects;
    }
}