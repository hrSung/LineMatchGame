public abstract class PoolItem : UI
{

    #region Override Function

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    #endregion

    public virtual void ResetItem() { }
}
