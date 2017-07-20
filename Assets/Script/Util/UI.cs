using UnityEngine;

public class UI : MonoBehaviour
{
    private GameObject caching_gameobject;
    private Transform caching_transform;

    public GameObject CachingGameObject
    {
        get
        {
            if (caching_gameobject == null)
                caching_gameobject = gameObject;

            return caching_gameobject;
        }
    }

    public Transform CachingTransform
    {
        get
        {
            if (caching_transform == null)
                caching_transform = transform;

            return caching_transform;
        }
    }

    #region MonoBehaviour Function

    void Awake()
    {
        OnAwake();
    }

    #endregion

    protected virtual void OnAwake() { }

    public virtual void Show()
    {
        CachingGameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        CachingGameObject.SetActive(false);
    }
}
