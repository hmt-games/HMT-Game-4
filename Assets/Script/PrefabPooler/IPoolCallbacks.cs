using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolCallbacks
{
    public void OnInstantiateFromPool();

    public void OnReleaseToPool();
}
