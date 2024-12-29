using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Chris
{
	/// <summary>
	/// Update mesh collider using JobSystem, but may still cause visible overhead.
	/// </summary>
	public class SkinnedMeshCollider : MonoBehaviour
	{
		public enum UpdateFrequency
		{
			Low,
			Default,
			High
		}
		private readonly struct VertexBoneWeight
		{
			public VertexBoneWeight(Vector3 pos, int boneId, float weight)
			{
				this.pos = pos;
				this.weight = weight;
				this.boneId = boneId;
			}
			public readonly int boneId;
			public readonly float3 pos;
			public readonly float weight;
		}
		[BurstCompile]
		private struct UpdateVerticesJob : IJobParallelFor
		{
			// Allow parallel writing
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<VertexBoneWeight> weights;
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> ids;
			[ReadOnly]
			public NativeArray<float4x4> matrix;
			[ReadOnly]
			public float4x4 transformMatrix;
			[BurstCompile]
			public void Execute(int index)
			{
				float4 vertex = new(0, 0, 0, 1);
				foreach (var id in ids.GetValuesForKey(index))
				{
					VertexBoneWeight weight = weights[id];
					float4 pos = new(weight.pos, 1);
					vertex += math.mul(matrix[weight.boneId], pos) * weight.weight;
				}
				vertices[index] = math.mul(transformMatrix, vertex).xyz;
			}
		}
		private bool hasJob;
		private NativeArray<float4x4> matrix_job_array;
		private NativeArray<float3> vertices_job_array;
		private JobHandle jobHandle;
		public bool forceUpdate;
		public bool updateOncePerFrame = true;
		private bool IsInit;
		private NativeParallelMultiHashMap<int, int> ids;
		private NativeArray<VertexBoneWeight> weights;
		private SkinnedMeshRenderer skinnedMeshRenderer;
		private MeshCollider meshCollider;
		private Mesh meshCalc;
		private int verticesL;
		private float frame;
		private float FrameRate => frequencies[(int)frequency];
		private static readonly float[] frequencies = new float[3] { 1f / 30, 1f / 60, 1f / 90 };
		public UpdateFrequency frequency = UpdateFrequency.Default;
		private void Start()
		{
			Init();
		}
		public bool Init()
		{
			if (IsInit)
			{
				return true;
			}
			skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			meshCollider = GetComponent<MeshCollider>();
			if (!meshCollider || !skinnedMeshRenderer) return false;
			meshCalc = Instantiate(skinnedMeshRenderer.sharedMesh);
			meshCalc.name = skinnedMeshRenderer.sharedMesh.name + "_calc";
			meshCollider.sharedMesh = meshCalc;
			meshCalc.MarkDynamic();
			Vector3[] vertices = skinnedMeshRenderer.sharedMesh.vertices;
			verticesL = vertices.Length;
			Matrix4x4[] bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
			BoneWeight[] boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
			var weights = new List<VertexBoneWeight>();
			int id = 0;
			ids = new NativeParallelMultiHashMap<int, int>(vertices.Length, Allocator.Persistent);
			for (int j = 0; j < vertices.Length; j++)
			{
				BoneWeight boneWeight = boneWeights[j];
				if (boneWeight.weight0 != 0f)
				{
					Vector3 p = bindPoses[boneWeight.boneIndex0].MultiplyPoint3x4(vertices[j]);
					weights.Add(new VertexBoneWeight(p, boneWeight.boneIndex0, boneWeight.weight0));
					ids.Add(j, id++);
				}
				if (boneWeight.weight1 != 0f)
				{
					Vector3 p2 = bindPoses[boneWeight.boneIndex1].MultiplyPoint3x4(vertices[j]);
					weights.Add(new VertexBoneWeight(p2, boneWeight.boneIndex1, boneWeight.weight1));
					ids.Add(j, id++);
				}
				if (boneWeight.weight2 != 0f)
				{
					Vector3 p3 = bindPoses[boneWeight.boneIndex2].MultiplyPoint3x4(vertices[j]);
					weights.Add(new VertexBoneWeight(p3, boneWeight.boneIndex2, boneWeight.weight2));
					ids.Add(j, id++);
				}
				if (boneWeight.weight3 != 0f)
				{
					Vector3 p4 = bindPoses[boneWeight.boneIndex3].MultiplyPoint3x4(vertices[j]);
					weights.Add(new VertexBoneWeight(p4, boneWeight.boneIndex3, boneWeight.weight3));
					ids.Add(j, id++);
				}
			}
			this.weights = new NativeArray<VertexBoneWeight>(weights.ToArray(), Allocator.Persistent);
			matrix_job_array = new(skinnedMeshRenderer.bones.Length, Allocator.Persistent);
			vertices_job_array = new(verticesL, Allocator.Persistent);
			IsInit = true;
			return true;
		}
		private void OnDisable()
		{
			if (hasJob)
			{
				jobHandle.Complete();
				hasJob = false;
			}
		}
		private void OnDestroy()
		{
			if (hasJob)
			{
				jobHandle.Complete();
				hasJob = false;
			}
			Release();
		}
		private void Update()
		{
			if (!IsInit)
			{
				return;
			}
			if (forceUpdate)
			{
				frame += Time.deltaTime;
				if (frame < FrameRate) return;
				frame = 0;
				if (updateOncePerFrame)
				{
					forceUpdate = false;
				}
				RunUpdateMeshJob();
			}
		}
		private void LateUpdate()
		{
			if (hasJob)
			{
				jobHandle.Complete();
				meshCalc.SetVertices(vertices_job_array);
				meshCollider.enabled = false;
				meshCollider.enabled = true;
				hasJob = false;
			}
		}
		public bool Release()
		{
			IsInit = false;
			weights.Dispose();
			ids.Dispose();
			vertices_job_array.Dispose();
			matrix_job_array.Dispose();
			Destroy(meshCalc);
			return true;
		}
		private void RunUpdateMeshJob()
		{
			hasJob = true;
			for (int i = 0; i < matrix_job_array.Length; ++i)
			{
				matrix_job_array[i] = skinnedMeshRenderer.bones[i].transform.localToWorldMatrix;
			}
			UpdateVerticesJob job = new()
			{
				vertices = vertices_job_array,
				matrix = matrix_job_array,
				weights = weights,
				ids = ids,
				transformMatrix = transform.localToWorldMatrix.inverse
			};
			jobHandle = job.Schedule(verticesL, 64);
		}

	}
}