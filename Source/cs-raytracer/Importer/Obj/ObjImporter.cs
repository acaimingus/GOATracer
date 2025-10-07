using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TracerUIMockup.Importer.Obj;

public class ObjImporter
{
    public SceneDescription ImportModel(string filePath)
    {
        // Split the file into sections based on the models specified within
        var wavefrontObjects = SplitFileByObjects(filePath);

        // Create a description for the scene
        var sceneDescription = new SceneDescription(Path.GetFileName(filePath));
        
        foreach (var wavefrontObject in wavefrontObjects)
        {
            // Create an object description
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
                
                // Get the first word of the line to determine what type it is
                var firstWord = line.Substring(0, firstSpaceIndex);

                // Filter the file sections for vertices and faces
                switch (firstWord)
                {
                    case "o":
                        // Remove the tag
                        var nameOnly = line[(firstSpaceIndex + 1)..];
                        objectDescription.objectName = nameOnly;
                        break;
                    // Vertex found
                    case "v":
                        // Remove the v tag to only have the coordinates
                        var coordinatesOnly = line[(firstSpaceIndex + 1)..];
                        
                        // Split the line into the coordinates
                        var splitStringCoordinates = coordinatesOnly.Split(' ');
                        
                        // Assign X,Y and Z to the corresponding index
                        var coordinates = new double[3];
                        
                        // Use InvariantCulture to avoid problems with culture number parsing (0,5 or 0.5)
                        coordinates[0] = double.Parse(splitStringCoordinates[0], CultureInfo.InvariantCulture);
                        coordinates[1] = double.Parse(splitStringCoordinates[1], CultureInfo.InvariantCulture);
                        coordinates[2] = double.Parse(splitStringCoordinates[2], CultureInfo.InvariantCulture);
                        
                        // Add the vertex point to the global vertex point list
                        sceneDescription.VertexPoints?.Add(new VertexPoint(coordinates));
                        break;
                    // Face found
                    case "f":
                        // Remove the f tag to only have the indices
                        var indicesOnly = line[(firstSpaceIndex + 1)..];
                        
                        // Split the line by each index group
                        var splitStringIndices = indicesOnly.Split(' ');
                        
                        var indices = new List<int>();

                        // Indices might contain v/vt/vn info, only use the first element
                        foreach (var index in splitStringIndices)
                        {
                            var vertexIndexOnly = index.Split('/');
                            indices.Add(int.Parse(vertexIndexOnly[0]));
                        }
                        
                        // Add the indices
                        objectDescription.FacePoints.Add(new FaceDescription(indices));
                        break;
                }
            }
            
            // Add the objectDescription to the SceneDescription
            sceneDescription.ObjectDescriptions?.Add(objectDescription);
        }
        
        return sceneDescription;
    }

    /// <summary>
    /// Method for splitting the file into sections based on objects in that file
    /// </summary>
    /// <param name="filePath">File to be split</param>
    /// <returns>A list of sections containing all the relevant information for each object</returns>
    private List<List<string>> SplitFileByObjects(string filePath)
    {
                
        // Open up a stream-reader at the specified path
        using var streamReader = new StreamReader(filePath);
        
        // Split the file by objects
        var objectCount = 0;
        // Create a list for the objects itself
        var objects = new List<List<string>>();
        // Save the lines from the currently read section
        var currentFileSection = new List<string>();
        
        // Read all lines from the file
        string? line;
        while ((line = streamReader.ReadLine()) != null)
        {
            // If the line is empty, skip it
            if (string.IsNullOrEmpty(line))
            {
                continue;    
            }
            
            // Get the first word of the line to determine what type it is
            var firstWord = line.Substring(0, line.IndexOf(' '));
            
            // o is in the beginning of the file, a new object was found
            if (firstWord == "o")
            {
                // Check if it is the first occurence of the o tag
                // If it is, then skip adding it to the list, because everything before the first o tag is not an object
                if (objectCount != 0)
                {
                    objects.Add(currentFileSection);
                    
                }
                // Up the object count
                ++objectCount;
                
                // Create a new list to start adding lines for the new object
                currentFileSection = [];
            }
            
            // Add the current line to the current file section
            currentFileSection.Add(line);
        }
        // Add the last object
        objects.Add(currentFileSection);

        return objects;
    }
}