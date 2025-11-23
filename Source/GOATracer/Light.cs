namespace GOATracer
{
    /// <summary>
    /// Light class representing a light source in the scene
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Gets or sets the unique identifier for the light.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the light.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the light is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// Gets or sets the X-coordinate of the light's position.
        /// </summary>
        public double LightPositionX { get; set; }
        /// <summary>
        /// Gets or sets the Y-coordinate of the light's position.
        /// </summary>
        public double LightPositionY { get; set; }
        /// <summary>
        /// Gets or sets the Z-coordinate of the light's position.
        /// </summary>
        public double LightPositionZ { get; set; }
    }
}
