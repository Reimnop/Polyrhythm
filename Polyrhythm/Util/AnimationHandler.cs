using OpenTK.Mathematics;
using Polyrhythm.Animation;
using Polyrhythm.Data;
using Polyrhythm.Rendering;

namespace Polyrhythm.Util;

public class AnimationHandler : IModelTransformer
{
    private class TransformableNode
    {
        public Vector3d Position { get; set; }
        public Vector3d Scale { get; set; }
        public Quaterniond Rotation { get; set; }

        private readonly ModelNode node;
        
        public TransformableNode(ModelNode node)
        {
            this.node = node;
            ResetTransformation();
        }

        public void ResetTransformation()
        {
            var transform = node.Transform;
            Position = transform.ExtractTranslation();
            Scale = transform.ExtractScale();
            Rotation = transform.ExtractRotation();
        }
    }
    
    private Node<ModelNode> rootModelNode;
    private Dictionary<string, TransformableNode> nodes;

    private ModelAnimation? animation;

    public AnimationHandler(Model model)
    {
        rootModelNode = model.RootNode;
        nodes = model.RootNode.ToDictionary(node => node.Value.Name, node => new TransformableNode(node.Value));
    }
    
    public void Transition(ModelAnimation? animation)
    {
        this.animation = animation;
        
        if (this.animation == null)
            foreach (var node in nodes.Values)
                node.ResetTransformation();
    }

    public void Update(double time)
    {
        if (animation == null) 
            return;

        var t = (time * animation.TicksPerSecond) % animation.DurationInTicks;

        foreach (var (name, node) in nodes)
        {
            var channel = animation.GetAnimationChannelFromName(name);
            if (channel == null) 
                continue;
            
            node.Position = Sequence.Interpolate(t, channel.PositionKeys, Vector3Lerp);
            node.Scale = Sequence.Interpolate(t, channel.ScaleKeys, Vector3Lerp);
            node.Rotation = Sequence.Interpolate(t, channel.RotationKeys, Quaterniond.Slerp);
        }
    }

    private static Vector3d Vector3Lerp(Vector3d a, Vector3d b, double t)
    {
        return new Vector3d(
                MathHelper.Lerp(a.X, b.X, t), 
                MathHelper.Lerp(a.Y, b.Y, t), 
                MathHelper.Lerp(a.Z, b.Z, t));
    }

    public Matrix4d GetNodeTransform(ModelNode node)
    {
        var transformableNode = nodes[node.Name];
        return Matrix4d.Scale(transformableNode.Scale) * 
               Matrix4d.CreateFromQuaternion(transformableNode.Rotation) *
               Matrix4d.CreateTranslation(transformableNode.Position);
    }
}