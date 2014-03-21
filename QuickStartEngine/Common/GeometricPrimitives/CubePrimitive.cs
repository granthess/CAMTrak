using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuickStart.GeometricPrimitives
{
    /// <summary>
    /// Geometric primitive class for drawing cubes.
    /// </summary>
    public class CubePrimitive : GeometricPrimitive
    {
        /// <summary>
        /// Constructs a new cube primitive, using default settings.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1)
        {
        }

        public CubePrimitive( GraphicsDevice graphicsDevice, float height, float width, float depth )
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Six indices (two triangles) per face.
                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 1);
                AddIndex(CurrentVertex + 2);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 2);
                AddIndex(CurrentVertex + 3);

                if (normal.X != 0)
                {
                    Vector3 center = normal;
                    center.X *= (width * 0.5f);

                    Vector3 right = new Vector3(0.0f, 0.0f, ( depth * 0.5f ) * normal.X);
                    Vector3 up = new Vector3(0.0f, ( height * 0.5f ), 0.0f);

                    // Four vertices per face.
                    AddVertex(center + right - up, normal);     // Bottom right
                    AddVertex(center + right + up, normal);     // Top right
                    AddVertex(center - right + up, normal);     // Top left
                    AddVertex(center - right - up, normal);     // Bottom left
                }
                else if (normal.Y != 0)
                {
                    Vector3 center = normal;
                    center.Y *= (height * 0.5f);

                    Vector3 right = new Vector3(( width * 0.5f ) * normal.Y, 0.0f, 0.0f);
                    Vector3 up = new Vector3(0.0f, 0.0f, ( depth * 0.5f ));

                    // Four vertices per face.
                    AddVertex(center + right - up, normal);     // Bottom right
                    AddVertex(center + right + up, normal);     // Top right
                    AddVertex(center - right + up, normal);     // Top left
                    AddVertex(center - right - up, normal);     // Bottom left
                }
                else if (normal.Z != 0)
                {
                    Vector3 center = normal;
                    center.Z *= (depth * 0.5f);

                    Vector3 right = new Vector3(( width * 0.5f ) * normal.Z, 0.0f, 0.0f);
                    Vector3 up = new Vector3(0.0f, ( height * 0.5f ), 0.0f);

                    // Four vertices per face.
                    AddVertex(center - right - up, normal);     // Bottom left
                    AddVertex(center - right + up, normal);     // Top left
                    AddVertex(center + right + up, normal);     // Top right
                    AddVertex(center + right - up, normal);     // Bottom right
                }
            }

            InitializePrimitive(graphicsDevice);
        }

        /// <summary>
        /// Constructs a new cube primitive, with the specified size.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice, float size)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

                // Six indices (two triangles) per face.
                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 1);
                AddIndex(CurrentVertex + 2);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 2);
                AddIndex(CurrentVertex + 3);

                // Four vertices per face.
                AddVertex((normal - side1 - side2) * size / 2, normal);
                AddVertex((normal - side1 + side2) * size / 2, normal);
                AddVertex((normal + side1 + side2) * size / 2, normal);
                AddVertex((normal + side1 - side2) * size / 2, normal);
            }

            InitializePrimitive(graphicsDevice);
        }
    }
}
