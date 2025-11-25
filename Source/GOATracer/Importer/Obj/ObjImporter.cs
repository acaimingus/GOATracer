using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Class for importing 3d geometry contents from an .obj file.
/// Source: https://paulbourke.net/dataformats/obj/obj_spec.pdf
/// Source: https://paulbourke.net/dataformats/mtl/
/// </summary>
public static class ObjImporter
{
    /// <summary>
    /// Parses a Wavefront .obj file and converts it into the internal 3D scene representation
    /// </summary>
    /// <param name="filePath">Path to the .obj file to import</param>
    /// <returns>ImportedSceneDescription containing all vertices, faces, and objects from the file</returns>
    public static ImportedSceneDescription ImportModel(string filePath)
    {
        var unnamedCount = 0;
        
        // Parse the .obj file and separate it into individual 3D objects
        var fileSegments = SplitFileByObjects(filePath);

        // Create a description for the scene
        var sceneDescription = new ImportedSceneDescription(Path.GetFileName(filePath));

        // Initialize a variable for the currently selected material in the file
        // May be null right now, but will be set when the usemtl command appears
        string? currentlyUsedMaterial = null;
        
        // Process each 3D object found in the file
        foreach (var fileSegment in fileSegments)
        {
            // Create a new object description for each object section
            var objectDescription = new ObjectDescription();

            // Process every line of each object
            foreach (var line in fileSegment)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Get the position of the first space to determine where the first word ends
                var firstSpaceIndex = line.IndexOf(' ');

                // Extract the .obj command type (o, v, f, etc.) by splitting off the first word from the line
                var firstWord = GetFirstWord(line);

                // Parse different types of .obj file data based on the command
                switch (firstWord)
                {
                    // Material command found
                    case "mtllib":
                        var fileNameOnly = line[(firstSpaceIndex + 1)..];
                        var mtlFullPath = Path.Combine(Path.GetDirectoryName(filePath)!, fileNameOnly);
                        try
                        {
                            ImportMaterials(mtlFullPath, sceneDescription);
                        }
                        catch (FileNotFoundException)
                        {
                            // The .mtl file is missing, there will be no materials
                        }
                        break;
                    
                    // Material use directive command found
                    case "usemtl":
                        // Set the name of the material currently to use as the currently used material
                        currentlyUsedMaterial = line[(firstSpaceIndex + 1)..];
                        break;
                    
                    // Object command found
                    case "o":
                    case "g":
                        // Extract the name for the object and set it in the object description
                        var nameOnly = line[(firstSpaceIndex + 1)..];
                        objectDescription.ObjectName = nameOnly;
                        break;

                    // Vertex command found
                    case "v":
                        // Store this vertex in the scene's master vertex list
                        sceneDescription.VertexPoints?.Add(ParseVector3(line, firstSpaceIndex));
                        break;

                    // Normal command found
                    case "vn":
                        // Extract the coordinates for the normal and add them to the master normal list
                        sceneDescription.NormalPoints?.Add(ParseVector3(line, firstSpaceIndex));
                        break;

                    // Texture command found
                    case "vt":
                        // Extract the coordinates for the texture point and add them to the master texture point list
                        sceneDescription.TexturePoints?.Add(ParseVector3(line, firstSpaceIndex));
                        break;
                    
                    // Face command found
                    case "f":
                        // Face definition - defines which vertices connect to form a polygon
                        var indicesOnly = line[(firstSpaceIndex + 1)..];

                        // Split into individual vertex references
                        var splitStringIndices = indicesOnly.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        
                        var indices = new List<FaceVertex>();

                        // Extract the indices from the X/Y/Z format
                        foreach (var index in splitStringIndices)
                        {
                            // Split the given indices by their separator
                            var vertexIndexOnly = index.Split('/');
                            
                            // Use TryParse for robust parsing; If parsing fails, the value will be null
                            // Check the length of the array for the second and third element to avoid OutOfRange exceptions
                            // These are currently being saved as 1-based; We will need to check if we make it 0-based here or when actually using the data
                            var vertexIndex = int.Parse(vertexIndexOnly[0]);
                            int? textureIndex = vertexIndexOnly.Length > 1 && int.TryParse(vertexIndexOnly[1], out var v1) ? v1 : null;
                            int? normalIndex = vertexIndexOnly.Length > 2 && int.TryParse(vertexIndexOnly[2], out var v2) ? v2 : null;
                            
                            indices.Add(new FaceVertex(vertexIndex, textureIndex, normalIndex));
                        }

                        // Add this face to the current object
                        objectDescription.FacePoints.Add(new ObjectFace(indices, currentlyUsedMaterial!));
                        break;
                }
            }
            
            if (string.IsNullOrWhiteSpace(objectDescription.ObjectName) && objectDescription.FacePoints.Count > 0)
            {
                // If an object has no name but vertices then give it a placeholder name
                objectDescription.ObjectName = $"Unnamed_Object_{unnamedCount++}";
            }
            
            // Check if the object actually is an object (it has a name)
            if (objectDescription.ObjectName != null)
            {
                // Add this completed object to our scene
                sceneDescription.ObjectDescriptions?.Add(objectDescription);
            }
        }

        return sceneDescription;
    }

    /// <summary>
    /// Helper method for handling the parsing of 3 coordinate Vertex points.
    /// </summary>
    /// <param name="line">The line of the file to be taken apart</param>
    /// <param name="firstSpaceIndex">The index of where the tag of the line ends</param>
    /// <returns>A VertexPoint with the needed coordinates</returns>
    private static Vector3 ParseVector3(string line, int firstSpaceIndex)
    {
        // Vertex definition - parse 3D coordinates (x, y, z)
        var coordinatesOnly = line[(firstSpaceIndex + 1)..];

        // Split "x y z" into individual coordinate strings
        var splitStringCoordinates = coordinatesOnly.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Convert coordinate strings to numbers
        var coordinates = new float[3];

        // Use TryParse to have a more robust parsing of the coordinates. If a coordinate is invalid, then it will be 0.0
        // Use NumberStyles to correctly parse floating-point numbers
        // Use InvariantCulture to handle decimal points correctly regardless of system locale
        for (var i = 0; i < Math.Min(splitStringCoordinates.Length, 3); i++)
        {
            float.TryParse(
                splitStringCoordinates[i],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out coordinates[i]
            );
        }

        // Return the Vector
        return new Vector3(coordinates);
    }

    /// <summary>
    /// Helper method for cleaner parsing of float values from the .obj file.
    /// </summary>
    /// <param name="line">The line of the file to be taken apart</param>
    /// <param name="firstSpaceIndex">The index of where the tag of the line ends</param>
    /// <returns>Either the float specified in the file or if it was invalid then 1.0f</returns>
    private static float ParseFloat(string line, int firstSpaceIndex)
    {
        return float.TryParse(line[firstSpaceIndex..], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 1.0f;
    }

    /// <summary>
    /// Method for importing materials from a .mtl file.
    /// </summary>
    /// <param name="fileName">File to import materials from</param>
    /// <param name="sceneDescription">SceneDescription to import the materials into</param>
    private static void ImportMaterials(string fileName, ImportedSceneDescription sceneDescription)
    {
        // Open the file for reading
        // Source: https://learn.microsoft.com/de-de/dotnet/api/system.io.streamreader?view=net-8.0
        using var streamReader = new StreamReader(fileName);

        // Create a material builder variable to initialize when the first material gets defined
        ObjectMaterialBuilder? currentMaterial = null;

        while (streamReader.ReadLine() is { } line)
        {
            // Skip empty lines
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            // Find the first space in the line to split the command from the rest of the arguments
            var firstSpaceIndex = line.IndexOf(' ');
            
            // Check what type of .obj command this line contains
            var firstWord = GetFirstWord(line);
            
            switch (firstWord)
            {
                // New material command found
                case "newmtl":
                    // Unless it's the first time setting newmtl, add the imported material to the material list
                    if (currentMaterial != null && !sceneDescription.Materials!.ContainsKey(currentMaterial.MaterialName))
                    {
                        // Add the material with the imported properties until now
                        sceneDescription.Materials.Add(currentMaterial.MaterialName, currentMaterial.BuildObjectMaterial());
                    }
                    // Create a new Material and give it the material name from the file
                    currentMaterial = new ObjectMaterialBuilder()
                    {
                        MaterialName = line[(firstSpaceIndex + 1)..]
                    };
                    break;
                
                // Specular exponent command found
                case "Ns":
                    // Set the according field in the material class
                    currentMaterial!.SpecularExponent = ParseFloat(line, firstSpaceIndex);
                    break;
                
                // Color ambient command found
                case "Ka":
                    // Set the according field in the material class
                    currentMaterial!.ColorAmbient = ParseVector3(line, firstSpaceIndex);
                    break;
                
                // Color diffuse command found
                case "Kd":
                    // Set the according field in the material class
                    currentMaterial!.ColorDiffuse = ParseVector3(line, firstSpaceIndex);
                    break;
                
                // Color specular command found
                case "Ks":
                    // Set the according field in the material class
                    currentMaterial!.ColorSpecular = ParseVector3(line, firstSpaceIndex);
                    break;
                
                // Optical density command found
                case "Ni":
                    // Set the according field in the material class
                    currentMaterial!.OpticalDensity = ParseFloat(line, firstSpaceIndex);
                    break;
                
                // Dissolve command found
                case "d":
                    // Set the according field in the material class
                    currentMaterial!.Dissolve = ParseFloat(line, firstSpaceIndex);
                    break;
                
                // Illumination model command found
                case "illum":
                    // Try parsing the given line in the file
                    // If the line is invalid, just use illumination model 2 (Color, ambient and highlight on)
                    currentMaterial!.IlluminationModel = int.TryParse(line[firstSpaceIndex..], out var value) ? value : 2;
                    break;
                
                // Diffuse texture command found
                case "map_Kd":
                    // Set the according field in the material class
                    currentMaterial!.DiffuseTexture = Path.Combine(Path.GetDirectoryName(fileName)!, line[(firstSpaceIndex + 1)..]);
                    break;
            }
        }
        
        // Add the last material (if it is there)
        if (currentMaterial != null && !sceneDescription.Materials!.ContainsKey(currentMaterial.MaterialName))
        {
            sceneDescription.Materials!.Add(currentMaterial!.MaterialName, currentMaterial.BuildObjectMaterial());
        }
    }

    /// <summary>
    /// Helper method for more reliably getting the first word (or the .obj/.mtl tag) of the line
    /// </summary>
    /// <param name="line">Line where the first word is to be extraced from</param>
    /// <returns>The first word of the line or the line if there is no space</returns>
    private static string GetFirstWord(string line)
    {
        try
        {
            return line[..line.IndexOf(' ')];
        }
        catch (ArgumentOutOfRangeException)
        {
            // There was no space
            return line;
        }
    }

    /// <summary>
    /// Splits a .obj file into separate sections, one for each 3D object defined in the file.
    /// </summary>
    /// <param name="filePath">Path to the .obj file to process</param>
    /// <returns>List of line groups, where each group contains all data for one 3D object</returns>
    private static List<List<string>> SplitFileByObjects(string filePath)
    {
        // Open the file for reading
        // Source: https://learn.microsoft.com/de-de/dotnet/api/system.io.streamreader?view=net-8.0
        using var streamReader = new StreamReader(filePath);
        
        // Store each object as a separate list of lines
        var objects = new List<List<string>>();
        // Collect lines for the current object being processed
        var currentFileSection = new List<string>();

        // Process the file line by line
        while (streamReader.ReadLine() is { } line)
        {
            // Skip empty lines
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            // Check what type of .obj command this line contains
            var firstWord = GetFirstWord(line);

            // "o" command starts a new 3D object definition
            // The example files given are incorrectly created and use "g" instead
            if (firstWord is "o" or "g")
            {
                objects.Add(currentFileSection);

                // Up the object count

                // Start collecting lines for this new object
                currentFileSection = [];
            }

            // Add this line to the current object's data
            currentFileSection.Add(line);
        }

        // Don't forget to add the final object when we reach the end of the file
        if (currentFileSection.Count > 0)
        {        
            objects.Add(currentFileSection);
        }

        return objects;
    }
}