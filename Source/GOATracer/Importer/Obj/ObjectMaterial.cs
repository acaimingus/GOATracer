using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Class describing all the properties of a material used within a scene.
/// Source: https://paulbourke.net/dataformats/mtl/
/// </summary>
public class ObjectMaterial
{
    /// <summary>
    /// The name of the material.
    /// </summary>
    public string MaterialName { get; }

    /// <summary>
    /// Specular exponent value of the material.
    /// Also known as the Ns tag in .mtl files.
    /// </summary>
    public float SpecularExponent { get; }

    /// <summary>
    /// Color ambient value of the material.
    /// Also known as the Ka tag in .mtl files.
    /// </summary>
    public Vector3 ColorAmbient { get; }

    /// <summary>
    /// Color diffuse value of the material.
    /// Also known as the Kd tag in .mtl files.
    /// </summary>
    public Vector3 ColorDiffuse { get; }

    /// <summary>
    /// Color specular value of the material.
    /// Also known as the Ks tag in .mtl files.
    /// </summary>
    public Vector3 ColorSpecular { get; }

    /// <summary>
    /// Optical density/refraction of the material.
    /// Also known as the Ni tag in .mtl files.
    /// </summary>
    public float OpticalDensity { get; }

    /// <summary>
    /// Dissolve or the transparency of the material.
    /// Also known as the d tag in .mtl files.
    /// </summary>
    public float Dissolve { get; }

    /// <summary>
    /// Illumination model to be used on the material.
    /// Also known as the illum tag in .mtl files.
    /// </summary>
    public int IlluminationModel { get; }
    
    /// <summary>
    /// File path of the diffuse texture to be used in the material.
    /// Also known as the map_Kd tag in .mtl files.
    /// </summary>
    public string DiffuseTexture { get; }

    public ObjectMaterial(string materialName, float specularExponent, Vector3 colorAmbient, Vector3 colorDiffuse, Vector3 colorSpecular, float  opticalDensity, float dissolve, int illuminationModel, string diffuseTexturePath)
    {
        MaterialName = materialName;
        SpecularExponent = specularExponent;
        ColorAmbient = colorAmbient;
        ColorDiffuse = colorDiffuse;
        ColorSpecular = colorSpecular;
        OpticalDensity = opticalDensity;
        Dissolve = dissolve;
        IlluminationModel = illuminationModel;
        DiffuseTexture = diffuseTexturePath;
    }
}