using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// The ObjectMaterial class should have all its properties be readonly, but they cannot be all set in the constructor.
/// As a solution, use a builder to create the object and then create the actual ObjectMaterial after all attributes have been gathered.
/// Source: https://paulbourke.net/dataformats/mtl/
/// </summary>
public class ObjectMaterialBuilder
{
    /// <summary>
    /// The name of the material.
    /// </summary>
    public required string MaterialName { get; set; }

    /// <summary>
    /// Specular exponent value of the material.
    /// Also known as the Ns tag in .mtl files.
    /// </summary>
    public float? SpecularExponent { get; set; }

    /// <summary>
    /// Color ambient value of the material.
    /// Also known as the Ka tag in .mtl files.
    /// </summary>
    public Vector3? ColorAmbient { get; set; }

    /// <summary>
    /// Color diffuse value of the material.
    /// Also known as the Kd tag in .mtl files.
    /// </summary>
    public Vector3? ColorDiffuse { get; set; }

    /// <summary>
    /// Color specular value of the material.
    /// Also known as the Ks tag in .mtl files.
    /// </summary>
    public Vector3? ColorSpecular { get; set; }

    /// <summary>
    /// Optical density/refraction of the material.
    /// Also known as the Ni tag in .mtl files.
    /// </summary>
    public float? OpticalDensity { get; set; }

    /// <summary>
    /// Dissolve or the transparency of the material.
    /// Also known as the d tag in .mtl files.
    /// </summary>
    public float? Dissolve { get; set; }

    /// <summary>
    /// Illumination model to be used on the material.
    /// Also known as the illum tag in .mtl files.
    /// </summary>
    public int? IlluminationModel { get; set; }
    
    /// <summary>
    /// File path of the diffuse texture to be used in the material.
    /// Also known as the map_Kd tag in .mtl files.
    /// </summary>
    public string? DiffuseTexture { get; set; }

    /// <summary>
    /// Builder method for creating the actual read-only ObjectMaterial
    /// </summary>
    /// <returns></returns>
    public ObjectMaterial BuildObjectMaterial()
    {
        return new ObjectMaterial(
            SpecularExponent, ColorAmbient, ColorDiffuse, ColorSpecular, OpticalDensity, Dissolve, IlluminationModel, DiffuseTexture);
    }
}