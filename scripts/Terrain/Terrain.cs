using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Terrain : Node3D
{
	[Export]
	private int _divisions = 8;
	private Vector3[] _points;
	private int[] _indices;
	private int _maxPointWidth;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// INITIALIZING MESH GEOMETRY
		// Creating vertices
		List<List<int>> vertexIndices = [];
		int minXNum = (int) (Math.Pow(2, _divisions) + 1);
		int maxXNum = (int) (Math.Pow(2, _divisions + 1) + 1);
		var zNum = maxXNum;
		float l = 128f / (maxXNum - 1);
		float h = (float) (.5 * l * Math.Sqrt(3));
		List<Vector3> pointsArray = [];
		int index = 0;
		for(int z = 0; z < zNum; z++)
		{
			int xNum;
			bool secondHalf;
			int secondHalfZ = z - (zNum / 2 + 1) + 1;
			if(secondHalfZ >= 0)
			{
				xNum = maxXNum - secondHalfZ;
				secondHalf = true;
			}
			else
			{
				xNum = minXNum + z;
				secondHalf = false;
			}
			List<int> rowIndices = [];
			for(int x = 0; x < xNum; x++)
			{
				float currentX;
				if(secondHalf)
				{
					currentX = (float) (-64.0 + 0.5 * l * secondHalfZ + l * x);
				}
				else
				{
					currentX = (float) (-32.0 - 0.5 * l * z + l * x);
				}
				var currentZ = (float) (32 * Math.Sqrt(3) - h * z);
				pointsArray.Add(new Vector3(currentX, 0f, currentZ));
				rowIndices.Add(index++);
			}
			vertexIndices.Add(rowIndices);
		}
		_points = [.. pointsArray];
		_maxPointWidth = maxXNum;
		// Create indices
		List<int> indices = [];
		for(int z = 0; z < vertexIndices.Count - 1; z++)
		{
			var topRow = vertexIndices[z];
			var bottomRow = vertexIndices[z + 1];
			var topRowLen = topRow.Count;
			var bottomRowLen = bottomRow.Count;
			if(bottomRowLen > topRowLen)
			{
				for(int x = 0; x < bottomRowLen + topRowLen - 2; x++)
				{
					int[] triangle;
					if(x % 2 == 0)
					{
						triangle = [bottomRow[x / 2], bottomRow[x / 2 + 1], topRow[x / 2]];
					}
					else
					{
						triangle = [topRow[x / 2], bottomRow[x / 2 + 1], topRow[x / 2 + 1]];
					}
					indices.AddRange(triangle);
				}
			}
			else
			{
				for(int x = 0; x < bottomRowLen + topRowLen - 2; x++)
				{
					int[] triangle;
					if(x % 2 == 0)
					{
						triangle = [topRow[x / 2], bottomRow[x / 2], topRow[x / 2 + 1]];
					}
					else
					{
						triangle = [bottomRow[x / 2], bottomRow[x / 2 + 1], topRow[x / 2 + 1]];
					}
					indices.AddRange(triangle);
				}
			}
		}
		_indices = [.. indices];
		// Add origin cell
		AddCell(Vector2I.Zero);
	}
	public void AddCell(Vector2I position)
	{
		Array<Variant> arrays = [];
		arrays.Resize((int) Mesh.ArrayType.Max);
		arrays[(int) Mesh.ArrayType.Vertex] = _points;
		arrays[(int) Mesh.ArrayType.Index] = _indices;

        var material = new ShaderMaterial
        {
            Shader = GD.Load<Shader>("res://shaders/terrain.gdshader")
        };
        var noise = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Seed = (int) Math.Floor(GD.Randf()),
			Frequency = 0.01f,
			FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
			FractalOctaves = 5,
			FractalLacunarity = 2.0f,
			FractalGain = 0.5f
        };
		var heightmapImg = noise.GetImage(_maxPointWidth, _maxPointWidth);

		// Blur heightmap
		// Create rendering device and shader
		var rd = RenderingServer.CreateLocalRenderingDevice();
        var shaderFile = GD.Load<RDShaderFile>("res://shaders/glsl/blur.glsl");
        var shaderSpirv = shaderFile.GetSpirV();
        var shader = rd.ShaderCreateFromSpirV(shaderSpirv);
		// Create uniforms
		var heightmapInputData = heightmapImg.GetData().Select(x => (int)x).ToArray();
		var heightmapBytes = new byte[heightmapInputData.Length * sizeof(int)];
		Buffer.BlockCopy(heightmapInputData, 0, heightmapBytes, 0, heightmapBytes.Length);
		var inputBuffer = rd.StorageBufferCreate((uint) heightmapBytes.Length, heightmapBytes);
		var inputUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 0
		};
		inputUniform.AddId(inputBuffer);
		var outputData = new byte[heightmapInputData.Length * sizeof(int)];
		System.Array.Fill(outputData, (byte) 0);
		var outputBuffer = rd.StorageBufferCreate((uint) heightmapBytes.Length, outputData);
		var outputUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 1
		};
		outputUniform.AddId(outputBuffer);
		int[] sizeInt = [_maxPointWidth];
		var sizeData = new byte[sizeof(int)];
		Buffer.BlockCopy(sizeInt, 0, sizeData, 0, sizeData.Length);
		var sizeBuffer = rd.StorageBufferCreate(sizeof(int), sizeData);
		var sizeUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 2
		};
		sizeUniform.AddId(sizeBuffer);
		int[] radiusInt = [4];
		var radiusData = new byte[sizeof(int)];
		Buffer.BlockCopy(radiusInt, 0, radiusData, 0, radiusData.Length);
		var radiusBuffer = rd.StorageBufferCreate(sizeof(int), radiusData);
		var radiusUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 3
		};
		radiusUniform.AddId(radiusBuffer);
		var uniformSet = rd.UniformSetCreate(
			[
				inputUniform,
				outputUniform,
				sizeUniform,
				radiusUniform
			],
			shader,
			0
		);
		// Create compute pipeline
		var pipeline = rd.ComputePipelineCreate(shader);
		var computeList = rd.ComputeListBegin();
		rd.ComputeListBindComputePipeline(computeList, pipeline);
		rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
		// FIXME: this isn't size-dependent.
		rd.ComputeListDispatch(computeList, xGroups: 512/16, yGroups: 512/16, zGroups: 1);
		rd.ComputeListEnd();
		// Run blur
		rd.Submit();
		rd.Sync();

		// Finish
		// Create texture
		var outputBytes = rd.BufferGetData(outputBuffer);
		var blurredInts = new int[heightmapInputData.Length];
		Buffer.BlockCopy(outputBytes, 0, blurredInts, 0, heightmapInputData.Length);
		var blurredImageData = blurredInts.Select(x => (byte)x).ToArray();
		var blurredImage = Image.CreateFromData(_maxPointWidth, _maxPointWidth, false, Image.Format.L8, blurredImageData);
		var blurredTex = ImageTexture.CreateFromImage(blurredImage);
		material.SetShaderParameter("heightmap", blurredTex);
		// Create mesh
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, (Godot.Collections.Array) arrays);
		mesh.SurfaceSetMaterial(0, material);
        var meshInstance = new MeshInstance3D
        {
            Mesh = mesh
        };
		// Add child
		AddChild(meshInstance);
    }
}
