<ul class="download">
	<li><a href="Wpf_glTF_testing.zip">Download source code - 55.7 MB</a></li>
</ul>

<p><img src="glft_program.png" style="width: 640px; height: 401px" /></p>

<h2>Introduction</h2>

<p>I got a little sidetracked on my project of solving differential equations. I really wanted to show how to calculate the trajectories of the planets in the solar system, with corrections for general relativity. Some planets won&#39;t actually have correct paths if you only use Newtons gravitational law so you need to solve some non-linear differential equations with relatively huge timesteps, which is perfect for the backwards Euler integration method.</p>

<p>In any event, I wanted to show the resulting planets in 3D so you could see the orbit. But different planets have different colors and makeup, so in order to separate them, I would need some coloring or equivalent <code>ImageBrushes</code> of textures if that was going to happen. That is when I stumbled upon <a href="https://solarsystem.nasa.gov/resources/all/?order=pub_date+desc&amp;per_page=50&amp;page=0&amp;search=3D&amp;condition_1=1%3Ais_in_resource_list&amp;fs=&amp;fc=&amp;ft=&amp;dp=&amp;category=">NASA&#39;s 3D resources</a>. They have all the 3D images stored in a glTF file format (<em>*.glb</em>) that you could download and use, but how to show these files in WPF 3D and what are these files really?</p>

<p>I am only going to use this tool to get some very simple 3D shapes and you have to rewrite the code for it to work in a general way.</p>

<h2>Background</h2>

<p><a href="https://www.khronos.org/gltf/">glTF</a> stands for <strong>g</strong>raphics <strong>l</strong>anguage <strong>T</strong>ransmission <strong>F</strong>ormat and seems to be the new standard that everyone is implementing when it comes to storing and sending 3D graphical components. The current version spec is maintained by the <a href="https://www.khronos.org/">Khronos Group</a>, which also publishes a lot of developer material on their Github account.</p>

<p>The documentation that describes the new standard 2.0 version of the exchange format is given <a href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html">here</a>. A quick guide in the format can be viewed in the <a href="https://www.khronos.org/files/gltf20-reference-guide.pdf">following quick guide pdf document</a>.</p>

<p>The document type is actually quite simply arranged, taken from the quick guide:</p>

<p><img src="gltf_BuildUp.png" style="height: 179px; width: 640px" /></p>

<p>So the code for loading in a <em>*.glb</em> is actually quite simple:</p>

<pre lang="cs">
// Load all byte arrays from the Binary file glTF version 2
using (var stream = File.Open(filename, FileMode.Open))
{
    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
    {
        // Reading the initial data that determines the file type
        Magic = reader.ReadUInt32();
        Version = reader.ReadUInt32();
        TotalFileLength  = reader.ReadUInt32();
        
        // Read the JSON data
        JsonChuckLength = reader.ReadUInt32();
        UInt32 chunckType = reader.ReadUInt32();
        // Should be equal to JSON_hex 0x4E4F534A;         
        string hexValue = chunckType.ToString(&quot;X&quot;);
        
        JSON_data = reader.ReadBytes((int)JsonChuckLength);
        
        // Read the binary data
        BinChuckLength = reader.ReadUInt32();
        UInt32 chunckType2 = reader.ReadUInt32();
        // Should be equal to BIN_hex 0x004E4942;
        string hexValue2 = chunckType2.ToString(&quot;X&quot;);
        
        BIN_data = reader.ReadBytes((int)BinChuckLength);
    }
}</pre>

<p>We now have extracted the JSON data and the Binary data in two different arrays. However, there is already a tool created for extracting all the information from the JSON that is available both on <a href="https://github.com/KhronosGroup/glTF-CSharp-Loader">github </a>and as a NuGet package. This will however only extract information on how the file is organized, what is where and so on. The actual data is either in the binary part, or in separate files.</p>

<p>There are a lot of resources on KhoronosGroup Github account but they are mostly for programming languages other than C#.</p>

<h2>Extract the 3D Model</h2>

<p>Any 3D model will have some position data that is usually organized with triangle indices, and each of these triangles will have a normal vector that gives its direction. Additionally, there might also, as in my case, be images that have some texture coordinates.</p>

<p>Describing what data is used in each object is given in the <code>Meshes</code> object that is extracted from the glTF JSON. Since I only want to show a planet, each file will only contain one mesh, but in general, as with the planet Saturn and its rings, there could be many meshes for each glb file. But for the simplicity and showing the principle, I have excluded the more difficult file types.</p>

<p>Each of the files will have so-called Accessors that will point to the position in the binary file where the actual information is stored. So here, I extract the information for each of the meshes (or mesh in this case).</p>

<pre lang="cs">
for (int i = 0; i &lt; glTFFile.Accessors.Count(); i++)
{
    Accessor CurrentAccessor = glTFFile.Accessors[i];
    
    // Read the byte positions and offsets for each accessors
    var BufferViewIndex = CurrentAccessor.BufferView;
    BufferView BufferView = glTFFile.BufferViews[(int)BufferViewIndex];
    var Offset = BufferView.ByteOffset;
    var Length = BufferView.ByteLength;
    
    // Check which type of accessor it is
    string type = &quot;&quot;;
    if (AttrebutesIndex.ContainsKey(i))
        type = AttrebutesIndex[i];
        
    if (type == &quot;POSITION&quot;)
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
        
        for (int n = Offset; n &lt; Offset + Length; n += 4)
        {
            float x = BitConverter.ToSingle(BIN_data, n) / 
                                   ScalingFactorForVariables[0] * UpscalingFactor;
            n += 4;
            float y = BitConverter.ToSingle(BIN_data, n) / 
                                   ScalingFactorForVariables[1] * UpscalingFactor;
            n += 4;
            float z = BitConverter.ToSingle(BIN_data, n) / 
                                   ScalingFactorForVariables[2] * UpscalingFactor;
            
            PointsPosisions.Add(new Point3D(x, y, z));
        }
        MaterialPoints = PointsPosisions;
    }
    else if (type == &quot;NORMAL&quot;)
    {
        Vector3DCollection NormalsForPosisions = new Vector3DCollection();
        for (int n = Offset; n &lt; Offset + Length; n += 4)
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
    else if (type.Contains(&quot;TEXCOORD&quot;))
    {
        // Assuming texture positions
        PointCollection vec2 = new PointCollection();
        for (int n = Offset; n &lt; Offset + Length; n += 4)
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
            for (int n = Offset; n &lt; Offset + Length; n += 2)
            {
                UInt16 TriangleItem = BitConverter.ToUInt16(BIN_data, n);
                Indecies.Add((Int32)TriangleItem);
            }
        }
    }
}</pre>

<p>If you have texture coordinates, there will also be images that you could load from either the binary part or in a separate file.</p>

<pre lang="cs">
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

	// Convert to image
	MemoryStream ms = new MemoryStream(ImageBytes);
	BitmapImage Img = new BitmapImage();
	Img.BeginInit();
	Img.StreamSource = ms;
	Img.EndInit();
	Images.Add(Img);             
}</pre>

<h2>Generate WPF 3D glTF Viewer</h2>

<p>Adding all this information to a <code>ModelVisual3D</code> that can be used for showing it in a <code>Viewport3d</code> is relatively straightforward.</p>

<pre lang="cs">
// Construct the WPF 3D ViewPort model
Model3D = new MeshGeometry3D();
Model3D.TriangleIndices = Indecies;
Model3D.Positions = MaterialPoints;
Model3D.Normals = NormalPoints;
Model3D.TextureCoordinates = TexturePoints;

// Geometry model
GeoModel3D.Geometry = Model3D;
GeoModel3D.Material = new DiffuseMaterial() { Brush = new ImageBrush(Images[0]) };

// ModelVisual3D for showing this component
Visualisation.Content = GeoModel3D;</pre>

<p>The <code>Viewport3D</code> is very simple and all I need is to position the camera and give some lights to the scene. All the planets are centered in origin (0,0,0).</p>

<pre lang="xaml">
&lt;Viewport3D Name=&quot;viewport3D1&quot; Width=&quot;400&quot; Height=&quot;400&quot;&gt;
	&lt;Viewport3D.Camera&gt;
		&lt;PerspectiveCamera x:Name=&quot;camMain&quot; 
		Position=&quot;6 5 4&quot; LookDirection=&quot;-6 -5 -4&quot;&gt;
		&lt;/PerspectiveCamera&gt;
	&lt;/Viewport3D.Camera&gt;
	&lt;ModelVisual3D&gt;
		&lt;ModelVisual3D.Content&gt;
			&lt;DirectionalLight x:Name=&quot;dirLightMain&quot; Direction=&quot;-1,-1,-1&quot;&gt;
			&lt;/DirectionalLight&gt;
		&lt;/ModelVisual3D.Content&gt;
	&lt;/ModelVisual3D&gt;    
&lt;/Viewport3D&gt;</pre>

<p>I stole some ideas on simple zooming and rotating from <a href="https://ericsink.com/wpf3d/9_Rotate_Zoom.html">this site</a>.</p>

<h2>History</h2>

<ul>
	<li>2<sup>nd</sup> May, 2023: Initial version</li>
</ul>
