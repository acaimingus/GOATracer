namespace GOATracer.Lights;

/// <summary>
/// Class storing the data of the light entities.
/// </summary>
public class Light
{
    /// <summary>
    /// ID of this light control
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// X position of the light
    /// </summary>
    public float X { get; set; }
    /// <summary>
    /// Y position of the light
    /// </summary>
    public float Y { get; set; }
    /// <summary>
    /// Z position of the light
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">ID of the light</param>
    public Light(int id)
    {
        Id = id;
        X = 0;
        Y = 0;
        Z = 0;
    }
}