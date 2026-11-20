namespace Periodiq.Interaction
{
    /// <summary>Anything in AR with a collider that should react to a simple tap (not a grab).</summary>
    public interface ITappable
    {
        void OnTap();
    }
}
