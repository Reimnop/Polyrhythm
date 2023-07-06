using OpenTK.Mathematics;
using Polyrhythm.Animation;

namespace Polyrhythm.Data;

public class ModelNodeAnimationChannel
{
    public string NodeName { get; }
    public IReadOnlyList<Key<Vector3d>> PositionKeys => positionKeys;
    public IReadOnlyList<Key<Vector3d>> ScaleKeys => scaleKeys;
    public IReadOnlyList<Key<Quaterniond>> RotationKeys => rotationKeys;

    private readonly List<Key<Vector3d>> positionKeys;
    private readonly List<Key<Vector3d>> scaleKeys;
    private readonly List<Key<Quaterniond>> rotationKeys;

    public ModelNodeAnimationChannel(string nodeName, IEnumerable<Key<Vector3d>> positionKeys, IEnumerable<Key<Vector3d>> scaleKeys, IEnumerable<Key<Quaterniond>> rotationKeys)
    {
        NodeName = nodeName;
        this.positionKeys = positionKeys.ToList();
        this.scaleKeys = scaleKeys.ToList();
        this.rotationKeys = rotationKeys.ToList();
    }
}