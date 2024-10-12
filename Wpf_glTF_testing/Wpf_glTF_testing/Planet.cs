using glTFLoader.Schema;
using glTFLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Linq;
using System.Xml.Linq;

namespace Wpf_glTF_testing
{
    public class Planet
    {
        public UInt32 Magic { get; private set; }

        public UInt32 Version { get; private set; }

        public UInt32 TotalFileLength { get; private set; }

        public UInt32 JsonChuckLength { get; private set; }
        public UInt32 BinChuckLength { get; private set; }

        public byte[] JSON_data { get; private set; }
        public byte[] BIN_data { get; private set; }

        public Point3DCollection MaterialPoints { get; private set; } = new Point3DCollection();    

        public Vector3DCollection NormalPoints { get; private set; } = new Vector3DCollection();  

        public PointCollection TexturePoints { get; private set; } = new PointCollection();

        public Int32Collection Indecies { get; private set; } = new Int32Collection();

        public MeshGeometry3D Model3D { get; private set; }

        public GeometryModel3D GeoModel3D { get;set; } = new GeometryModel3D();

        public ModelVisual3D Visualisation { get; set; } = new ModelVisual3D();

        public List<BitmapImage> Images { get; private set; } = new List<BitmapImage>();

        public Planet(string filename)
        {
            // Get all metadata
            Gltf glTFFile = Interface.LoadModel(filename);

            // The different attributes in a reverse dictionary
            Dictionary<string,int> attributes = glTFFile.Meshes[0].Primitives[0].Attributes;
            Dictionary<int, string> AttrebutesIndex = attributes.ToDictionary(x => x.Value, x => x.Key);


            // Load all byte arrays from the Binary file glTF version 2
            using (var stream = File.Open(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    // Reading the inital data that determines the file type
                    Magic = reader.ReadUInt32();
                    Version = reader.ReadUInt32();
                    TotalFileLength  = reader.ReadUInt32();

                    // Read the JSON data
                    JsonChuckLength = reader.ReadUInt32();
                    UInt32 chunckType = reader.ReadUInt32();
                    // Should be equal to JSON_hex 0x4E4F534A;         
                    string hexValue = chunckType.ToString("X");

                    JSON_data = reader.ReadBytes((int)JsonChuckLength);

                    // Read teh binary data
                    BinChuckLength = reader.ReadUInt32();
                    UInt32 chunckType2 = reader.ReadUInt32();
                    // Should be equal to BIN_hex 0x004E4942;
                    string hexValue2 = chunckType2.ToString("X");
                    
                    BIN_data = reader.ReadBytes((int)BinChuckLength);
                }
            }

            for (int i = 0; i < glTFFile.Accessors.Count(); i++)
            {
                Accessor CurrentAccessor = glTFFile.Accessors[i];

                // Read the byte positions and offsets for each accessors
                var BufferViewIndex = CurrentAccessor.BufferView;
                BufferView BufferView = glTFFile.BufferViews[(int)BufferViewIndex];
                var Offset = BufferView.ByteOffset;
                var Length = BufferView.ByteLength;


                // Check which type of accessor it is
                string type = "";
                if (AttrebutesIndex.ContainsKey(i))
                    type = AttrebutesIndex[i];

                if (type == "POSITION")
                {
                    // Used to scale all planets to +/- 1
                    float[] ScalingFactorForVariables = new float[3];

                    if (CurrentAccessor.Max == null)
                        ScalingFactorForVariables = new float[3] { 1.0f, 1.0f, 1.0f };
                    else
                        ScalingFactorForVariables = CurrentAccessor.Max;

                    // Upscaling factor
                    float UpscalingFactor = 1.5f;

                    Point3DCollection PointsPosisions = new Point3DCollection();

                    for (int n = Offset; n < Offset + Length; n += 4)
                    {
                        float x = BitConverter.ToSingle(BIN_data, n) / ScalingFactorForVariables[0] * UpscalingFactor;
                        n += 4;
                        float y = BitConverter.ToSingle(BIN_data, n) / ScalingFactorForVariables[1] * UpscalingFactor;
                        n += 4;
                        float z = BitConverter.ToSingle(BIN_data, n) / ScalingFactorForVariables[2] * UpscalingFactor;

                        PointsPosisions.Add(new Point3D(x, y, z));
                    }
                    MaterialPoints = PointsPosisions;
                }
                else if (type == "NORMAL")
                {
                    Vector3DCollection NormalsForPosisions = new Vector3DCollection();
                    for (int n = Offset; n < Offset + Length; n += 4)
                    {
                        float x = BitConverter.ToSingle(BIN_data, n);
                        n += 4;
                        float y = BitConverter.ToSingle(BIN_data, n);
                        n += 4;
                        float z = BitConverter.ToSingle(BIN_data, n);

                        NormalsForPosisions.Add(new Vector3D(x, y, z));
                    }

                    NormalPoints = NormalsForPosisions;
                }
                else if (type.Contains("TEXCOORD"))
                {
                    // Assuming texture posisions
                    PointCollection vec2 = new PointCollection();
                    for (int n = Offset; n < Offset + Length; n += 4)
                    {
                        double x = (double)BitConverter.ToSingle(BIN_data, n);
                        n += 4;
                        double y = (double)BitConverter.ToSingle(BIN_data, n);

                        vec2.Add(new Point(x, y));
                    }

                    TexturePoints = vec2;
                }
                else
                {
                    if (CurrentAccessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
                    {
                        for (int n = Offset; n < Offset + Length; n += 2)
                        {
                            UInt16 TriangleItem = BitConverter.ToUInt16(BIN_data, n);
                            Indecies.Add((Int32)TriangleItem);

                        }
                    }
                }
            }

            // Showing the image
            foreach (glTFLoader.Schema.Image item in glTFFile.Images)
            {
                //var ImageType = item.MimeType;
                int BufferViewIndex = (int)item.BufferView;
                BufferView BufferView = glTFFile.BufferViews[BufferViewIndex];
                var Offset = BufferView.ByteOffset;
                var Length = BufferView.ByteLength;

                // Copy the relevant data from binary part
                byte[] ImageBytes = new byte[Length];
                Array.Copy(BIN_data, Offset, ImageBytes, 0, Length);

                // Conmvert to image
                MemoryStream ms = new MemoryStream(ImageBytes);
                BitmapImage Img = new BitmapImage();
                Img.BeginInit();
                Img.StreamSource = ms;
                Img.EndInit();
                Images.Add(Img);             
            }

            // Construct the WPF 3D ViewPort model
            Model3D = new MeshGeometry3D();
            Model3D.TriangleIndices = Indecies;
            Model3D.Positions = MaterialPoints;
            Model3D.Normals = NormalPoints;
            Model3D.TextureCoordinates = TexturePoints;

            // Geometry model
            GeoModel3D.Geometry = Model3D;

            if(glTFFile.Meshes.Count()==1)
                GeoModel3D.Material = new DiffuseMaterial() { Brush = new ImageBrush(Images[Images.Count-1]) };
            else
                GeoModel3D.Material = new DiffuseMaterial() { Brush = new ImageBrush(Images[0]) };
            
            // ModelVisual3D for showing this component
            Visualisation.Content = GeoModel3D;
        }
    }
}
