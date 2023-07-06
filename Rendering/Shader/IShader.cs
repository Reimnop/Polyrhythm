namespace Polyrhythm.Rendering.Shader;

public interface IShader<in TInput, out TOutput> 
    where TInput : unmanaged
    where TOutput : unmanaged
{
    TOutput Process(TInput input);
}