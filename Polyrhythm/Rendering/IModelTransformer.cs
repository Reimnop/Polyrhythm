using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Rendering;

public interface IModelTransformer
{
    Matrix4d GetNodeTransform(ModelNode node);
}