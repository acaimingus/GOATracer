using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Class describing all the properties of a material used within a scene.
/// Source: https://paulbourke.net/dataformats/mtl/
/// </summary>
public class ObjectMaterial
{
    /// <summary>
    /// Specular exponent value of the material.
    /// Also known as the Ns tag in .mtl files.
    /// </summary>
    public float? SpecularExponent { get; }

    /// <summary>
    /// Color ambient value of the material.
    /// Also known as the Ka tag in .mtl files.
    /// </summary>
    public Vector3? ColorAmbient { get; }

    /// <summary>
    /// Color diffuse value of the material.
    /// Also known as the Kd tag in .mtl files.
    /// </summary>
    public Vector3? ColorDiffuse { get; }

    /// <summary>
    /// Color specular value of the material.
    /// Also known as the Ks tag in .mtl files.
    /// </summary>
    public Vector3? ColorSpecular { get; }

    /// <summary>
    /// Optical density/refraction of the material.
    /// Also known as the Ni tag in .mtl files.
    /// </summary>
    public float? OpticalDensity { get; }

    /// <summary>
    /// Dissolve or the transparency of the material.
    /// Also known as the d tag in .mtl files.
    /// </summary>
    public float? Dissolve { get; }

    /// <summary>
    /// Illumination model to be used on the material.
    /// Also known as the illum tag in .mtl files.
    /// </summary>
    public int? IlluminationModel { get; }
    
    /// <summary>
    /// File path of the diffuse texture to be used in the material.
    /// Also known as the map_Kd tag in .mtl files.
    /// </summary>
    public string? DiffuseTexture { get; }

    /// <summary>
    /// Constructor for the Material itself.
    /// Called by the MaterialBuilder.
    /// </summary>
    /// <param name="specularExponent">The desired specular exponent</param>
    /// <param name="colorAmbient">The desired ambient color</param>
    /// <param name="colorDiffuse">The desired diffuse color</param>
    /// <param name="colorSpecular">The desired specular color</param>
    /// <param name="opticalDensity">The desired optical density</param>
    /// <param name="dissolve">The desired dissolve</param>
    /// <param name="illuminationModel">The desired illumination model</param>
    /// <param name="diffuseTexture">The diffuse texture to use for the material</param>
    public ObjectMaterial(float? specularExponent, Vector3? colorAmbient, Vector3? colorDiffuse, Vector3? colorSpecular, float? opticalDensity, float? dissolve, int? illuminationModel, string? diffuseTexture)
    { 
        SpecularExponent = specularExponent;
        ColorAmbient = colorAmbient;
        ColorDiffuse = colorDiffuse;
        ColorSpecular = colorSpecular;
        OpticalDensity = opticalDensity;
        Dissolve = dissolve;
        IlluminationModel = illuminationModel;
        DiffuseTexture = diffuseTexture;
    }
}