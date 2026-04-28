using System;
using System.Collections.Generic;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Slicing
{
    public readonly struct SliceResult
    {
        public SliceResult(
            Mesh positiveMesh,
            Mesh negativeMesh,
            Material[] positiveMaterials,
            Material[] negativeMaterials)
        {
            PositiveMesh = positiveMesh;
            NegativeMesh = negativeMesh;
            PositiveMaterials = positiveMaterials;
            NegativeMaterials = negativeMaterials;
        }

        public Mesh PositiveMesh { get; }
        public Mesh NegativeMesh { get; }
        public Material[] PositiveMaterials { get; }
        public Material[] NegativeMaterials { get; }
    }

    public static class MeshSlicer
    {
        private const float Epsilon = 0.0001f;

        public static bool Slice(
            Mesh sourceMesh,
            Plane slicePlane,
            Material[] sourceMaterials,
            Material cutSurfaceMaterial,
            out SliceResult sliceResult)
        {
            sliceResult = default;

            if (sourceMesh == null || sourceMesh.vertexCount < 3)
                return false;

            Vector3[] sourceVertices = sourceMesh.vertices;
            Vector3[] sourceNormals = sourceMesh.normals;
            Vector2[] sourceUvs = sourceMesh.uv;
            bool preserveSourceNormals = sourceNormals != null && sourceNormals.Length == sourceVertices.Length;
            bool preserveSourceUvs = sourceUvs != null && sourceUvs.Length == sourceVertices.Length;

            MeshBuilder positiveBuilder = new MeshBuilder(sourceMesh.subMeshCount, preserveSourceNormals);
            MeshBuilder negativeBuilder = new MeshBuilder(sourceMesh.subMeshCount, preserveSourceNormals);
            List<Vector3> capPoints = new List<Vector3>();

            for (int subMeshIndex = 0; subMeshIndex < sourceMesh.subMeshCount; subMeshIndex++)
            {
                int[] triangles = sourceMesh.GetTriangles(subMeshIndex);
                for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex += 3)
                {
                    VertexData vertexA = GetVertexData(triangles[triangleIndex], sourceVertices, sourceNormals, sourceUvs, preserveSourceNormals, preserveSourceUvs);
                    VertexData vertexB = GetVertexData(triangles[triangleIndex + 1], sourceVertices, sourceNormals, sourceUvs, preserveSourceNormals, preserveSourceUvs);
                    VertexData vertexC = GetVertexData(triangles[triangleIndex + 2], sourceVertices, sourceNormals, sourceUvs, preserveSourceNormals, preserveSourceUvs);

                    float distanceA = slicePlane.GetDistanceToPoint(vertexA.Position);
                    float distanceB = slicePlane.GetDistanceToPoint(vertexB.Position);
                    float distanceC = slicePlane.GetDistanceToPoint(vertexC.Position);

                    bool sideA = distanceA >= 0f;
                    bool sideB = distanceB >= 0f;
                    bool sideC = distanceC >= 0f;

                    VertexData[] vertices = { vertexA, vertexB, vertexC };
                    float[] distances = { distanceA, distanceB, distanceC };
                    bool[] sides = { sideA, sideB, sideC };
                    int positiveCount = (sideA ? 1 : 0) + (sideB ? 1 : 0) + (sideC ? 1 : 0);
                    Vector3 sourceFaceNormal = Vector3.Cross(vertexB.Position - vertexA.Position, vertexC.Position - vertexA.Position);

                    if (positiveCount == 0)
                    {
                        negativeBuilder.AddTriangle(vertexA, vertexB, vertexC, subMeshIndex, sourceFaceNormal);
                        continue;
                    }

                    if (positiveCount == 3)
                    {
                        positiveBuilder.AddTriangle(vertexA, vertexB, vertexC, subMeshIndex, sourceFaceNormal);
                        continue;
                    }

                    if (positiveCount == 1)
                    {
                        int positiveIndex = Array.FindIndex(sides, side => side);
                        int negativeIndexA = (positiveIndex + 1) % 3;
                        int negativeIndexB = (positiveIndex + 2) % 3;
                        if (sides[negativeIndexA])
                            (negativeIndexA, negativeIndexB) = (negativeIndexB, negativeIndexA);

                        SliceSinglePositiveTriangle(
                            vertices[positiveIndex],
                            distances[positiveIndex],
                            vertices[negativeIndexA],
                            distances[negativeIndexA],
                            vertices[negativeIndexB],
                            distances[negativeIndexB],
                            subMeshIndex,
                            sourceFaceNormal,
                            positiveBuilder,
                            negativeBuilder,
                            capPoints);
                    }
                    else
                    {
                        int negativeIndex = Array.FindIndex(sides, side => !side);
                        int positiveIndexA = (negativeIndex + 1) % 3;
                        int positiveIndexB = (negativeIndex + 2) % 3;
                        if (!sides[positiveIndexA])
                            (positiveIndexA, positiveIndexB) = (positiveIndexB, positiveIndexA);

                        SliceSingleNegativeTriangle(
                            vertices[negativeIndex],
                            distances[negativeIndex],
                            vertices[positiveIndexA],
                            distances[positiveIndexA],
                            vertices[positiveIndexB],
                            distances[positiveIndexB],
                            subMeshIndex,
                            sourceFaceNormal,
                            positiveBuilder,
                            negativeBuilder,
                            capPoints);
                    }
                }
            }

            if (capPoints.Count < 3)
                return false;

            BuildCapGeometry(positiveBuilder, negativeBuilder, capPoints, slicePlane.normal);

            Mesh positiveMesh = positiveBuilder.ToMesh($"{sourceMesh.name}_Positive");
            Mesh negativeMesh = negativeBuilder.ToMesh($"{sourceMesh.name}_Negative");
            if (positiveMesh == null || negativeMesh == null)
            {
                if (positiveMesh != null)
                    UnityEngine.Object.Destroy(positiveMesh);
                if (negativeMesh != null)
                    UnityEngine.Object.Destroy(negativeMesh);
                return false;
            }

            Material[] positiveMaterials = BuildMaterials(sourceMaterials, positiveBuilder.RequiresCapSubmesh, cutSurfaceMaterial);
            Material[] negativeMaterials = BuildMaterials(sourceMaterials, negativeBuilder.RequiresCapSubmesh, cutSurfaceMaterial);

            sliceResult = new SliceResult(positiveMesh, negativeMesh, positiveMaterials, negativeMaterials);
            return true;
        }

        private static VertexData GetVertexData(
            int vertexIndex,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector2> uvs,
            bool preserveSourceNormals,
            bool preserveSourceUvs)
        {
            Vector3 normal = preserveSourceNormals ? normals[vertexIndex] : Vector3.zero;
            Vector2 uv = preserveSourceUvs ? uvs[vertexIndex] : Vector2.zero;
            return new VertexData(vertices[vertexIndex], normal, uv);
        }

        private static void SliceSinglePositiveTriangle(
            VertexData positiveVertex,
            float positiveDistance,
            VertexData negativeVertexA,
            float negativeDistanceA,
            VertexData negativeVertexB,
            float negativeDistanceB,
            int subMeshIndex,
            Vector3 sourceFaceNormal,
            MeshBuilder positiveBuilder,
            MeshBuilder negativeBuilder,
            List<Vector3> capPoints)
        {
            VertexData intersectionA = Intersect(positiveVertex, negativeVertexA, positiveDistance, negativeDistanceA);
            VertexData intersectionB = Intersect(positiveVertex, negativeVertexB, positiveDistance, negativeDistanceB);

            positiveBuilder.AddTriangle(positiveVertex, intersectionA, intersectionB, subMeshIndex, sourceFaceNormal);
            negativeBuilder.AddTriangle(negativeVertexA, negativeVertexB, intersectionB, subMeshIndex, sourceFaceNormal);
            negativeBuilder.AddTriangle(negativeVertexA, intersectionB, intersectionA, subMeshIndex, sourceFaceNormal);

            AddUniqueCapPoint(capPoints, intersectionA.Position);
            AddUniqueCapPoint(capPoints, intersectionB.Position);
        }

        private static void SliceSingleNegativeTriangle(
            VertexData negativeVertex,
            float negativeDistance,
            VertexData positiveVertexA,
            float positiveDistanceA,
            VertexData positiveVertexB,
            float positiveDistanceB,
            int subMeshIndex,
            Vector3 sourceFaceNormal,
            MeshBuilder positiveBuilder,
            MeshBuilder negativeBuilder,
            List<Vector3> capPoints)
        {
            VertexData intersectionA = Intersect(negativeVertex, positiveVertexA, negativeDistance, positiveDistanceA);
            VertexData intersectionB = Intersect(negativeVertex, positiveVertexB, negativeDistance, positiveDistanceB);

            negativeBuilder.AddTriangle(negativeVertex, intersectionA, intersectionB, subMeshIndex, sourceFaceNormal);
            positiveBuilder.AddTriangle(positiveVertexA, positiveVertexB, intersectionB, subMeshIndex, sourceFaceNormal);
            positiveBuilder.AddTriangle(positiveVertexA, intersectionB, intersectionA, subMeshIndex, sourceFaceNormal);

            AddUniqueCapPoint(capPoints, intersectionA.Position);
            AddUniqueCapPoint(capPoints, intersectionB.Position);
        }

        private static VertexData Intersect(VertexData from, VertexData to, float fromDistance, float toDistance)
        {
            float interpolation = fromDistance / (fromDistance - toDistance);
            Vector3 normal = Vector3.Lerp(from.Normal, to.Normal, interpolation);
            return new VertexData(
                Vector3.Lerp(from.Position, to.Position, interpolation),
                normal.sqrMagnitude > Epsilon ? normal.normalized : normal,
                Vector2.Lerp(from.Uv, to.Uv, interpolation));
        }

        private static void BuildCapGeometry(
            MeshBuilder positiveBuilder,
            MeshBuilder negativeBuilder,
            List<Vector3> capPoints,
            Vector3 sliceNormal)
        {
            List<Vector3> orderedPoints = BuildOrderedCapPolygon(capPoints, sliceNormal);
            if (orderedPoints.Count < 3)
                return;

            Vector3 center = Vector3.zero;
            foreach (Vector3 point in orderedPoints)
                center += point;
            center /= orderedPoints.Count;

            Vector3 axisX = Vector3.Cross(sliceNormal, Vector3.up);
            if (axisX.sqrMagnitude < Epsilon)
                axisX = Vector3.Cross(sliceNormal, Vector3.right);
            axisX.Normalize();
            Vector3 axisY = Vector3.Cross(sliceNormal, axisX).normalized;

            VertexData positiveCenter = new VertexData(center, -sliceNormal, ProjectCapUv(center, center, axisX, axisY));
            VertexData negativeCenter = new VertexData(center, sliceNormal, ProjectCapUv(center, center, axisX, axisY));

            for (int pointIndex = 0; pointIndex < orderedPoints.Count; pointIndex++)
            {
                Vector3 current = orderedPoints[pointIndex];
                Vector3 next = orderedPoints[(pointIndex + 1) % orderedPoints.Count];

                VertexData positiveCurrent = new VertexData(current, -sliceNormal, ProjectCapUv(current, center, axisX, axisY));
                VertexData positiveNext = new VertexData(next, -sliceNormal, ProjectCapUv(next, center, axisX, axisY));
                VertexData negativeCurrent = new VertexData(current, sliceNormal, ProjectCapUv(current, center, axisX, axisY));
                VertexData negativeNext = new VertexData(next, sliceNormal, ProjectCapUv(next, center, axisX, axisY));

                positiveBuilder.AddCapTriangle(positiveCenter, positiveNext, positiveCurrent, -sliceNormal);
                negativeBuilder.AddCapTriangle(negativeCenter, negativeCurrent, negativeNext, sliceNormal);
            }
        }

        private static List<Vector3> BuildOrderedCapPolygon(List<Vector3> capPoints, Vector3 sliceNormal)
        {
            List<Vector3> uniquePoints = new List<Vector3>();
            foreach (Vector3 point in capPoints)
                AddUniqueCapPoint(uniquePoints, point);

            Vector3 center = Vector3.zero;
            foreach (Vector3 point in uniquePoints)
                center += point;
            center /= Mathf.Max(1, uniquePoints.Count);

            Vector3 axisX = Vector3.Cross(sliceNormal, Vector3.up);
            if (axisX.sqrMagnitude < Epsilon)
                axisX = Vector3.Cross(sliceNormal, Vector3.right);
            axisX.Normalize();
            Vector3 axisY = Vector3.Cross(sliceNormal, axisX).normalized;

            uniquePoints.Sort((left, right) =>
            {
                Vector3 leftOffset = left - center;
                Vector3 rightOffset = right - center;
                float leftAngle = Mathf.Atan2(Vector3.Dot(leftOffset, axisY), Vector3.Dot(leftOffset, axisX));
                float rightAngle = Mathf.Atan2(Vector3.Dot(rightOffset, axisY), Vector3.Dot(rightOffset, axisX));
                return leftAngle.CompareTo(rightAngle);
            });

            return uniquePoints;
        }

        private static Vector2 ProjectCapUv(Vector3 point, Vector3 center, Vector3 axisX, Vector3 axisY)
        {
            Vector3 offset = point - center;
            return new Vector2(Vector3.Dot(offset, axisX), Vector3.Dot(offset, axisY));
        }

        private static Material[] BuildMaterials(Material[] sourceMaterials, bool requiresCapSubmesh, Material cutSurfaceMaterial)
        {
            sourceMaterials ??= Array.Empty<Material>();

            int sourceCount = Mathf.Max(1, sourceMaterials.Length);
            int extraSubmeshCount = requiresCapSubmesh ? 1 : 0;
            Material fallbackMaterial = sourceMaterials.Length > 0
                ? sourceMaterials[sourceMaterials.Length - 1]
                : cutSurfaceMaterial;

            Material[] materials = new Material[sourceCount + extraSubmeshCount];
            for (int index = 0; index < sourceCount; index++)
            {
                materials[index] = sourceMaterials.Length > index
                    ? sourceMaterials[index]
                    : fallbackMaterial;
            }

            if (requiresCapSubmesh)
                materials[materials.Length - 1] = cutSurfaceMaterial != null ? cutSurfaceMaterial : fallbackMaterial;

            return materials;
        }

        private static void AddUniqueCapPoint(List<Vector3> capPoints, Vector3 point)
        {
            foreach (Vector3 existingPoint in capPoints)
            {
                if ((existingPoint - point).sqrMagnitude <= Epsilon * Epsilon)
                    return;
            }

            capPoints.Add(point);
        }

        private readonly struct VertexData
        {
            public VertexData(Vector3 position, Vector3 normal, Vector2 uv)
            {
                Position = position;
                Normal = normal;
                Uv = uv;
            }

            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public Vector2 Uv { get; }
        }

        private sealed class MeshBuilder
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<Vector3> _normals = new List<Vector3>();
            private readonly List<Vector2> _uvs = new List<Vector2>();
            private readonly List<List<int>> _subMeshTriangles = new List<List<int>>();
            private readonly List<int> _capTriangles = new List<int>();
            private readonly bool _preserveSourceNormals;

            public MeshBuilder(int subMeshCount, bool preserveSourceNormals)
            {
                _preserveSourceNormals = preserveSourceNormals;

                for (int index = 0; index < subMeshCount; index++)
                    _subMeshTriangles.Add(new List<int>());
            }

            public bool RequiresCapSubmesh => _capTriangles.Count > 0;

            public void AddTriangle(VertexData a, VertexData b, VertexData c, int subMeshIndex, Vector3 expectedNormal)
            {
                AddTriangleInternal(_subMeshTriangles[subMeshIndex], a, b, c, expectedNormal);
            }

            public void AddCapTriangle(VertexData a, VertexData b, VertexData c, Vector3 expectedNormal)
            {
                AddTriangleInternal(_capTriangles, a, b, c, expectedNormal);
            }

            public Mesh ToMesh(string meshName)
            {
                if (_vertices.Count < 3)
                    return null;

                Mesh mesh = new Mesh
                {
                    name = meshName,
                    indexFormat = _vertices.Count > ushort.MaxValue
                        ? UnityEngine.Rendering.IndexFormat.UInt32
                        : UnityEngine.Rendering.IndexFormat.UInt16
                };

                mesh.SetVertices(_vertices);
                mesh.SetUVs(0, _uvs);

                if (_preserveSourceNormals)
                    mesh.SetNormals(_normals);

                int subMeshCount = _subMeshTriangles.Count + (RequiresCapSubmesh ? 1 : 0);
                mesh.subMeshCount = subMeshCount;

                for (int index = 0; index < _subMeshTriangles.Count; index++)
                    mesh.SetTriangles(_subMeshTriangles[index], index, true);

                if (RequiresCapSubmesh)
                    mesh.SetTriangles(_capTriangles, subMeshCount - 1, true);

                mesh.RecalculateBounds();
                if (!_preserveSourceNormals)
                    mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                return mesh;
            }

            private void AddTriangleInternal(List<int> triangleBuffer, VertexData a, VertexData b, VertexData c, Vector3 expectedNormal)
            {
                if ((b.Position - a.Position).sqrMagnitude <= Epsilon * Epsilon ||
                    (c.Position - a.Position).sqrMagnitude <= Epsilon * Epsilon ||
                    (c.Position - b.Position).sqrMagnitude <= Epsilon * Epsilon)
                {
                    return;
                }

                Vector3 triangleNormal = Vector3.Cross(b.Position - a.Position, c.Position - a.Position);
                if (triangleNormal.sqrMagnitude <= Epsilon * Epsilon)
                    return;

                if (expectedNormal.sqrMagnitude > Epsilon * Epsilon && Vector3.Dot(triangleNormal, expectedNormal) < 0f)
                {
                    (b, c) = (c, b);
                }

                int startIndex = _vertices.Count;
                AddVertex(a);
                AddVertex(b);
                AddVertex(c);

                triangleBuffer.Add(startIndex);
                triangleBuffer.Add(startIndex + 1);
                triangleBuffer.Add(startIndex + 2);
            }

            private void AddVertex(VertexData vertex)
            {
                _vertices.Add(vertex.Position);
                _uvs.Add(vertex.Uv);

                if (_preserveSourceNormals)
                    _normals.Add(vertex.Normal);
            }
        }
    }
}
