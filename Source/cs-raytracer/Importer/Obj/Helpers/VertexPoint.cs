namespace TracerUIMockup.Importer.Obj.Helpers;

public class VertexPoint(double[] coordinates)
{
    private readonly double _x = coordinates[0];
    private readonly double _y = coordinates[1];
    private readonly double _z = coordinates[2];

    public double[] GetCoordinates()
    {
        return [_x, _y, _z];
    }
}