namespace Gobchat
{

            public class Point3F
            {
                public float x;
                public float y;
                public float z;

                public Point3F(float x, float y, float z)
                {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }

                public override string ToString()
                {
                    var className = nameof(Point3F);
                    return $"{className} => x:{x} | y:{y} | z:{z}";
                }
            }
    
}
